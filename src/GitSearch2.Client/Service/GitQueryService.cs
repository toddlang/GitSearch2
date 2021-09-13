using GitSearch2.Shared;

namespace GitSearch2.Client.Service {
	internal sealed class GitQueryService : IGitQueryService {

		private readonly HttpClient _http;
		private readonly IJsonConverter _json;

		public GitQueryService(
			HttpClient http,
			IJsonConverter jsonConverter
		) {
			_http = http;
			_json = jsonConverter;
		}

		async Task<string> IGitQueryService.BeginUpdate() {
			HttpResponseMessage response = await _http.PostAsync( "/api/GitQuery/Update", default );
			string content = await response.Content.ReadAsStringAsync();
			return content;
		}

		async Task<int> IGitQueryService.GetProgress( string sessionId ) {
			HttpResponseMessage response = await _http.GetAsync( $"/api/GitQuery/Progress/{sessionId}" );
			string content = await response.Content.ReadAsStringAsync();
			return int.Parse( content );
		}

		async Task<GitQueryResponse> IGitQueryService.GitQuery( string searchTerm, int startRecord ) {
			GitQuery request = new GitQuery( searchTerm, startRecord, 100 );
			return await _http.PostJsonAsync( "/api/GitQuery/Search", request,
				( r ) => _json.Serialize( r ),
				( s ) => _json.Deserialize<GitQueryResponse>( s ) );
		}
	}
}
