
namespace GitSearch2.Shared {
	public interface IUrlGenerator {

		string CommitUrl( CommitDetails details );

		string PrUrl( string project, string repo, CommitDetails details );

		string MergeUrl( CommitDetails details, string commitId );

		string FileUrl( CommitDetails details, string fileName );
	}
}
