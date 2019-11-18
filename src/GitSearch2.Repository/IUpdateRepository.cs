using System;

namespace GitSearch2.Repository {
	public interface IUpdateRepository {
		void Initialize();

		DateTimeOffset GetMostRecentCommit();

		UpdateSession GetUpdateSession( Guid session );

		void Begin( Guid session, string repo, string project, DateTime started );

		void End( Guid session, DateTime finished, int commitsWritten );
	}
}
