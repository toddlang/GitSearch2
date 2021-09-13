using LibGit2Sharp;

namespace GitSearch2.Indexer {
	public interface IGitRepoProvider {
		IRepository GetRepo();

		IRepository GetRepo( Commit current, out Commit newCurrent );

		IEnumerable<string> GetCommitFiles( Commit commit );
	}
}
