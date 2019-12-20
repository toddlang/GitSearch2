using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Moq;
using NUnit.Framework;

namespace GitSearch2.Repository.SqlServer.Tests.Unit {

	[TestFixture]
	public sealed class CommitSqlServerRepositoryTests {

		private Mock<IDb> _db;
		private CommitSqlServerRepository _repo;

		[SetUp]
		public void SetUp() {
			_db = new Mock<IDb>( MockBehavior.Strict );
			_repo = new CommitSqlServerRepository( _db.Object );
		}

		[Test]
		public void Initialize_NoSchema_SchemaCreated() {
			_db
				.Setup( db => db.ExecuteNonQuery( It.IsAny<string>() ) );


			_db
				.Setup( db => db.ExecuteSingleReader( It.IsAny<string>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<Func<DbDataReader, string>>() ))
				.Callback<string, IDictionary<string, object>, Func<DbDataReader, string>>( (_, p, __) => { Assert.AreEqual( "schemaid123", p["@settingId"] ); })
				.Returns( default(string) );

			_db
				.Setup( db => db.ExecuteNonQuery( It.IsAny<string>(), It.IsAny<IDictionary<string, object>>() ) )
				.Callback<string, IDictionary<string, object>>( ( _, p ) => {
					Assert.AreEqual( "schemaid123", p["@settingId"] );
					Assert.AreEqual( "1", p["@settingValue"] );
				} );

			_repo.Initialize( "schemaid123", 1 );

			_db.Verify( db => db.ExecuteNonQuery( It.IsAny<string>() ), Times.Exactly( 6 ) );
		}
	}
}
