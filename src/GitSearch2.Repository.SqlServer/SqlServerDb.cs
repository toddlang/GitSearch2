using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace GitSearch2.Repository.SqlServer {
	internal sealed class SqlServerDb : IDb {

		private readonly string _connectionString;

		public SqlServerDb(
			SqlServerOptions options
		) {
			if( options is null ) {
				throw new ArgumentNullException( nameof( options ) );
			}

			if( string.IsNullOrWhiteSpace( options.ConnectionString ) ) {
				throw new ArgumentException( options.ConnectionString );
			}

			_connectionString = options.ConnectionString;
		}

		public string LoadString( DbDataReader reader ) {
			return reader.GetString( 0 );
		}

		public int LoadInt( DbDataReader reader ) {
			return reader.GetInt32( 0 );
		}

		public DateTimeOffset LoadDateTimeOffset( DbDataReader reader ) {
			if( reader.IsDBNull( 0 ) ) {
				return default;
			}

			return new DateTimeOffset( reader.GetDateTime( 0 ), TimeSpan.Zero );
		}

		public int GetInt( DbDataReader reader, string column ) {
			return reader.GetInt32( reader.GetOrdinal( column ) );
		}

		public bool GetBoolean( DbDataReader reader, string column ) {
			return reader.GetBoolean( reader.GetOrdinal( column ) );
		}

		public string GetString( DbDataReader reader, string column ) {
			int index = reader.GetOrdinal( column );
			if( reader.IsDBNull( index ) ) {
				return default;
			}
			return reader.GetString( index );
		}

		public DateTimeOffset GetDateTimeOffset( DbDataReader reader, string column ) {
			return new DateTimeOffset( reader.GetDateTime( reader.GetOrdinal( column ) ), TimeSpan.Zero );
		}

		public DateTime GetDateTime( DbDataReader reader, string column ) {
			DateTime result = reader.GetDateTime( reader.GetOrdinal( column ) );

			return result.ToUniversalTime();
		}

		public string ToText( DateTimeOffset value ) {
			return value.ToUniversalTime().ToString( "yyyy-MM-ddTHH:mm:ss" );
		}

		public string ToText( DateTime value ) {
			return value.ToUniversalTime().ToString( "yyyy-MM-ddTHH:mm:ss" );
		}

		public DateTimeOffset? GetNullableDateTimeOffset( DbDataReader reader, string column ) {
			int index = reader.GetOrdinal( column );
			if( reader.IsDBNull( index ) ) {
				return default;
			}

			return new DateTimeOffset( reader.GetDateTime( index ), TimeSpan.Zero );
		}

		public DateTime? GetNullableDateTime( DbDataReader reader, string column ) {
			int index = reader.GetOrdinal( column );
			if( reader.IsDBNull( index ) ) {
				return default;
			}

			DateTime result = reader.GetDateTime( index );
			return result.ToUniversalTime();
		}

		public async Task ExecuteNonQueryAsync( string sql ) {

			using var connection = new SqlConnection( _connectionString );
			await connection.OpenAsync();

			await PerformNonQueryAsync( connection, sql );

			connection.Close();
		}

		public void ExecuteNonQuery( string sql ) {

			using var connection = new SqlConnection( _connectionString );
			connection.Open();

			PerformNonQuery( connection, sql );

			connection.Close();
		}

		public async Task ExecuteNonQueryAsync(
			string sql,
			IDictionary<string, object> parameters
		) {
			using var connection = new SqlConnection( _connectionString );
			await connection.OpenAsync();

			await PerformNonQueryAsync( connection, sql, parameters );

			connection.Close();
		}

		public void ExecuteNonQuery(
			string sql,
			IDictionary<string, object> parameters
		) {
			using var connection = new SqlConnection( _connectionString );
			connection.Open();

			PerformNonQuery( connection, sql, parameters );

			connection.Close();
		}

		public async Task<IEnumerable<T>> ExecuteReaderAsync<T>(
			string sql,
			Func<DbDataReader, T> loader
		) {
			IEnumerable<T> result;

			using( var connection = new SqlConnection( _connectionString ) ) {
				await connection.OpenAsync();

				result = await PerformReaderAsync( connection, sql, loader );

				connection.Close();
			}
			return result;
		}

		public IEnumerable<T> ExecuteReader<T>(
			string sql,
			Func<DbDataReader, T> loader
		) {
			IEnumerable<T> result;

			using( var connection = new SqlConnection( _connectionString ) ) {
				connection.Open();

				result = PerformReader( connection, sql, loader );

				connection.Close();
			}
			return result;
		}

		public async Task<IEnumerable<T>> ExecuteReaderAsync<T>(
			string sql,
			IDictionary<string, object> parameters,
			Func<DbDataReader, T> loader
		) {
			var result = default( IEnumerable<T> );

			using( var connection = new SqlConnection( _connectionString ) ) {
				await connection.OpenAsync();

				result = await PerformReaderAsync( connection, sql, parameters, loader );

				connection.Close();
			}

			return result;
		}

		public IEnumerable<T> ExecuteReader<T>(
			string sql,
			IDictionary<string, object> parameters,
			Func<DbDataReader, T> loader
		) {
			var result = default( IEnumerable<T> );

			using( var connection = new SqlConnection( _connectionString ) ) {
				connection.Open();

				result = PerformReader( connection, sql, parameters, loader );

				connection.Close();
			}

			return result;
		}

		public async Task<T> ExecuteSingleReaderAsync<T>(
			string sql,
			IDictionary<string, object> parameters,
			Func<DbDataReader, T> loader
		) {
			T result = default;

			using( var connection = new SqlConnection( _connectionString ) ) {
				await connection.OpenAsync();

				result = await PerformSingleReaderAsync( connection, sql, parameters, loader );

				connection.Close();
			}

			return result;
		}

		public T ExecuteSingleReader<T>(
			string sql,
			IDictionary<string, object> parameters,
			Func<DbDataReader, T> loader
		) {
			T result = default;

			using( var connection = new SqlConnection( _connectionString ) ) {
				connection.Open();

				result = PerformSingleReader( connection, sql, parameters, loader );

				connection.Close();
			}

			return result;
		}

		private static async Task PerformNonQueryAsync(
			SqlConnection connection,
			string sql
		) {
			using SqlCommand command = connection.CreateCommand();
			command.CommandText = sql;

			await command.ExecuteNonQueryAsync();
		}

		private static void PerformNonQuery(
			SqlConnection connection,
			string sql
		) {
			using SqlCommand command = connection.CreateCommand();
			command.CommandText = sql;

			command.ExecuteNonQuery();
		}

		private static async Task PerformNonQueryAsync(
			SqlConnection connection,
			string sql,
			IDictionary<string, object> parameters
		) {
			using SqlCommand command = connection.CreateCommand();
			command.CommandText = sql;
			foreach( string key in parameters.Keys ) {
				command.Parameters.AddWithValue( key, parameters[key] );
			}

			await command.ExecuteNonQueryAsync();
		}

		private static void PerformNonQuery(
			SqlConnection connection,
			string sql,
			IDictionary<string, object> parameters
		) {
			using SqlCommand command = connection.CreateCommand();
			command.CommandText = sql;
			foreach( string key in parameters.Keys ) {
				command.Parameters.AddWithValue( key, parameters[key] );
			}

			command.ExecuteNonQuery();
		}

		private static async Task<IEnumerable<T>> PerformReaderAsync<T>(
			SqlConnection connection,
			string sql,
			Func<DbDataReader, T> loader
		) {
			var result = new List<T>();
			using( SqlCommand command = connection.CreateCommand() ) {
				command.CommandText = sql;

				using DbDataReader reader = await command.ExecuteReaderAsync();
				if( reader.HasRows ) {
					while( await reader.ReadAsync() ) {
						result.Add( loader( reader ) );
					}
				}

				reader.Close();
			}
			return result;
		}

		private static IEnumerable<T> PerformReader<T>(
			SqlConnection connection,
			string sql,
			Func<DbDataReader, T> loader
		) {
			var result = new List<T>();
			using( SqlCommand command = connection.CreateCommand() ) {
				command.CommandText = sql;

				using SqlDataReader reader = command.ExecuteReader();
				if( reader.HasRows ) {
					while( reader.Read() ) {
						result.Add( loader( reader ) );
					}
				}

				reader.Close();
			}
			return result;
		}

		private static async Task<IEnumerable<T>> PerformReaderAsync<T>(
			SqlConnection connection,
			string sql,
			IDictionary<string, object> parameters,
			Func<DbDataReader, T> loader
		) {
			var result = new List<T>();

			using( SqlCommand command = connection.CreateCommand() ) {
				command.CommandText = sql;
				foreach( string key in parameters.Keys ) {
					command.Parameters.AddWithValue( key, parameters[key] );
				}

				using DbDataReader reader = await command.ExecuteReaderAsync();
				if( reader.HasRows ) {
					while( await reader.ReadAsync() ) {
						result.Add( loader( reader ) );
					}
				}

				reader.Close();
			}

			return result;
		}

		private static IEnumerable<T> PerformReader<T>(
			SqlConnection connection,
			string sql,
			IDictionary<string, object> parameters,
			Func<DbDataReader, T> loader
		) {
			var result = new List<T>();

			using( SqlCommand command = connection.CreateCommand() ) {
				command.CommandText = sql;
				foreach( string key in parameters.Keys ) {
					command.Parameters.AddWithValue( key, parameters[key] );
				}

				using SqlDataReader reader = command.ExecuteReader();
				if( reader.HasRows ) {
					while( reader.Read() ) {
						result.Add( loader( reader ) );
					}
				}

				reader.Close();
			}

			return result;
		}

		private static async Task<T> PerformSingleReaderAsync<T>(
			SqlConnection connection,
			string sql,
			IDictionary<string, object> parameters,
			Func<DbDataReader, T> loader
		) {
			T result = default;

			using( SqlCommand command = connection.CreateCommand() ) {
				command.CommandText = sql;
				foreach( string key in parameters.Keys ) {
					command.Parameters.AddWithValue( key, parameters[key] );
				}

				using DbDataReader reader = await command.ExecuteReaderAsync();
				if( reader.HasRows ) {
					if( await reader.ReadAsync() ) {
						result = loader( reader );
					}
				}
				reader.Close();
			}

			return result;
		}

		private static T PerformSingleReader<T>(
			SqlConnection connection,
			string sql,
			IDictionary<string, object> parameters,
			Func<DbDataReader, T> loader
		) {
			T result = default;

			using( SqlCommand command = connection.CreateCommand() ) {
				command.CommandText = sql;
				foreach( string key in parameters.Keys ) {
					command.Parameters.AddWithValue( key, parameters[key] );
				}

				using SqlDataReader reader = command.ExecuteReader();
				if( reader.HasRows ) {
					if( reader.Read() ) {
						result = loader( reader );
					}
				}
				reader.Close();
			}

			return result;
		}
	}
}
