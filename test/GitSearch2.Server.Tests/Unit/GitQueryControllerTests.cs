using System;
using System.Collections.Generic;
using GitSearch2.Repository;
using GitSearch2.Server.Controllers;
using GitSearch2.Shared;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GitSearch2.Server.Tests.Unit
{
	[TestFixture]
    public sealed class GitQueryControllerTests
    {
		private GitQueryController _gitQueryController;
		private Mock<ICommitRepository> _commitRepository;

		[SetUp]
		public void SetUp() {
			_commitRepository = new Mock<ICommitRepository>( MockBehavior.Strict );
			_gitQueryController = new GitQueryController( _commitRepository.Object );
		}

		[Test]
		public void Ctor_NullRepository_ThrowsException() {
			Assert.Throws<ArgumentException>( () => { new GitQueryController( null ); } );
		}

		[Test]
		public void Search_NullRequest_ReturnsBadRequest() {
			ActionResult<IEnumerable<CommitDetails>> response = _gitQueryController.Search( null );

			Assert.IsInstanceOf<BadRequestResult>( response.Result );
		}

		[Test]
		public void Search_ValidRequest_ReturnsOk() {
			GitQuery query = new GitQuery( "term", 0, 1 );
			var commits = new List<CommitDetails>();
			_commitRepository
				.Setup( cr => cr.Search( "term", 1 ) )
				.Returns( commits );
			ActionResult<IEnumerable<CommitDetails>> response = _gitQueryController.Search( query );

			Assert.IsInstanceOf<OkObjectResult>( response.Result );

		}

		[Test]
		public void Search_ValidRequestNullResult_Returns500() {
			GitQuery query = new GitQuery( "term", 0, 1 );
			var commits = new List<CommitDetails>();
			_commitRepository
				.Setup( cr => cr.Search( "term", 1 ) )
				.Returns( default(IEnumerable<CommitDetails>) );
			ActionResult<IEnumerable<CommitDetails>> response = _gitQueryController.Search( query );

			Assert.IsInstanceOf<StatusCodeResult>( response.Result );
			StatusCodeResult result = (StatusCodeResult)response.Result;
			Assert.AreEqual( 500, result.StatusCode );
		}
	}
}
