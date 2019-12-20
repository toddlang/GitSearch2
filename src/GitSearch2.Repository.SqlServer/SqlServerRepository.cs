using System;
using System.Collections.Generic;
using System.Data.Common;

namespace GitSearch2.Repository.SqlServer {
	public class SqlServerRepository {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities

		public SqlServerRepository(
			IDb db
		) {
			if( db is null ) {
				throw new ArgumentNullException( nameof( db ) );
			}

			Db = db;
		}

		public IDb Db { get; }

		public void Initialize(
			string schemaId,
			int targetSchema
		) {
			const string sqlCreateTable = @"
				IF NOT EXISTS (
					SELECT
						'X'
					FROM
						INFORMATION_SCHEMA.TABLES
					WHERE
						TABLE_NAME = 'SETTINGS'
				)
				BEGIN
					CREATE TABLE SETTINGS
					(
						SETTING_ID NVARCHAR(32) NOT NULL PRIMARY KEY,
						SETTING_VALUE TEXT NOT NULL
					)
				END
			;";

			Db.ExecuteNonQuery( sqlCreateTable );

			const string sqlGetValue = @"
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

			string result = Db.ExecuteSingleReader( sqlGetValue, parameters, LoadString );
			int currentSchema = int.Parse( result ?? "0" );

			if( currentSchema == 0 ) {
				CreateSchema();

				const string sqlInsertValue = @"
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
				Db.ExecuteNonQuery( sqlInsertValue, parameters );

			} else if( currentSchema < targetSchema ) {

				for( int version = currentSchema + 1; version <= targetSchema; version++ ) {
					UpdateSchema( version );
				}

				const string sqlUpdateValue = @"
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
				Db.ExecuteNonQuery( sqlUpdateValue, parameters );
			}
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
			return value.ToUniversalTime().ToString( "yyyy-MM-ddTHH:mm:ss" );
		}

		protected static string ToText( DateTime value ) {
			return value.ToUniversalTime().ToString( "yyyy-MM-ddTHH:mm:ss" );
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

#pragma warning restore CA2100
	}
}
