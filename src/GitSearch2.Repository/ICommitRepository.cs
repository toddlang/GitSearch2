using System;
using System.Collections.Generic;
using GitSearch2.Shared;

namespace GitSearch2.Repository {
	public interface ICommitRepository {
		void Initialize();

		IEnumerable<CommitDetails> Search( string term, int limit );

		bool ContainsCommit( string commitId, string project, string repo );

		void Add( CommitDetails commit );
	}
}
