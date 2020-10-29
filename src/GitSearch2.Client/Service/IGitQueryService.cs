using System.Threading.Tasks;
using GitSearch2.Shared;

namespace GitSearch2.Client.Service {
	public interface IGitQueryService {
		Task<GitQueryResponse> GitQuery( string searchTerm, int startRecord );

		Task<string> BeginUpdate();

		Task<int> GetProgress( string sessionId );
	}
}
