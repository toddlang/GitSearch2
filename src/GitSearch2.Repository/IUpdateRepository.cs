using System;

namespace GitSearch2.Repository {
	public interface IUpdateRepository {
		void Initialize();

		DateTimeOffset GetMostRecentCommit();

		UpdateSession GetUpdateSession( Guid session );

		UpdateSession Begin(
			Guid session,
			string repo,
			string project,
			DateTime started );

		void Resume(
			Guid session,
			DateTime started );

		void End(
			Guid session,
			DateTime finished,
			int commitsWritten );

		bool UpdateInProgress(
			string repo,
			string project );

		UpdateSession GetScheduledUpdate(
			string repo,
			string project );

		UpdateSession ScheduleUpdate(
			Guid session,
			string repo,
			string project );
	}
}
