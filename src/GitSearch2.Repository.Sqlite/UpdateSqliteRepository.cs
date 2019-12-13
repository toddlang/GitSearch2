using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Extensions.Options;

namespace GitSearch2.Repository.Sqlite {
	public sealed class UpdateSqliteRepository : SqliteRepository, IUpdateRepository {

		private readonly Dictionary<string, object> NoParameters = new Dictionary<string, object>();

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
					STARTED TEXT,
					FINISHED TEXT,
					COMMITS_WRITTEN INT,
					PRIMARY KEY(SESSION, PROJECT, REPO)
				)
			;";

			ExecuteNonQuery( sql );
		}

		protected override void UpdateSchema( int targetSchema ) {
			throw new InvalidOperationException();
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

		UpdateSession IUpdateRepository.Begin(
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

			return new UpdateSession( session, repo, project, started, null, 0 );
		}

		void IUpdateRepository.Resume(
			Guid session,
			DateTime started
		) {
			string sql = @"
				UPDATE GIT_UPDATE
				SET
					STARTED = @started
				WHERE
					SESSION = @session
			;";

			var parameters = new Dictionary<string, object>() {
				{ "@session", session.ToString("N") },
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

			return ExecuteSingleReader( sql, NoParameters, LoadDateTimeOffset );
		}

		bool IUpdateRepository.UpdateInProgress( string repo, string project ) {
			string sql = @"
				SELECT
					COUNT(*)
				FROM
					GIT_UPDATE
				WHERE
					FINISHED IS NULL
					AND REPO = @repo
					AND PROJECT = @project
			";

			var parameters = new Dictionary<string, object>() {
				{ "@repo", repo },
				{ "@project", project }
			};

			int count = ExecuteSingleReader( sql, parameters, LoadInt );
			return ( count > 0 );
		}

		UpdateSession IUpdateRepository.GetScheduledUpdate( string repo, string project ) {
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
					STARTED IS NULL
					AND REPO = @repo
					AND PROJECT = @project
			";

			var parameters = new Dictionary<string, object>() {
				{ "@repo", repo },
				{ "@project", project }
			};

			return ExecuteSingleReader( sql, parameters, ReadProgress );
		}

		UpdateSession IUpdateRepository.ScheduleUpdate(
			Guid session,
			string repo,
			string project
		) {
			string sql = @"
				INSERT INTO GIT_UPDATE
				(
					SESSION,
					PROJECT,
					REPO
				)
				VALUES
				(
					@session,
					@project,
					@repo
				)
			;";

			var parameters = new Dictionary<string, object>() {
				{ "@session", session.ToString("N") },
				{ "@project", project },
				{ "@repo", repo }
			};

			ExecuteNonQuery( sql, parameters );

			return new UpdateSession( session, repo, project, null, null, 0 );
		}


		private UpdateSession ReadProgress( DbDataReader reader ) {
			string dbSession = GetString( reader, "SESSION" );
			string dbRepo = GetString( reader, "REPO" );
			string dbProject = GetString( reader, "PROJECT" );
			DateTime dbStarted = GetDateTime( reader, "STARTED" );
			DateTime? dbFinished = GetNullableDateTime( reader, "FINISHED" );
			int dbCommitsWritten = GetInt( reader, "COMMITS_WRITTEN" );

			return new UpdateSession( new Guid( dbSession ), dbRepo, dbProject, dbStarted, dbFinished, dbCommitsWritten );
		}
	}
}
