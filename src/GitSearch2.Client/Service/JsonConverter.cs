using System;
using Utf8Json;

namespace GitSearch2.Client.Service {
	internal sealed class JsonConverter : IJsonConverter {

		T IJsonConverter.Deserialize<T>( string value ) {
			return JsonSerializer.Deserialize<T>( value );
		}

		string IJsonConverter.Serialize( object value ) {
			if( value is null ) {
				return "{}";
			}
			return JsonSerializer.ToJsonString( value );
		}
	}
}
