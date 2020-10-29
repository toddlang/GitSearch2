using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace GitSearch2.Shared.Tests.Unit {

	[TestFixture]
	public sealed class CommitDetailsTests {

		[Test]
		public void Ctor_ValidProperties_CorrectPropertiesSet() {
			var description = new List<string>() { "description" };
			var files = new List<string>() { "files" };
			var commits = new List<string>() { "commits" };
			var details = new CommitDetails( description, "repo", "authorEmail", "authorName", "date", files, "commitId", "project", "pr", commits, true, "github" );

			CollectionAssert.AreEqual( description, details.Description );
			Assert.AreEqual( "repo", details.Repo );
			Assert.AreEqual( "authorEmail", details.AuthorEmail );
			Assert.AreEqual( "authorName", details.AuthorName );
			Assert.AreEqual( "date", details.Date );
			CollectionAssert.AreEqual( files, details.Files );
			Assert.AreEqual( "commitId", details.CommitId );
			Assert.AreEqual( "project", details.Project );
			Assert.AreEqual( "pr", details.PR );
			CollectionAssert.AreEqual( commits, details.Commits );
			Assert.AreEqual( true, details.IsMerge );
			Assert.AreEqual( "github", details.OriginId );
		}

		[Test]
		public void Ctor_NullDescription_ThrowsArgumentException() {
			Assert.Throws<ArgumentException>( () => { new CommitDetails( null, "repo", "authorEmail", "authorName", "date", new List<string>(), "commitId", "project", "pr", new List<string>(), false, "github" ); } );
		}

		[TestCase( default(string ))]
		[TestCase( "" )]
		[TestCase( " ")]
		public void Ctor_BadRepo_ThrowsArgumentException( string repo ) {
			Assert.Throws<ArgumentException>( () => { new CommitDetails( new List<string>(), repo, "authorEmail", "authorName", "date", new List<string>(), "commitId", "project", "pr", new List<string>(), false, "github" ); } );
		}

		[TestCase( default( string ) )]
		[TestCase( "" )]
		[TestCase( " " )]
		public void Ctor_BadAuthorName_ThrowsArgumentException( string authorName ) {
			Assert.Throws<ArgumentException>( () => { new CommitDetails( new List<string>(), "repo", "authorEmail", authorName, "date", new List<string>(), "commitId", "project", "pr", new List<string>(), false, "github" ); } );
		}

		[TestCase( default( string ) )]
		[TestCase( "" )]
		[TestCase( " " )]
		public void Ctor_BadDate_ThrowsArgumentException( string date ) {
			Assert.Throws<ArgumentException>( () => { new CommitDetails( new List<string>(), "repo", "authorEmail", "authorName", date, new List<string>(), "commitId", "project", "pr", new List<string>(), false, "github" ); } );
		}

		[TestCase( default( string ) )]
		[TestCase( "" )]
		[TestCase( " " )]
		public void Ctor_BadCommitId_ThrowsArgumentException( string commitId ) {
			Assert.Throws<ArgumentException>( () => { new CommitDetails( new List<string>(), "repo", "authorEmail", "authorName", "date", new List<string>(), commitId, "project", "pr", new List<string>(), false, "github" ); } );
		}
	}
}
