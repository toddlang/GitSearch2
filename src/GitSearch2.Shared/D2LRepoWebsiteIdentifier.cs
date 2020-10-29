namespace GitSearch2.Shared {
	internal sealed class D2LRepoWebsiteIdentifier : IRepoWebsiteIdentifier {

		private readonly BitBucketUrlGenerator _bitbucket;
		private readonly GitHubUrlGenerator _github;

		public const string GitHubOriginId = "github";
		public const string BitbucketOriginId = "bitbucket";

		public D2LRepoWebsiteIdentifier() {
			_bitbucket = new BitBucketUrlGenerator();
			_github = new GitHubUrlGenerator();
		}

		string IRepoWebsiteIdentifier.GetRepoOriginId( string remoteUrl ) {
			// Assume it's on github unless we can see otherwise

			string result = GitHubOriginId;
			if( remoteUrl.Contains( "git.dev.d2l" ) ) {
				result = BitbucketOriginId;
			}

			return result;
		}

		IUrlGenerator IRepoWebsiteIdentifier.GetUrlGenerator( string originId ) {
			return originId switch {
				GitHubOriginId => _github,
				BitbucketOriginId => _bitbucket,
				_ => null,
			};
		}
	}
}
