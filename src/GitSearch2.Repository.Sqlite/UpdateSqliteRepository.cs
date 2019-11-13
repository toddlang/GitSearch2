using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Extensions.Options;

namespace GitSearch2.Repository.Sqlite {
	public sealed class UpdateSqliteRepository : SqliteRepository, IUpdateRepository {

		private const string SchemaId = "15c8b53ab898475497ad37cf968b93aa";
		private const int TargetSchema = 1;

		public UpdateSqliteRepository( IOptions<SqliteOptions> options )
			: this( options.Value ) {
		}

		public UpdateSqliteRepository( SqliteOptions options ):
			base( options ) {
		}

		void IUpdateRepository.Initialize() {
			Initialize( SchemaId, TargetSchema );
		}

		protected override void CreateSchema() {
			string sql = @"
				CREATE TABLE GIT_UPDATE
				(
					SESSION NVARCHAR(32) NOT NULL,
					PROJECT NVARCHAR(128) NOT NULL,
					REPO NVARCHAR(128) NOT NULL,
					PROGRESS INTEGER NOT NULL,
					STARTED TEXT NOT NULL,
					FINISHED TEXT,
					LAST_COMMIT_ID NVARCHAR(64),
					PRIMARY KEY(SESSION, PROJECT, REPO)
				)
			;";

			ExecuteNonQuery( sql );
		}

		protected override void UpdateSchema( int targetSchema ) {
			throw new InvalidOperationException();
		}

		IEnumerable<RepoProgress> IUpdateRepository.GetProgress(
			Guid session
		) {
			string sql = @"
				SELECT
					PROJECT,
					REPO,
					PROGRESS,
					STARTED,
					FINISHED,
					LAST_COMMIT_ID
				FROM
					GIT_UPDATE
				WHERE
					SESSION = @session
			;";

			var parameters = new Dictionary<string, object>() {
				{ "@session", session.ToString("N") }
			};

			return ExecuteReader( sql, parameters, ReadProgress );
		}

		void IUpdateRepository.Begin(
			Guid session,
			string repo,
			string project,
			DateTimeOffset started
		) {
			string sql = @"
				INSERT INTO GIT_UPDATE
				(
					SESSION,
					PROJECT,
					REPO,
					PROGRESS,
					STARTED
				)
				VALUES
				(
					@session,
					@project,
					@repo,
					@progress,
					@started
				)
			;";

			var parameters = new Dictionary<string, object>() {
				{ "@session", session.ToString("N") },
				{ "@project", project },
				{ "@repo", repo },
				{ "@progress", 0 },
				{ "@started", ToText(started) }
			};

			ExecuteNonQuery( sql, parameters );
		}

		void IUpdateRepository.End(
			Guid session,
			string repo,
			string project,
			DateTimeOffset finished,
			string lastCommitId
		) {
			string sql = @"
				UPDATE GIT_UPDATE
				SET
					FINISHED = @finished,
					PROGRESS = @progress,
					LAST_COMMIT_ID = @lastCommitId
				WHERE
					SESSION = @session
					AND PROJECT = @project
					AND REPO = @repo
			;";

			var parameters = new Dictionary<string, object>() {
				{ "@session", session.ToString("N") },
				{ "@project", project },
				{ "@repo", repo },
				{ "@finished", ToText(finished) },
				{ "@progress", 100 },
				{ "@lastCommitId", lastCommitId }
			};

			ExecuteNonQuery( sql, parameters );
		}

		void IUpdateRepository.SetProgress(
			Guid session,
			string repo,
			string project,
			int progress
		) {
			string sql = @"
				UPDATE GIT_UPDATE
				SET
					PROGRESS = @progress
				WHERE
					SESSION = @session
					AND PROJECT = @project
					AND REPO = @repo
			;";

			var parameters = new Dictionary<string, object>() {
				{ "@session", session.ToString("N") },
				{ "@project", project },
				{ "@repo", repo },
				{ "@progress", progress }
			};

			ExecuteNonQuery( sql, parameters );
		}

		DateTimeOffset IUpdateRepository.GetMostRecentCommit() {
			string sql = @"
				SELECT
					MIN(STARTED) AS LAST_INDEXED
				FROM
					GIT_UPDATE
				WHERE
					SESSION = (
					SELECT
						SESSION
					FROM
						GIT_UPDATE
					GROUP BY
						SESSION
					HAVING
						MAX(FINISHED)
				)
			;";

			var parameters = new Dictionary<string, object>() {
			};

			return ExecuteSingleReader( sql, parameters, LoadDateTimeOffset );
		}

		private RepoProgress ReadProgress( DbDataReader reader ) {
			string dbRepo = GetString( reader, "REPO" );
			string dbProject = GetString( reader, "PROJECT" );
			int dbProgress = GetInt( reader, "PROGRESS" );
			DateTimeOffset dbStarted = GetDateTimeOffset( reader, "STARTED" );
			DateTimeOffset? dbFinished = GetNullableDateTimeOffset( reader, "FINISHED" );
			string dbLastCommitId = GetString( reader, "LAST_COMMIT_ID" );

			return new RepoProgress( dbRepo, dbProject, dbProgress, dbStarted, dbFinished, dbLastCommitId );
		}
	}
}
