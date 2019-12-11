using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GitSearch2.Shared;
using Microsoft.Extensions.Options;

namespace GitSearch2.Repository.SqlServer {
	public sealed class CommitSqlServerRepository : SqlServerRepository, ICommitRepository {

		private readonly string[] EnvironmentNewLine = new string[] { Environment.NewLine };
		private const string SchemaId = "b914f541f1564495802d10f5ee208a41";
		private const int TargetSchema = 1;

		public CommitSqlServerRepository( IOptions<SqlServerOptions> options )
			: this( options.Value ) {
		}

		public CommitSqlServerRepository( SqlServerOptions options ) :
			base( options ) {
		}

		void ICommitRepository.Initialize() {
			Initialize( SchemaId, TargetSchema );
		}

		protected override void CreateSchema() {
			string sql = @"
				CREATE TABLE GIT_COMMIT
				(
					COMMIT_ID NVARCHAR(64) NOT NULL,
					PROJECT NVARCHAR(128) NOT NULL,
					REPO NVARCHAR(128) NOT NULL,
					DESCRIPTION TEXT NOT NULL,
					AUTHOR_EMAIL NVARCHAR(128) NOT NULL,
					AUTHOR_NAME NVARCHAR(128) NOT NULL,
					COMMIT_DATE DATETIME2 NOT NULL,
					FILES TEXT NOT NULL,
					PR_NUMBER NVARCHAR(32) NOT NULL,
					MERGE_COMMITS TEXT NOT NULL,
					CONSTRAINT PK_GIT_COMMIT PRIMARY KEY(COMMIT_ID, PROJECT, REPO)
				)
			;";
			ExecuteNonQuery( sql );

			sql = @"
				CREATE INDEX
					IDX_EXISTS
				ON
					GIT_COMMIT (
						COMMIT_ID,
						PROJECT,
						REPO
					)
			;";
			ExecuteNonQuery( sql );

			sql = @"
				CREATE UNIQUE INDEX
					IDX_COMMIT_ID
				ON
					GIT_COMMIT (
						COMMIT_ID
					)
			;";
			ExecuteNonQuery( sql );

			sql = @"
				CREATE FULLTEXT CATALOG
					COMMIT_CATALOG
				WITH
					ACCENT_SENSITIVITY = OFF
				AS DEFAULT
			;";
			ExecuteNonQuery( sql );

			sql = @"
				CREATE FULLTEXT INDEX ON
					GIT_COMMIT(
						COMMIT_ID,
						PROJECT,
						REPO,
						DESCRIPTION,
						AUTHOR_NAME,
						AUTHOR_EMAIL,
						FILES,
						MERGE_COMMITS
					)
				KEY INDEX
					IDX_COMMIT_ID
					ON COMMIT_CATALOG
				WITH
					CHANGE_TRACKING = AUTO
			;";

			ExecuteNonQuery( sql );
		}

		protected override void UpdateSchema( int targetSchema ) {
			throw new InvalidOperationException();
		}

		int ICommitRepository.CountCommits() {
			string sql = @"
				SELECT
					COUNT(*)
				FROM
					GIT_COMMIT
			;";

			var parameters = new Dictionary<string, object>();

			return ExecuteSingleReader( sql, parameters, LoadInt );
		}

		async Task<int> ICommitRepository.CountCommitsAsync() {
			string sql = @"
				SELECT
					COUNT(*)
				FROM
					GIT_COMMIT
			;";

			var parameters = new Dictionary<string, object>();

			return await ExecuteSingleReaderAsync( sql, parameters, LoadInt );
		}

		IEnumerable<CommitDetails> ICommitRepository.Search(
			string term,
			int limit
		) {
			string sql = @"
				SELECT
					GC.COMMIT_ID,
					GC.PROJECT,
					GC.REPO,
					GC.DESCRIPTION,
					GC.AUTHOR_EMAIL,
					GC.AUTHOR_NAME,
					GC.COMMIT_DATE,
					GC.FILES,
					GC.PR_NUMBER,
					GC.MERGE_COMMITS
				FROM 
					GIT_COMMIT AS GC
				WHERE 
					CONTAINS( COMMIT_ID, @term )
					OR CONTAINS( DESCRIPTION, @term )
					OR CONTAINS( AUTHOR_EMAIL, @term )
					OR CONTAINS( AUTHOR_NAME, @term )
					OR CONTAINS( FILES, @term )
				ORDER BY
					GC.COMMIT_DATE DESC
				OFFSET 0 ROWS
				FETCH NEXT @limit ROWS ONLY
			;";

			var parameters = new Dictionary<string, object>() {
				{ "@term", term },
				{ "@limit", limit }
			};

			return ExecuteReader( sql, parameters, ReadCommit );
		}

		async Task<IEnumerable<CommitDetails>> ICommitRepository.SearchAsync(
			string term,
			int limit
		) {
			string sql = @"
				SELECT
					GC.COMMIT_ID,
					GC.PROJECT,
					GC.REPO,
					GC.DESCRIPTION,
					GC.AUTHOR_EMAIL,
					GC.AUTHOR_NAME,
					GC.COMMIT_DATE,
					GC.FILES,
					GC.PR_NUMBER,
					GC.MERGE_COMMITS
				FROM 
					GIT_COMMIT AS GC
				WHERE 
					CONTAINS( COMMIT_ID, @term )
					OR CONTAINS( DESCRIPTION, @term )
					OR CONTAINS( AUTHOR_EMAIL, @term )
					OR CONTAINS( AUTHOR_NAME, @term )
					OR CONTAINS( FILES, @term )
				ORDER BY
					GC.COMMIT_DATE DESC
				OFFSET 0 ROWS
				FETCH NEXT @limit ROWS ONLY
			;";

			var parameters = new Dictionary<string, object>() {
				{ "@term", term },
				{ "@limit", limit }
			};

			return await ExecuteReaderAsync( sql, parameters, ReadCommit );
		}

		bool ICommitRepository.ContainsCommit(
			string commitId,
			string project,
			string repo
		) {
			string sql = @"
				SELECT
					1
				FROM
					GIT_COMMIT
				WHERE
					COMMIT_ID = @commitId
					AND PROJECT = @project
					AND REPO = @repo
			;";

			var parameters = new Dictionary<string, object>() {
				{ "@commitId", commitId },
				{ "@project", project },
				{ "@repo", repo }
			};

			int hasRow = ExecuteSingleReader( sql, parameters, LoadInt );

			return ( hasRow == 1 );
		}

		async Task<bool> ICommitRepository.ContainsCommitAsync(
			string commitId,
			string project,
			string repo
		) {
			string sql = @"
				SELECT
					1
				FROM
					GIT_COMMIT
				WHERE
					COMMIT_ID = @commitId
					AND PROJECT = @project
					AND REPO = @repo
			;";

			var parameters = new Dictionary<string, object>() {
				{ "@commitId", commitId },
				{ "@project", project },
				{ "@repo", repo }
			};

			int hasRow = await ExecuteSingleReaderAsync( sql, parameters, LoadInt );

			return ( hasRow == 1 );
		}

		void ICommitRepository.Add( CommitDetails commit ) {
			string description = commit.Description.Any() ? commit.Description.Aggregate( ( current, next ) => current + Environment.NewLine + next ) : string.Empty;
			string files = commit.Files.Any() ? commit.Files.Aggregate( ( current, next ) => current + Environment.NewLine + next ) : string.Empty;
			string mergeCommits = commit.Commits.Any() ? commit.Commits.Aggregate( ( current, next ) => current + Environment.NewLine + next ) : string.Empty;
			DateTime commitDate = DateTime.ParseExact( commit.Date, "yyyyMMddTHHmmssfffffffZ", CultureInfo.InvariantCulture ).ToUniversalTime();

			var parameters = new Dictionary<string, object>() {
				{ "@commitId", commit.CommitId },
				{ "@description", description },
				{ "@repo", commit.Repo },
				{ "@authorEmail", commit.AuthorEmail },
				{ "@authorName", commit.AuthorName },
				{ "@commitDate", ToText(commitDate) },
				{ "@files", files },
				{ "@project", commit.Project },
				{ "@prNumber", commit.PR },
				{ "@mergeCommits", mergeCommits }
			};

			string sql = @"
				INSERT INTO GIT_COMMIT 
				(
					COMMIT_ID,
					PROJECT,
					REPO,
					DESCRIPTION,
					AUTHOR_EMAIL,
					AUTHOR_NAME,
					COMMIT_DATE,
					FILES,
					PR_NUMBER,
					MERGE_COMMITS
				)
				VALUES
				(
					@commitId,
					@project,
					@repo,
					@description,
					@authorEmail,
					@authorName,
					@commitDate,
					@files,
					@prNumber,
					@mergeCommits
				)
			;";

			ExecuteNonQuery( sql, parameters );
		}

		async Task ICommitRepository.AddAsync( CommitDetails commit ) {
			string description = commit.Description.Any() ? commit.Description.Aggregate( ( current, next ) => current + Environment.NewLine + next ) : string.Empty;
			string files = commit.Files.Any() ? commit.Files.Aggregate( ( current, next ) => current + Environment.NewLine + next ) : string.Empty;
			string mergeCommits = commit.Commits.Any() ? commit.Commits.Aggregate( ( current, next ) => current + Environment.NewLine + next ) : string.Empty;
			DateTime commitDate = DateTime.ParseExact( commit.Date, "yyyyMMddTHHmmssfffffffZ", CultureInfo.InvariantCulture ).ToUniversalTime();

			var parameters = new Dictionary<string, object>() {
				{ "@commitId", commit.CommitId },
				{ "@description", description },
				{ "@repo", commit.Repo },
				{ "@authorEmail", commit.AuthorEmail },
				{ "@authorName", commit.AuthorName },
				{ "@commitDate", ToText(commitDate) },
				{ "@files", files },
				{ "@project", commit.Project },
				{ "@prNumber", commit.PR },
				{ "@mergeCommits", mergeCommits }
			};

			string sql = @"
				INSERT INTO GIT_COMMIT 
				(
					COMMIT_ID,
					PROJECT,
					REPO,
					DESCRIPTION,
					AUTHOR_EMAIL,
					AUTHOR_NAME,
					COMMIT_DATE,
					FILES,
					PR_NUMBER,
					MERGE_COMMITS
				)
				VALUES
				(
					@commitId,
					@project,
					@repo,
					@description,
					@authorEmail,
					@authorName,
					@commitDate,
					@files,
					@prNumber,
					@mergeCommits
				)
			;";

			await ExecuteNonQueryAsync( sql, parameters );
		}

		private CommitDetails ReadCommit( DbDataReader reader ) {
			string dbCommitId = GetString( reader, "COMMIT_ID" );
			string dbDescription = GetString( reader, "DESCRIPTION" );
			string dbRepo = GetString( reader, "REPO" );
			string dbAuthorEmail = GetString( reader, "AUTHOR_EMAIL" );
			string dbAuthorName = GetString( reader, "AUTHOR_NAME" );
			DateTime dbCommitDate = GetDateTime( reader, "COMMIT_DATE" );
			string dbFiles = GetString( reader, "FILES" );
			string dbProject = GetString( reader, "PROJECT" );
			string dbPrNumber = GetString( reader, "PR_NUMBER" );
			string dbCommits = GetString( reader, "MERGE_COMMITS" );

			return new CommitDetails(
				authorEmail: dbAuthorEmail,
				authorName: dbAuthorName,
				commitId: dbCommitId,
				date: dbCommitDate.ToString( "yyyy-MM-ddTHH:mm:ssZ" ), //ISO8601
				description: dbDescription.Split( EnvironmentNewLine, StringSplitOptions.None ),
				files: dbFiles.Split( EnvironmentNewLine, StringSplitOptions.None ),
				repo: dbRepo,
				project: dbProject,
				pr: dbPrNumber,
				commits: dbCommits.Split( EnvironmentNewLine, StringSplitOptions.None ),
				isMerge: false
			);
		}
	}
}