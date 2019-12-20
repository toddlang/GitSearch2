using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace GitSearch2.Repository.Sqlite {
	internal sealed class SqliteDb: IDb {

		private readonly string _connectionString;

		public SqliteDb( IOptions<SqliteOptions> options )
			:this( options.Value ) {
		}

		public SqliteDb( SqliteOptions options ) {

			if( options is null ) {
				throw new ArgumentNullException( nameof( options ) );
			}

			if( string.IsNullOrWhiteSpace( options.ConnectionString ) ) {
				throw new ArgumentException( nameof( options ) );
			}

			_connectionString = options.ConnectionString;
		}

		public async Task ExecuteNonQueryAsync( string sql ) {
			using( var connection = new SqliteConnection( _connectionString ) ) {
				await connection.OpenAsync();

				await PerformNonQueryAsync( connection, sql );

				connection.Close();
			}
		}

		public void ExecuteNonQuery( string sql ) {
			using( var connection = new SqliteConnection( _connectionString ) ) {
				connection.Open();

				PerformNonQuery( connection, sql );

				connection.Close();
			}
		}

		public async Task ExecuteNonQueryAsync(
			string sql,
			IDictionary<string, object> parameters
		) {
			using( var connection = new SqliteConnection( _connectionString ) ) {
				await connection.OpenAsync();

				await PerformNonQueryAsync( connection, sql, parameters );

				connection.Close();
			}
		}

		public void ExecuteNonQuery(
			string sql,
			IDictionary<string, object> parameters
		) {
			using( var connection = new SqliteConnection( _connectionString ) ) {
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
			using( var connection = new SqliteConnection( _connectionString ) ) {
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

			using( var connection = new SqliteConnection( _connectionString ) ) {
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

			using( var connection = new SqliteConnection( _connectionString ) ) {
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

			using( var connection = new SqliteConnection( _connectionString ) ) {
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

			using( var connection = new SqliteConnection( _connectionString ) ) {
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

			using( var connection = new SqliteConnection( _connectionString ) ) {
				connection.Open();

				result = PerformSingleReader( connection, sql, parameters, loader );

				connection.Close();
			}

			return result;
		}

		private async Task PerformNonQueryAsync(
			SqliteConnection connection,
			string sql
		) {
			using( SqliteCommand command = connection.CreateCommand() ) {
				command.CommandText = sql;

				await command.ExecuteNonQueryAsync();
			}
		}

		private void PerformNonQuery(
			SqliteConnection connection,
			string sql
		) {
			using( SqliteCommand command = connection.CreateCommand() ) {
				command.CommandText = sql;

				command.ExecuteNonQuery();
			}
		}

		private async Task PerformNonQueryAsync(
			SqliteConnection connection,
			string sql,
			IDictionary<string, object> parameters
		) {
			using( SqliteCommand command = connection.CreateCommand() ) {
				command.CommandText = sql;
				foreach( string key in parameters.Keys ) {
					command.Parameters.AddWithValue( key, parameters[key] );
				}

				await command.ExecuteNonQueryAsync();
			}
		}

		private void PerformNonQuery(
			SqliteConnection connection,
			string sql,
			IDictionary<string, object> parameters
		) {
			using( SqliteCommand command = connection.CreateCommand() ) {
				command.CommandText = sql;
				foreach( string key in parameters.Keys ) {
					command.Parameters.AddWithValue( key, parameters[key] );
				}

				command.ExecuteNonQuery();
			}
		}

		private async Task<IEnumerable<T>> PerformReaderAsync<T>(
			SqliteConnection connection,
			string sql,
			Func<DbDataReader, T> loader
		) {
			var result = new List<T>();
			using( SqliteCommand command = connection.CreateCommand() ) {
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
			SqliteConnection connection,
			string sql,
			Func<DbDataReader, T> loader
		) {
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

		private async Task<IEnumerable<T>> PerformReaderAsync<T>(
			SqliteConnection connection,
			string sql,
			IDictionary<string, object> parameters,
			Func<DbDataReader, T> loader
		) {
			var result = new List<T>();

			using( SqliteCommand command = connection.CreateCommand() ) {
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
			SqliteConnection connection,
			string sql,
			IDictionary<string, object> parameters,
			Func<DbDataReader, T> loader
		) {
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

		private async Task<T> PerformSingleReaderAsync<T>(
			SqliteConnection connection,
			string sql,
			IDictionary<string, object> parameters,
			Func<DbDataReader, T> loader
		) {
			T result = default;

			using( SqliteCommand command = connection.CreateCommand() ) {
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
			SqliteConnection connection,
			string sql,
			IDictionary<string, object> parameters,
			Func<DbDataReader, T> loader
		) {
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
	}
}
