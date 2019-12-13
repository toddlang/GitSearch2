using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitSearch2.Repository;
using GitSearch2.Server.Controllers;
using GitSearch2.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace GitSearch2.Server.Tests.Unit {
	[TestFixture]
	public sealed class GitQueryControllerTests {
		private GitQueryController _gitQueryController;
		private Mock<ICommitRepository> _commitRepository;
		private Mock<ILogger<GitQueryController>> _logger;

		[SetUp]
		public void SetUp() {
			_commitRepository = new Mock<ICommitRepository>( MockBehavior.Strict );
			_logger = new Mock<ILogger<GitQueryController>>();
			_gitQueryController = new GitQueryController( _logger.Object, _commitRepository.Object );
		}

		[Test]
		public void Ctor_NullRepository_ThrowsException() {
			Assert.Throws<ArgumentException>( () => { new GitQueryController( _logger.Object, null ); } );
		}

		[Test]
		public async Task Search_NullRequest_ReturnsBadRequest() {
			ActionResult<GitQueryResponse> response = await _gitQueryController.SearchAsync( null );

			Assert.IsInstanceOf<BadRequestResult>( response.Result );
		}

		[Test]
		public async Task Search_ValidRequest_ReturnsOk() {
			GitQuery query = new GitQuery( "term", 0, 1 );
			IEnumerable<CommitDetails> commits = new List<CommitDetails>();
			_commitRepository
				.Setup( cr => cr.SearchAsync( "term", 1 ) )
				.Returns( Task.FromResult( commits ) );
			ActionResult<GitQueryResponse> response = await _gitQueryController.SearchAsync( query );

			Assert.IsInstanceOf<OkObjectResult>( response.Result );

		}

		[Test]
		public async Task Search_ValidRequestNullResult_Returns500() {
			GitQuery query = new GitQuery( "term", 0, 1 );
			_commitRepository
				.Setup( cr => cr.SearchAsync( "term", 1 ) )
				.Returns( Task.FromResult( default( IEnumerable<CommitDetails> ) ) );
			ActionResult<GitQueryResponse> response = await _gitQueryController.SearchAsync( query );

			Assert.IsInstanceOf<StatusCodeResult>( response.Result );
			StatusCodeResult result = (StatusCodeResult)response.Result;
			Assert.AreEqual( 500, result.StatusCode );
		}
	}
}
