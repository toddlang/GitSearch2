using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
namespace GitSearch2.Repository.Sqlite {
	public sealed class SqliteRepositoryTransaction {
		internal SqliteConnection Connection;
		internal SqliteTransaction Transaction;
	}

	public abstract class SqliteRepository {
		private readonly string _connectionString;

		public SqliteRepository( SqliteOptions options ) {
			if( string.IsNullOrWhiteSpace( options.ConnectionString ) ) {
				throw new ArgumentException( nameof( options ) );
			}

			_connectionString = options.ConnectionString;
		}

		public SqliteRepositoryTransaction Begin(bool startTransaction = true) {
			var connection = new SqliteConnection( _connectionString );
			connection.Open();

			var transaction = default( SqliteTransaction );
			if (startTransaction) {
				transaction = connection.BeginTransaction();
			}

			var result = new SqliteRepositoryTransaction {
				Connection = connection,
				Transaction = transaction
			};

			return result;
		}

		public static void End( SqliteRepositoryTransaction transaction ) {
			if( transaction != null ) {
				transaction.Transaction?.Commit();
				transaction.Connection.Close();

				transaction.Transaction?.Dispose();
				transaction.Connection.Dispose();

				transaction.Transaction = null;
				transaction.Connection = null;
			}
		}

		public void Initialize( string schemaId, int targetSchema ) {
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

		protected abstract void CreateSchema();

		protected abstract void UpdateSchema( int targetSchema );

		public async Task ExecuteNonQueryAsync( string sql, SqliteRepositoryTransaction transaction = null ) {

			if( transaction?.Connection != null ) {
				await PerformNonQueryAsync( transaction.Connection, sql );
			} else {

			}
			using( var connection = new SqliteConnection( _connectionString ) ) {
				await connection.OpenAsync();

				await PerformNonQueryAsync( connection, sql );

				connection.Close();
			}
		}

		public void ExecuteNonQuery( string sql, SqliteRepositoryTransaction transaction = null ) {

			if( transaction?.Connection != null ) {
				PerformNonQuery( transaction.Connection, sql );
			} else {

			}
			using( var connection = new SqliteConnection( _connectionString ) ) {
				connection.Open();

				PerformNonQuery( connection, sql );

				connection.Close();
			}
		}

		private async Task PerformNonQueryAsync( SqliteConnection connection, string sql ) {
			using( SqliteCommand command = connection.CreateCommand() ) {
				command.CommandText = sql;

				await command.ExecuteNonQueryAsync();
			}
		}

		private void PerformNonQuery( SqliteConnection connection, string sql ) {
			using( SqliteCommand command = connection.CreateCommand() ) {
				command.CommandText = sql;

				command.ExecuteNonQuery();
			}
		}

		public async Task ExecuteNonQueryAsync( string sql, IDictionary<string, object> parameters, SqliteRepositoryTransaction transaction = null ) {
			if( transaction?.Connection != null ) {
				await PerformNonQueryAsync( transaction.Connection, sql, parameters );
			} else {
				using( var connection = new SqliteConnection( _connectionString ) ) {
					await connection.OpenAsync();

					await PerformNonQueryAsync( connection, sql, parameters );

					connection.Close();
				}
			}
		}

		public void ExecuteNonQuery( string sql, IDictionary<string, object> parameters, SqliteRepositoryTransaction transaction = null ) {
			if( transaction?.Connection != null ) {
				PerformNonQuery( transaction.Connection, sql, parameters );
			} else {
				using( var connection = new SqliteConnection( _connectionString ) ) {
					connection.Open();

					PerformNonQuery( connection, sql, parameters );

					connection.Close();
				}
			}
		}

		private async Task PerformNonQueryAsync( SqliteConnection connection, string sql, IDictionary<string, object> parameters ) {
			using( SqliteCommand command = connection.CreateCommand() ) {
				command.CommandText = sql;
				foreach( string key in parameters.Keys ) {
					command.Parameters.AddWithValue( key, parameters[key] );
				}

				await command.ExecuteNonQueryAsync();
			}
		}

		private void PerformNonQuery( SqliteConnection connection, string sql, IDictionary<string, object> parameters ) {
			using( SqliteCommand command = connection.CreateCommand() ) {
				command.CommandText = sql;
				foreach( string key in parameters.Keys ) {
					command.Parameters.AddWithValue( key, parameters[key] );
				}

				command.ExecuteNonQuery();
			}
		}

		public async Task<IEnumerable<T>> ExecuteReaderAsync<T>( string sql, Func<DbDataReader, T> loader, SqliteRepositoryTransaction transaction = null ) {
			IEnumerable<T> result;

			if( transaction?.Connection != null ) {
				result = await PerformReaderAsync( transaction.Connection, sql, loader );
			} else {
				using( var connection = new SqliteConnection( _connectionString ) ) {
					await connection.OpenAsync();

					result = await PerformReaderAsync( connection, sql, loader );

					connection.Close();
				}
			}
			return result;
		}

		public IEnumerable<T> ExecuteReader<T>( string sql, Func<DbDataReader, T> loader, SqliteRepositoryTransaction transaction = null ) {
			IEnumerable<T> result;

			if( transaction?.Connection != null ) {
				result = PerformReader( transaction.Connection, sql, loader );
			} else {
				using( var connection = new SqliteConnection( _connectionString ) ) {
					connection.OpenAsync();

					result = PerformReader( connection, sql, loader );

					connection.Close();
				}
			}
			return result;
		}

		private async Task<IEnumerable<T>> PerformReaderAsync<T>( SqliteConnection connection, string sql, Func<DbDataReader, T> loader ) {
			var result = new List<T>();
			using( SqliteCommand command = connection.CreateCommand() ) {
				command.CommandText = sql;

				using( DbDataReader reader = await command.ExecuteReaderAsync() ) {
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

		private IEnumerable<T> PerformReader<T>( SqliteConnection connection, string sql, Func<DbDataReader, T> loader ) {
			var result = new List<T>();
			using( SqliteCommand command = connection.CreateCommand() ) {
				command.CommandText = sql;

				using( SqliteDataReader reader = command.ExecuteReader() ) {
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

		public async Task<IEnumerable<T>> ExecuteReaderAsync<T>( string sql, IDictionary<string, object> parameters, Func<DbDataReader, T> loader, SqliteRepositoryTransaction transaction = null ) {
			var result = default( IEnumerable<T> );

			if( transaction?.Connection != null ) {
				result = await PerformReaderAsync( transaction.Connection, sql, parameters, loader );
			} else {
				using( var connection = new SqliteConnection( _connectionString ) ) {
					await connection.OpenAsync();

					result = await PerformReaderAsync( connection, sql, parameters, loader );

					connection.Close();
				}
			}

			return result;
		}

		public IEnumerable<T> ExecuteReader<T>( string sql, IDictionary<string, object> parameters, Func<DbDataReader, T> loader, SqliteRepositoryTransaction transaction = null ) {
			var result = default( IEnumerable<T> );

			if( transaction?.Connection != null ) {
				result = PerformReader( transaction.Connection, sql, parameters, loader );
			} else {
				using( var connection = new SqliteConnection( _connectionString ) ) {
					connection.OpenAsync();

					result = PerformReader( connection, sql, parameters, loader );

					connection.Close();
				}
			}

			return result;
		}

		private async Task<IEnumerable<T>> PerformReaderAsync<T>( SqliteConnection connection, string sql, IDictionary<string, object> parameters, Func<DbDataReader, T> loader ) {
			var result = new List<T>();

			using( SqliteCommand command = connection.CreateCommand() ) {
				command.CommandText = sql;
				foreach( string key in parameters.Keys ) {
					command.Parameters.AddWithValue( key, parameters[key] );
				}

				using( DbDataReader reader = await command.ExecuteReaderAsync() ) {
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

		private IEnumerable<T> PerformReader<T>( SqliteConnection connection, string sql, IDictionary<string, object> parameters, Func<DbDataReader, T> loader ) {
			var result = new List<T>();

			using( SqliteCommand command = connection.CreateCommand() ) {
				command.CommandText = sql;
				foreach( string key in parameters.Keys ) {
					command.Parameters.AddWithValue( key, parameters[key] );
				}

				using( SqliteDataReader reader = command.ExecuteReader() ) {
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

		public async Task<T> ExecuteSingleReaderAsync<T>( string sql, IDictionary<string, object> parameters, Func<DbDataReader, T> loader, SqliteRepositoryTransaction transaction = null ) {
			T result = default;

			if( transaction?.Connection != null ) {
				result = await PerformSingleReaderAsync( transaction.Connection, sql, parameters, loader );
			} else {
				using( var connection = new SqliteConnection( _connectionString ) ) {
					await connection.OpenAsync();

					result = await PerformSingleReaderAsync( connection, sql, parameters, loader );

					connection.Close();
				}
			}

			return result;
		}

		public T ExecuteSingleReader<T>( string sql, IDictionary<string, object> parameters, Func<DbDataReader, T> loader, SqliteRepositoryTransaction transaction = null ) {
			T result = default;

			if( transaction?.Connection != null ) {
				result = PerformSingleReader( transaction.Connection, sql, parameters, loader );
			} else {
				using( var connection = new SqliteConnection( _connectionString ) ) {
					connection.Open();

					result = PerformSingleReader( connection, sql, parameters, loader );

					connection.Close();
				}
			}

			return result;
		}

		private async Task<T> PerformSingleReaderAsync<T>( SqliteConnection connection, string sql, IDictionary<string, object> parameters, Func<DbDataReader, T> loader ) {
			T result = default;

			using( SqliteCommand command = connection.CreateCommand() ) {
				command.CommandText = sql;
				foreach( string key in parameters.Keys ) {
					command.Parameters.AddWithValue( key, parameters[key] );
				}

				using( DbDataReader reader = await command.ExecuteReaderAsync() ) {
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

		private T PerformSingleReader<T>( SqliteConnection connection, string sql, IDictionary<string, object> parameters, Func<DbDataReader, T> loader ) {
			T result = default;

			using( SqliteCommand command = connection.CreateCommand() ) {
				command.CommandText = sql;
				foreach( string key in parameters.Keys ) {
					command.Parameters.AddWithValue( key, parameters[key] );
				}

				using( SqliteDataReader reader = command.ExecuteReader() ) {
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

		protected static string LoadString( DbDataReader reader ) {
			return reader.GetString( 0 );
		}

		protected static int LoadInt( DbDataReader reader ) {
			return reader.GetInt32( 0 );
		}

		protected static DateTimeOffset LoadDateTimeOffset( DbDataReader reader ) {
			if (reader.IsDBNull(0)) {
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
			if (reader.IsDBNull( index )) {
				return default;
			}
			return reader.GetString( index );
		}

		protected static DateTimeOffset GetDateTimeOffset( DbDataReader reader, string column ) {
			return new DateTimeOffset( reader.GetDateTime( reader.GetOrdinal( column ) ), TimeSpan.Zero );
		}

		protected static string ToText(DateTimeOffset value) {
			return value.ToUniversalTime().ToString( "yyyy-MM-dd HH:mm:ss" );
		}

		protected static DateTimeOffset? GetNullableDateTimeOffset( DbDataReader reader, string column ) {
			int index = reader.GetOrdinal( column );
			if( reader.IsDBNull( index ) ) {
				return default;
			}

			return new DateTimeOffset( reader.GetDateTime( index ), TimeSpan.Zero );
		}
	}
}
