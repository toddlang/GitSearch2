using System;
using System.Collections.Generic;

namespace GitSearch2.Repository {
	public interface IUpdateRepository {
		void Initialize();

		DateTimeOffset GetMostRecentCommit();

		IEnumerable<RepoProgress> GetProgress( Guid session );

		void SetProgress( Guid session, string repo, string project, int progress );

		void Begin( Guid session, string repo, string project, DateTimeOffset started );

		void End( Guid session, string repo, string project, DateTimeOffset finished, string lastCommitId );
	}
}
