using LibGit2Sharp;

namespace GitSearch2.Indexer {
	internal interface INameParser {

		RepoProjectName Parse( IRepository repository );
	}
}
