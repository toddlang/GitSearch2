using System.Text;

namespace GitSearch2.Shared {

	internal sealed class GitHubUrlGenerator : IUrlGenerator {

		string IUrlGenerator.CommitUrl( CommitDetails details ) {
			return $@"https://github.com/Brightspace/{details.Repo}/commit/{details.CommitId}";
		}

		string IUrlGenerator.FileUrl( CommitDetails details, string fileName ) {
			Sha256 sha256 = new Sha256();
			byte[] payload = Encoding.UTF8.GetBytes( fileName );
			sha256.AddData( payload, 0, (uint)payload.Length );
			string hash = Sha256.ArrayToString( sha256.GetHash() );
			return $@"https://github.com/Brightspace/{details.Repo}/commit/{details.CommitId}#diff-{hash}";
		}

		string IUrlGenerator.MergeUrl( CommitDetails details, string commitId ) {
			return $@"https://github.com/Brightspace/{details.Repo}/commit/{commitId}";
		}

		string IUrlGenerator.PrUrl( string project, string repo, CommitDetails details ) {
			return $@"https://github.com/Brightspace/{details.Repo}/pull/{details.PR}";
		}
	}
}
