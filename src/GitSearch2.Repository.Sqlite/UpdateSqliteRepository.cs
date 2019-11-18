using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Extensions.Options;

namespace GitSearch2.Repository.Sqlite {
	public sealed class UpdateSqliteRepository : SqliteRepository, IUpdateRepository {

		private const string SchemaId = "15c8b53ab898475497ad37cf968b93aa";
		private const int TargetSchema = 2;

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
					STARTED TEXT NOT NULL,
					FINISHED TEXT,
					COMMITS_WRITTEN INT,
					PRIMARY KEY(SESSION, PROJECT, REPO)
				)
			;";

			ExecuteNonQuery( sql );
		}

		protected override void UpdateSchema( int targetSchema ) {
			switch (targetSchema) {
				case 2: {
						string sql = @"
							ALTER TABLE
								GIT_UPDATE
							RENAME TO GIT_UPDATE_OLD
						;";
						ExecuteNonQuery( sql );

						sql = @"
							CREATE TABLE GIT_UPDATE
							(
								SESSION NVARCHAR(32) NOT NULL,
								PROJECT NVARCHAR(128) NOT NULL,
								REPO NVARCHAR(128) NOT NULL,
								STARTED TEXT NOT NULL,
								FINISHED TEXT,
								COMMITS_WRITTEN INT,
								PRIMARY KEY(SESSION, PROJECT, REPO)
							)
						;";

						ExecuteNonQuery( sql );

						sql = @"
							INSERT INTO GIT_UPDATE
								(SESSION, PROJECT, REPO, STARTED, FINISHED)
							SELECT
								SESSION,
								PROJECT,
								REPO,
								STARTED,
								FINISHED
							FROM
								GIT_UPDATE_OLD
						;";

						ExecuteNonQuery( sql );

						sql = @"
							DROP TABLE GIT_UPDATE_OLD
						;";

						ExecuteNonQuery( sql );
					}
					break;
			}
		}

		UpdateSession IUpdateRepository.GetUpdateSession(
			Guid session
		) {
			string sql = @"
				SELECT
					SESSION,
					PROJECT,
					REPO,
					STARTED,
					FINISHED,
					COMMITS_WRITTEN
				FROM
					GIT_UPDATE
				WHERE
					SESSION = @session
			;";

			var parameters = new Dictionary<string, object>() {
				{ "@session", session.ToString("N") }
			};

			return ExecuteSingleReader( sql, parameters, ReadProgress );
		}

		void IUpdateRepository.Begin(
			Guid session,
			string repo,
			string project,
			DateTime started
		) {
			string sql = @"
				INSERT INTO GIT_UPDATE
				(
					SESSION,
					PROJECT,
					REPO,
					STARTED
				)
				VALUES
				(
					@session,
					@project,
					@repo,
					@started
				)
			;";

			var parameters = new Dictionary<string, object>() {
				{ "@session", session.ToString("N") },
				{ "@project", project },
				{ "@repo", repo },
				{ "@started", ToText(started) }
			};

			ExecuteNonQuery( sql, parameters );
		}

		void IUpdateRepository.End(
			Guid session,
			DateTime finished,
			int commitsWritten
		) {
			string sql = @"
				UPDATE GIT_UPDATE
				SET
					FINISHED = @finished,
					COMMITS_WRITTEN = @written
				WHERE
					SESSION = @session
			;";

			var parameters = new Dictionary<string, object>() {
				{ "@session", session.ToString("N") },
				{ "@finished", ToText(finished) },
				{ "@written", commitsWritten }
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

		private UpdateSession ReadProgress( DbDataReader reader ) {
			string dbSession = GetString( reader, "SESSION" );
			string dbRepo = GetString( reader, "REPO" );
			string dbProject = GetString( reader, "PROJECT" );
			DateTime dbStarted = GetDateTime( reader, "STARTED" );
			DateTime? dbFinished = GetNullableDateTime( reader, "FINISHED" );
			int dbCommitsWritten = GetInt( reader, "COMMITS_WRITTEN" );

			return new UpdateSession( dbSession, dbRepo, dbProject, dbStarted, dbFinished, dbCommitsWritten );
		}
	}
}
