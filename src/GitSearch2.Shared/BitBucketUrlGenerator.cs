
namespace GitSearch2.Shared {
	internal sealed class BitBucketUrlGenerator : IUrlGenerator {
		string IUrlGenerator.CommitUrl( CommitDetails details ) {
			return $@"https://git.dev.d2l/projects/{details.Project}/repos/{details.Repo}/commits/{details.CommitId}";
		}

		string IUrlGenerator.FileUrl( CommitDetails details, string fileName ) {
			return $@"https://git.dev.d2l/projects/{details.Project}/repos/{details.Repo}/commits/{details.CommitId}#{fileName}";
		}

		string IUrlGenerator.MergeUrl( CommitDetails details, string commitId ) {
			return $@"https://git.dev.d2l/projects/{details.Project}/repos/{details.Repo}/commits/{commitId}";
		}

		string IUrlGenerator.PrUrl( string project, string repo, CommitDetails details ) {
			return $@"https://git.dev.d2l/projects/{project}/repos/{repo}/pull-requests/{details.PR}/overview";
		}
	}
}
