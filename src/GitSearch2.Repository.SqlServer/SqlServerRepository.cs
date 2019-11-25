using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace GitSearch2.Repository.SqlServer {
	public class SqlServerRepository {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities

		private readonly string _connectionString;

		public SqlServerRepository(
			SqlServerOptions options
		) {
			if (options is null) {
				throw new ArgumentNullException( nameof( options ) );
			}

			if (string.IsNullOrWhiteSpace(options.ConnectionString)) {
				throw new ArgumentException( options.ConnectionString );
			}

			_connectionString = options.ConnectionString;
		}

		public void Initialize(
			string schemaId,
			int targetSchema
		) {
			string sql = @"
                CREATE TABLE IF NOT EXISTS SETTINGS
                (
				    SETTING_ID NVARCHAR(32) NOT NULL PRIMARY KEY,
				    SETTING_VALUE TEXT NOT NULL
                )
			;";

			ExecuteNonQuery( sql );

			sql = @"
				SELECT
					SETTING_VALUE
				FROM
					SETTINGS
				WHERE
					SETTING_ID = @settingId
			;";

			var parameters = new Dictionary<string, object>() {
				{ "@settingId", schemaId }
			};

			string result = ExecuteSingleReader( sql, parameters, LoadString );
			int currentSchema = int.Parse( result ?? "0" );

			if( currentSchema == 0 ) {
				CreateSchema();

				sql = @"
					INSERT INTO SETTINGS
					(
						SETTING_ID,
						SETTING_VALUE
					)
					VALUES
					(
						@settingId,
						@settingValue
					)
				;";

				parameters = new Dictionary<string, object>() {
					{ "@settingId", schemaId },
					{ "@settingValue", targetSchema.ToString() }
				};
				ExecuteNonQuery( sql, parameters );

			} else if( currentSchema < targetSchema ) {

				for( int version = currentSchema + 1; version <= targetSchema; version++ ) {
					UpdateSchema( version );
				}

				sql = @"
					UPDATE SETTINGS
					SET
						SETTING_VALUE = @settingValue
					WHERE
						SETTING_ID = @settingId
				;";

				parameters = new Dictionary<string, object>() {
					{ "@settingId", schemaId },
					{ "@settingValue", targetSchema.ToString() }
				};
				ExecuteNonQuery( sql, parameters );
			}
		}

		public async Task ExecuteNonQueryAsync( string sql ) {

			using( var connection = new SqlConnection( _connectionString ) ) {
				await connection.OpenAsync();

				await PerformNonQueryAsync( connection, sql );

				connection.Close();
			}
		}

		public void ExecuteNonQuery( string sql ) {

			using( var connection = new SqlConnection( _connectionString ) ) {
				connection.Open();

				PerformNonQuery( connection, sql );

				connection.Close();
			}
		}

		public async Task ExecuteNonQueryAsync(
			string sql,
			IDictionary<string, object> parameters
		) {
			using( var connection = new SqlConnection( _connectionString ) ) {
				await connection.OpenAsync();

				await PerformNonQueryAsync( connection, sql, parameters );

				connection.Close();
			}
		}

		public void ExecuteNonQuery(
			string sql,
			IDictionary<string, object> parameters
		) {
			using( var connection = new SqlConnection( _connectionString ) ) {
				connection.Open();

				PerformNonQuery( connection, sql, parameters );

				connection.Close();
			}
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

		protected virtual void CreateSchema() {
			throw new NotImplementedException();
		}

		protected virtual void UpdateSchema( int targetSchema ) {
			throw new NotImplementedException();
		}

		protected static string LoadString( DbDataReader reader ) {
			return reader.GetString( 0 );
		}

		protected static int LoadInt( DbDataReader reader ) {
			return reader.GetInt32( 0 );
		}

		protected static DateTimeOffset LoadDateTimeOffset( DbDataReader reader ) {
			if( reader.IsDBNull( 0 ) ) {
				return default;
			}

			return new DateTimeOffset( reader.GetDateTime( 0 ), TimeSpan.Zero );
		}

		protected static int GetInt( DbDataReader reader, string column ) {
			return reader.GetInt32( reader.GetOrdinal( column ) );
		}

		protected static bool GetBoolean( DbDataReader reader, string column ) {
			return reader.GetBoolean( reader.GetOrdinal( column ) );
		}

		protected static string GetString( DbDataReader reader, string column ) {
			int index = reader.GetOrdinal( column );
			if( reader.IsDBNull( index ) ) {
				return default;
			}
			return reader.GetString( index );
		}

		protected static DateTimeOffset GetDateTimeOffset( DbDataReader reader, string column ) {
			return new DateTimeOffset( reader.GetDateTime( reader.GetOrdinal( column ) ), TimeSpan.Zero );
		}

		protected static DateTime GetDateTime( DbDataReader reader, string column ) {
			DateTime result = reader.GetDateTime( reader.GetOrdinal( column ) );

			return result.ToUniversalTime();
		}

		protected static string ToText( DateTimeOffset value ) {
			return value.ToUniversalTime().ToString( "yyyy-MM-dd HH:mm:ss" );
		}

		protected static string ToText( DateTime value ) {
			return value.ToUniversalTime().ToString( "yyyy-MM-dd HH:mm:ss" );
		}

		protected static DateTimeOffset? GetNullableDateTimeOffset( DbDataReader reader, string column ) {
			int index = reader.GetOrdinal( column );
			if( reader.IsDBNull( index ) ) {
				return default;
			}

			return new DateTimeOffset( reader.GetDateTime( index ), TimeSpan.Zero );
		}

		protected static DateTime? GetNullableDateTime( DbDataReader reader, string column ) {
			int index = reader.GetOrdinal( column );
			if( reader.IsDBNull( index ) ) {
				return default;
			}

			DateTime result = reader.GetDateTime( index );
			return result.ToUniversalTime();
		}

		private async Task PerformNonQueryAsync(
			SqlConnection connection,
			string sql
		) {
			using( SqlCommand command = connection.CreateCommand() ) {
				command.CommandText = sql;

				await command.ExecuteNonQueryAsync();
			}
		}

		private void PerformNonQuery(
			SqlConnection connection,
			string sql
		) {
			using( SqlCommand command = connection.CreateCommand() ) {
				command.CommandText = sql;

				command.ExecuteNonQuery();
			}
		}

		private async Task PerformNonQueryAsync(
			SqlConnection connection,
			string sql,
			IDictionary<string, object> parameters
		) {
			using( SqlCommand command = connection.CreateCommand() ) {
				command.CommandText = sql;
				foreach( string key in parameters.Keys ) {
					command.Parameters.AddWithValue( key, parameters[key] );
				}

				await command.ExecuteNonQueryAsync();
			}
		}

		private void PerformNonQuery(
			SqlConnection connection,
			string sql,
			IDictionary<string, object> parameters
		) {
			using( SqlCommand command = connection.CreateCommand() ) {
				command.CommandText = sql;
				foreach( string key in parameters.Keys ) {
					command.Parameters.AddWithValue( key, parameters[key] );
				}

				command.ExecuteNonQuery();
			}
		}

		private async Task<IEnumerable<T>> PerformReaderAsync<T>(
			SqlConnection connection,
			string sql,
			Func<DbDataReader, T> loader
		) {
			var result = new List<T>();
			using( SqlCommand command = connection.CreateCommand() ) {
				command.CommandText = sql;

				using( DbDataReader reader = await command.ExecuteReaderAsync() ) {
					if( reader.HasRows ) {
						while( await reader.ReadAsync() ) {
							result.Add( loader( reader ) );
						}
					}

					reader.Close();
				}
			}
			return result;
		}

		private IEnumerable<T> PerformReader<T>(
			SqlConnection connection,
			string sql,
			Func<DbDataReader, T> loader
		) {
			var result = new List<T>();
			using( SqlCommand command = connection.CreateCommand() ) {
				command.CommandText = sql;

				using( SqlDataReader reader = command.ExecuteReader() ) {
					if( reader.HasRows ) {
						while( reader.Read() ) {
							result.Add( loader( reader ) );
						}
					}

					reader.Close();
				}
			}
			return result;
		}

		private async Task<IEnumerable<T>> PerformReaderAsync<T>(
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

				using( DbDataReader reader = await command.ExecuteReaderAsync() ) {
					if( reader.HasRows ) {
						while( await reader.ReadAsync() ) {
							result.Add( loader( reader ) );
						}
					}

					reader.Close();
				}
			}

			return result;
		}

		private IEnumerable<T> PerformReader<T>(
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

				using( SqlDataReader reader = command.ExecuteReader() ) {
					if( reader.HasRows ) {
						while( reader.Read() ) {
							result.Add( loader( reader ) );
						}
					}

					reader.Close();
				}
			}

			return result;
		}

		private async Task<T> PerformSingleReaderAsync<T>(
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

				using( DbDataReader reader = await command.ExecuteReaderAsync() ) {
					if( reader.HasRows ) {
						if( await reader.ReadAsync() ) {
							result = loader( reader );
						}
					}
					reader.Close();
				}
			}

			return result;
		}

		private T PerformSingleReader<T>(
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

				using( SqlDataReader reader = command.ExecuteReader() ) {
					if( reader.HasRows ) {
						if( reader.Read() ) {
							result = loader( reader );
						}
					}
					reader.Close();
				}
			}

			return result;
		}
#pragma warning restore CA2100
	}
}
