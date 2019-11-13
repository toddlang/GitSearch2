using System;
using NUnit.Framework;

namespace GitSearch2.Shared.Tests.Unit {
	[TestFixture]
	public sealed class GitQueryTests {

		[Test]
		public void Ctor_DistinctProperties_CorrectPropertiesSet() {
			GitQuery query = new GitQuery( "term", 1, 2 );

			Assert.AreEqual( "term", query.SearchTerm );
			Assert.AreEqual( 1, query.StartRecord );
			Assert.AreEqual( 2, query.MaximumRecords );
		}

		[Test]
		public void Ctor_BadSearchTerm_ThrowsArgumentException() {
			Assert.Throws<ArgumentException>( () => { new GitQuery( "", 1, 2 ); } );
		}

		[TestCase( -1 )]
		[TestCase( int.MinValue )]
		public void Ctor_BadStartRecord_ThrowsArgumentException( int startRecord ) {
			Assert.Throws<ArgumentException>( () => { new GitQuery( "term", startRecord, 2 ); } );
		}

		[TestCase( 0 )]
		[TestCase( -1 )]
		[TestCase( int.MinValue )]
		public void Ctor_BadMaximumRecords_ThrowsArgumentException( int maximumRecords ) {
			Assert.Throws<ArgumentException>( () => { new GitQuery( "term", 1, maximumRecords ); } );
		}

		[Test]
		public void ObjectEquals_OtherIsNull_ReturnsFalse() {
			GitQuery query = new GitQuery( "term", 0, 1 );

			Assert.IsFalse( query.Equals( null ) );
		}

		[Test]
		public void TypedEquals_OtherIsNull_ReturnsFalse() {
			GitQuery query = new GitQuery( "term", 0, 1 );

			Assert.IsFalse( query.Equals( default ) );
		}

		[Test]
		public void TypedEquals_OtherIsSame_ReturnsFalse() {
			GitQuery query = new GitQuery( "term", 0, 1 );

			Assert.IsTrue( query.Equals( query ) );
		}

		[Test]
		public void GetHashCode_SameValues_CodeIsSame() {
			GitQuery query1 = new GitQuery( "term", 0, 1 );
			GitQuery query2 = new GitQuery( "term", 0, 1 );

			Assert.AreEqual( query1.GetHashCode(), query2.GetHashCode() );
		}

		[Test]
		public void GetHashCode_DifferentStart_CodeIsDifferent() {
			GitQuery query1 = new GitQuery( "term", 1, 1 );
			GitQuery query2 = new GitQuery( "term", 0, 1 );

			Assert.AreNotEqual( query1.GetHashCode(), query2.GetHashCode() );
		}

		[Test]
		public void GetHashCode_DifferentMaximum_CodeIsDifferent() {
			GitQuery query1 = new GitQuery( "term", 0, 1 );
			GitQuery query2 = new GitQuery( "term", 0, 2 );

			Assert.AreNotEqual( query1.GetHashCode(), query2.GetHashCode() );
		}

		[Test]
		public void GetHashCode_DifferentTerm_CodeIsDifferent() {
			GitQuery query1 = new GitQuery( "term", 0, 1 );
			GitQuery query2 = new GitQuery( "term2", 0, 1 );

			Assert.AreNotEqual( query1.GetHashCode(), query2.GetHashCode() );
		}
	}
}
