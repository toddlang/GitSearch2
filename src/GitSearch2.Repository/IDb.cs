using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace GitSearch2.Repository {
	public interface IDb {
		void ExecuteNonQuery( string sql );
		void ExecuteNonQuery( string sql, IDictionary<string, object> parameters );
		Task ExecuteNonQueryAsync( string sql );
		Task ExecuteNonQueryAsync( string sql, IDictionary<string, object> parameters );
		IEnumerable<T> ExecuteReader<T>( string sql, Func<DbDataReader, T> loader );
		IEnumerable<T> ExecuteReader<T>( string sql, IDictionary<string, object> parameters, Func<DbDataReader, T> loader );
		Task<IEnumerable<T>> ExecuteReaderAsync<T>( string sql, Func<DbDataReader, T> loader );
		Task<IEnumerable<T>> ExecuteReaderAsync<T>( string sql, IDictionary<string, object> parameters, Func<DbDataReader, T> loader );
		T ExecuteSingleReader<T>( string sql, IDictionary<string, object> parameters, Func<DbDataReader, T> loader );
		Task<T> ExecuteSingleReaderAsync<T>( string sql, IDictionary<string, object> parameters, Func<DbDataReader, T> loader );
	}
}
