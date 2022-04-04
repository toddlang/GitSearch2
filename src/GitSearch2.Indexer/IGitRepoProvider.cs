using System;
using System.Collections.Generic;
using LibGit2Sharp;

namespace GitSearch2.Indexer {
	public interface IGitRepoProvider: IDisposable {
		IRepository GetRepo();

		IRepository GetRepo( Commit current, out Commit newCurrent );

		IEnumerable<string> GetCommitFiles( Commit commit );
	}
}
