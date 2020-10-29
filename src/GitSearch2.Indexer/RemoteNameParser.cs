using System.Linq;
using LibGit2Sharp;

namespace GitSearch2.Indexer {
	internal sealed class RemoteNameParser : INameParser {
		RepoProjectName INameParser.Parse( IRepository repository ) {
			string remoteUrl = repository.Network.Remotes.First().Url;

			int repoNameStart = remoteUrl.LastIndexOf( @"/" ) + 1;
			int repoNameEnd = remoteUrl.IndexOf( ".git" );
			string repoName = remoteUrl[repoNameStart..repoNameEnd];

			string cutUrl = remoteUrl.Substring( 0, remoteUrl.LastIndexOf( @"/" ) );
			int projectStart = cutUrl.LastIndexOf( @"/" ) + 1;
			int projectEnd = cutUrl.Length;
			string projectName = cutUrl[projectStart..projectEnd];

			return new RepoProjectName( repoName, projectName );
		}
	}
}
