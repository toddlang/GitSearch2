
namespace GitSearch2.Shared {
	public interface IRepoWebsiteIdentifier {

		string GetRepoOriginId( string remoteUrl );

		IUrlGenerator GetUrlGenerator( string originId );
	}
}
