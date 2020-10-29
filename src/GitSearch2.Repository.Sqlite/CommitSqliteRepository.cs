using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GitSearch2.Shared;

namespace GitSearch2.Repository.Sqlite {
	public sealed class CommitSqliteRepository : SqliteRepository, ICommitRepository {

		private readonly string[] EnvironmentNewLine = new string[] { Environment.NewLine };
		private const string SchemaId = "b914f541f1564495802d10f5ee208a41";
		private const int TargetSchema = 2;

		public CommitSqliteRepository( IDb db ) :
			base( db ) {
		}

		void ICommitRepository.Initialize() {
			Initialize( SchemaId, TargetSchema );
		}

		protected override void CreateSchema() {
			const string sqlCreateTable = @"
				CREATE TABLE GIT_COMMIT
				(
					COMMIT_ID NVARCHAR(64) NOT NULL,
					PROJECT NVARCHAR(128) NOT NULL,
					REPO NVARCHAR(128) NOT NULL,
					DESCRIPTION TEXT NOT NULL,
					AUTHOR_EMAIL NVARCHAR(128) NOT NULL,
					AUTHOR_NAME NVARCHAR(128) NOT NULL,
					COMMIT_DATE TEXT NOT NULL,
					FILES TEXT NOT NULL,
					PR_NUMBER NVARCHAR(32) NOT NULL,
					MERGE_COMMITS TEXT NOT NULL,
					ORIGIN_ID NVARCHAR(32) NOT NULL,
					PRIMARY KEY(COMMIT_ID, PROJECT, REPO)
				)
			;";
			Db.ExecuteNonQuery( sqlCreateTable );

			const string sqlCreateIndex = @"
				CREATE INDEX
					IDX_COMMIT_ID
				ON
					GIT_COMMIT (
						COMMIT_ID,
						PROJECT,
						REPO
					)
			;";

			Db.ExecuteNonQuery( sqlCreateIndex );

			const string sqlCreateFulltext = @"
				CREATE VIRTUAL TABLE SEARCHABLE_COMMIT USING FTS4(
					COMMIT_ID,
					PROJECT,
					REPO,
					DESCRIPTION,
					AUTHOR_NAME,
					AUTHOR_EMAIL,
					FILES,
					MERGE_COMMITS,
					tokenize=unicode61 ""tokenchars=_.""
				)
			;";

			Db.ExecuteNonQuery( sqlCreateFulltext );
		}

		protected override void UpdateSchema( int targetSchema ) {
			if (targetSchema == 2) {
				const string sqlAddOrigin = @"
					ALTER TABLE GIT_COMMIT
					ADD ORIGIN_ID NVARCHAR(32) NOT NULL
						DEFAULT ""github""
				;";

				Db.ExecuteNonQuery( sqlAddOrigin );
			} else {
				throw new InvalidOperationException();
			}			
		}

		int ICommitRepository.CountCommits() {
			const string sql = @"
				SELECT
					COUNT(*)
				FROM
					GIT_COMMIT
			;";

			var parameters = new Dictionary<string, object>();

			return Db.ExecuteSingleReader( sql, parameters, Db.LoadInt );
		}

		async Task<int> ICommitRepository.CountCommitsAsync() {
			const string sql = @"
				SELECT
					COUNT(*)
				FROM
					GIT_COMMIT
			;";

			var parameters = new Dictionary<string, object>();

			return await Db.ExecuteSingleReaderAsync( sql, parameters, Db.LoadInt );
		}

		IEnumerable<CommitDetails> ICommitRepository.Search(
			string term,
			int limit
		) {
			const string sql = @"
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
					GC.MERGE_COMMITS,
					GC.ORIGIN_ID
				FROM 
					GIT_COMMIT AS GC
					INNER JOIN SEARCHABLE_COMMIT AS SC
						ON GC.COMMIT_ID = SC.COMMIT_ID
						AND GC.REPO = SC.REPO
						AND GC.PROJECT = SC.PROJECT
				WHERE 
					SEARCHABLE_COMMIT MATCH @term
				ORDER BY
					GC.COMMIT_DATE DESC
				LIMIT @limit
			;";

			var parameters = new Dictionary<string, object>() {
				{ "@term", term },
				{ "@limit", limit }
			};

			return Db.ExecuteReader( sql, parameters, ReadCommit );
		}

		async Task<IEnumerable<CommitDetails>> ICommitRepository.SearchAsync(
			string term,
			int limit
		) {
			const string sql = @"
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
					GC.MERGE_COMMITS,
					GC.ORIGIN_ID
				FROM 
					GIT_COMMIT AS GC
					INNER JOIN SEARCHABLE_COMMIT AS SC
						ON GC.COMMIT_ID = SC.COMMIT_ID
						AND GC.REPO = SC.REPO
						AND GC.PROJECT = SC.PROJECT
				WHERE 
					SEARCHABLE_COMMIT MATCH @term
				ORDER BY
					GC.COMMIT_DATE DESC
				LIMIT @limit
			;";

			var parameters = new Dictionary<string, object>() {
				{ "@term", term },
				{ "@limit", limit }
			};

			return await Db.ExecuteReaderAsync( sql, parameters, ReadCommit );
		}

		bool ICommitRepository.ContainsCommit(
			string commitId,
			string project,
			string repo
		) {
			const string sql = @"
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

			int hasRow = Db.ExecuteSingleReader( sql, parameters, Db.LoadInt );

			return ( hasRow == 1 );
		}

		async Task<bool> ICommitRepository.ContainsCommitAsync(
			string commitId,
			string project,
			string repo
		) {
			const string sql = @"
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

			int hasRow = await Db.ExecuteSingleReaderAsync( sql, parameters, Db.LoadInt );

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
				{ "@commitDate", Db.ToText(commitDate) },
				{ "@files", files },
				{ "@project", commit.Project },
				{ "@prNumber", commit.PR },
				{ "@mergeCommits", mergeCommits },
				{ "@originId", commit.OriginId }
			};

			const string sqlInsertCommit = @"
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
					MERGE_COMMITS,
					ORIGIN_ID
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
					@mergeCommits,
					@originId
				)
			;";

			Db.ExecuteNonQuery( sqlInsertCommit, parameters );

			const string sqlInsertFulltext = @"
				INSERT INTO SEARCHABLE_COMMIT
				(
					COMMIT_ID,
					PROJECT,
					REPO,
					DESCRIPTION,
					AUTHOR_NAME,
					AUTHOR_EMAIL,
					FILES,
					MERGE_COMMITS
				)
				VALUES
				(
					@commitId,
					@project,
					@repo,
					@description,
					@authorName,
					@authorEmail,
					@files,
					@mergeCommits
				)
			;";

			Db.ExecuteNonQuery( sqlInsertFulltext, parameters );
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
				{ "@commitDate", Db.ToText(commitDate) },
				{ "@files", files },
				{ "@project", commit.Project },
				{ "@prNumber", commit.PR },
				{ "@mergeCommits", mergeCommits },
				{ "@originId", commit.OriginId }
			};

			const string sqlInsertCommit = @"
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
					MERGE_COMMITS,
					ORIGIN_ID
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
					@mergeCommits,
					@originId
				)
			;";

			await Db.ExecuteNonQueryAsync( sqlInsertCommit, parameters );

			const string sqlInsertFulltext = @"
				INSERT INTO SEARCHABLE_COMMIT
				(
					COMMIT_ID,
					PROJECT,
					REPO,
					DESCRIPTION,
					AUTHOR_NAME,
					AUTHOR_EMAIL,
					FILES,
					MERGE_COMMITS
				)
				VALUES
				(
					@commitId,
					@project,
					@repo,
					@description,
					@authorName,
					@authorEmail,
					@files,
					@mergeCommits
				)
			;";

			await Db.ExecuteNonQueryAsync( sqlInsertFulltext, parameters );
		}

		private CommitDetails ReadCommit( DbDataReader reader ) {
			string dbCommitId = Db.GetString( reader, "COMMIT_ID" );
			string dbDescription = Db.GetString( reader, "DESCRIPTION" );
			string dbRepo = Db.GetString( reader, "REPO" );
			string dbAuthorEmail = Db.GetString( reader, "AUTHOR_EMAIL" );
			string dbAuthorName = Db.GetString( reader, "AUTHOR_NAME" );
			DateTime dbCommitDate = Db.GetDateTime( reader, "COMMIT_DATE" );
			string dbFiles = Db.GetString( reader, "FILES" );
			string dbProject = Db.GetString( reader, "PROJECT" );
			string dbPrNumber = Db.GetString( reader, "PR_NUMBER" );
			string dbCommits = Db.GetString( reader, "MERGE_COMMITS" );
			string originId = Db.GetString( reader, "ORIGIN_ID" );

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
				isMerge: false,
				originId: originId
			);
		}
	}
}
