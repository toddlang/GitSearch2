using System.Collections.Generic;
using LibGit2Sharp;

namespace GitSearch2.Indexer {
	public interface IGitRepoProvider {
		LibGit2Sharp.Repository GetRepo();

		LibGit2Sharp.Repository GetRepo( Commit current, out Commit newCurrent );

		IEnumerable<string> GetCommitFiles( Commit commit );
	}
}
