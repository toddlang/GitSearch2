using GitSearch2.Shared;

namespace GitSearch2.Indexer.Tests.Integration {
	internal sealed class TestingWebsiteIdentifier : IRepoWebsiteIdentifier {
		string IRepoWebsiteIdentifier.GetRepoOriginId( string remoteUrl ) {
			return "github";
		}

		IUrlGenerator IRepoWebsiteIdentifier.GetUrlGenerator( string originId ) {
			return null;
		}
	}
}
