using System.Text.Json;

namespace GitSearch2.Client.Service {
	internal sealed class JsonConverter : IJsonConverter {

		private readonly JsonSerializerOptions m_jsonOptions;

		public JsonConverter() {
			m_jsonOptions = new JsonSerializerOptions() {
				PropertyNameCaseInsensitive = true
			};
		}

		T IJsonConverter.Deserialize<T>( string value ) {
			return JsonSerializer.Deserialize<T>( value, m_jsonOptions );
		}

		string IJsonConverter.Serialize( object value ) {
			if( value is null ) {
				return "{}";
			}
			return JsonSerializer.Serialize( value, m_jsonOptions );
		}
	}
}
