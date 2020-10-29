using System;
using System.Collections.Generic;
using System.Data.Common;

namespace GitSearch2.Repository.Sqlite {

	public class SqliteRepository {

		public SqliteRepository( IDb db ) {
			if ( db is null ) { 
				throw new ArgumentException( "IDb instance not specified.", nameof( db ) );
			}

			Db = db;
		}

		public IDb Db { get; }

		public void Initialize(
			string schemaId,
			int targetSchema
		) {
			const string sqlCreateTable = @"
                CREATE TABLE IF NOT EXISTS SETTINGS
                (
				    SETTING_ID NVARCHAR(32) NOT NULL PRIMARY KEY,
				    SETTING_VALUE TEXT NOT NULL
                )
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

			string result = Db.ExecuteSingleReader( sqlGetValue, parameters, Db.LoadString );
			int currentSchema = int.Parse( result ?? "0" );

			if( currentSchema == 0 ) {
				CreateSchema();

				const string sqlInsertSetting = @"
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
				Db.ExecuteNonQuery( sqlInsertSetting, parameters );

			} else if( currentSchema < targetSchema ) {

				for( int version = currentSchema + 1; version <= targetSchema; version++ ) {
					UpdateSchema( version );
				}

				const string sqlUpdateSetting = @"
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
				Db.ExecuteNonQuery( sqlUpdateSetting, parameters );
			}
		}


		protected virtual void CreateSchema() {
			throw new NotImplementedException();
		}

		protected virtual void UpdateSchema( int targetSchema ) {
			throw new NotImplementedException();
		}

	}
}
