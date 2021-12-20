using System;
using System.Collections.Generic;
using System.Data.Common;

namespace GitSearch2.Repository.SqlServer {
	public sealed class UpdateSqlServerRepository : SqlServerRepository, IUpdateRepository {

		private readonly Dictionary<string, object> NoParameters = new Dictionary<string, object>();
		private const string SchemaId = "15c8b53ab898475497ad37cf968b93aa";
		private const int TargetSchema = 2;

		public UpdateSqlServerRepository(
			IDb db
		) : base( db ) {
		}

		void IUpdateRepository.Initialize() {
			Initialize( SchemaId, TargetSchema );
		}

		protected override void CreateSchema() {
			const string sql = @"
				CREATE TABLE GIT_UPDATE
				(
					SESSION NVARCHAR(32) NOT NULL,
					PROJECT NVARCHAR(128) NOT NULL,
					REPO NVARCHAR(128) NOT NULL,
					STARTED DATETIME2,
					FINISHED DATETIME2,
					COMMITS_WRITTEN INT,
					PRIMARY KEY(SESSION, PROJECT, REPO)
				)
			;";

			Db.ExecuteNonQuery( sql );
		}

		protected override void UpdateSchema( int targetSchema ) {
			throw new InvalidOperationException();
		}

		UpdateSession IUpdateRepository.GetUpdateSession(
			Guid session
		) {
			const string sql = @"
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

			return Db.ExecuteSingleReader( sql, parameters, ReadProgress );
		}

		UpdateSession IUpdateRepository.Begin(
			Guid session,
			string repo,
			string project,
			DateTime started
		) {
			const string sql = @"
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
				{ "@started", Db.ToText(started) }
			};

			Db.ExecuteNonQuery( sql, parameters );

			return new UpdateSession( session, repo, project, started, null, 0 );
		}

		void IUpdateRepository.Resume(
			Guid session,
			DateTime started
		) {
			const string sql = @"
				UPDATE GIT_UPDATE
				SET
					STARTED = @started
				WHERE
					SESSION = @session
			;";

			var parameters = new Dictionary<string, object>() {
				{ "@session", session.ToString("N") },
				{ "@started", Db.ToText(started) }
			};

			Db.ExecuteNonQuery( sql, parameters );
		}

		void IUpdateRepository.End(
			Guid session,
			DateTime finished,
			int commitsWritten
		) {
			const string sql = @"
				UPDATE GIT_UPDATE
				SET
					FINISHED = @finished,
					COMMITS_WRITTEN = @written
				WHERE
					SESSION = @session
			;";

			var parameters = new Dictionary<string, object>() {
				{ "@session", session.ToString("N") },
				{ "@finished", Db.ToText(finished) },
				{ "@written", commitsWritten }
			};

			Db.ExecuteNonQuery( sql, parameters );
		}

		DateTimeOffset IUpdateRepository.GetMostRecentCommit() {
			const string sql = @"
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

			return Db.ExecuteSingleReader( sql, NoParameters, Db.LoadDateTimeOffset );
		}

		bool IUpdateRepository.UpdateInProgress(
			string repo,
			string project
		) {
			const string sql = @"
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

			int count = Db.ExecuteSingleReader( sql, parameters, Db.LoadInt );
			return ( count > 0 );
		}

		UpdateSession IUpdateRepository.GetScheduledUpdate(
			string repo,
			string project
		) {
			const string sql = @"
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

			return Db.ExecuteSingleReader( sql, parameters, ReadProgress );
		}

		UpdateSession IUpdateRepository.ScheduleUpdate(
			Guid session,
			string repo,
			string project
		) {
			const string sql = @"
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

			Db.ExecuteNonQuery( sql, parameters );

			return new UpdateSession( session, repo, project, null, null, 0 );
		}

		private UpdateSession ReadProgress(
			DbDataReader reader
		) {
			string dbSession = Db.GetString( reader, "SESSION" );
			string dbRepo = Db.GetString( reader, "REPO" );
			string dbProject = Db.GetString( reader, "PROJECT" );
			DateTime? dbStarted = Db.GetNullableDateTime( reader, "STARTED" );
			DateTime? dbFinished = Db.GetNullableDateTime( reader, "FINISHED" );
			int dbCommitsWritten = Db.GetInt( reader, "COMMITS_WRITTEN" );

			return new UpdateSession( new Guid( dbSession ), dbRepo, dbProject, dbStarted, dbFinished, dbCommitsWritten );
		}
	}
}
