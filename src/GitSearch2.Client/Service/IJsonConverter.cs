namespace GitSearch2.Client.Service {
	public interface IJsonConverter {
		T Deserialize<T>( string value );

		string Serialize( object value );
	}
}
