using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace GitSearch2.Client {
	public static class ExtensionMethods {
		public static string GetParameter( this NavigationManager uriHelper, string name ) {
			Uri uri = new Uri( uriHelper.Uri );
			string value = QueryHelpers.ParseQuery( uri.Query ).TryGetValue( name, out StringValues values ) ? values.First() : string.Empty;

			return value;
		}

		public static async Task<T> GetJsonAsync<T>( this HttpClient httpClient, string requestUri, Func<string, T> fromJson ) where T : class {
			string responseJson = await httpClient.GetStringAsync( requestUri );
			return fromJson( responseJson );
		}

		public static Task<S> PostJsonAsync<T, S>( this HttpClient httpClient, string requestUri, T content, Func<T, string> toJson, Func<string, S> fromJson ) where T : class where S : class
			=> httpClient.SendJsonAsync<T, S>( HttpMethod.Post, requestUri, content, toJson, fromJson );

		public static Task PostJsonAsync<T>( this HttpClient httpClient, string requestUri, T content, Func<T, string> toJson ) where T : class
			=> httpClient.SendJsonAsync<T, IgnoreResponse>( HttpMethod.Post, requestUri, content, toJson, default );

		public static Task<S> PutJsonAsync<T, S>( this HttpClient httpClient, string requestUri, T content, Func<T, string> toJson, Func<string, S> fromJson ) where T : class where S : class
			=> httpClient.SendJsonAsync<T, S>( HttpMethod.Put, requestUri, content, toJson, fromJson );

		public static Task PutJsonAsync<T>( this HttpClient httpClient, string requestUri, T content, Func<T, string> toJson ) where T : class
			=> httpClient.SendJsonAsync<T, IgnoreResponse>( HttpMethod.Put, requestUri, content, toJson, default );


		private static async Task<S> SendJsonAsync<T, S>( this HttpClient httpClient, HttpMethod method, string requestUri, T content, Func<T, string> toJson, Func<string, S> fromJson ) where T : class where S : class {
			string requestJson = toJson( content );
			HttpResponseMessage response = await httpClient.SendAsync( new HttpRequestMessage( method, requestUri ) {
				Content = new StringContent( requestJson, Encoding.UTF8, "application/json" )
			} );

			if( ( fromJson is null ) || ( typeof( S ) == typeof( IgnoreResponse ) ) ) {
				return default;
			} else {
				string responseJson = await response.Content.ReadAsStringAsync();
				return fromJson( responseJson );
			}
		}

		private class IgnoreResponse { }
	}
}
