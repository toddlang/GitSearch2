using GitSearch2.Repository;
using Moq;
using NUnit.Framework;

namespace GitSearch2.Indexer.Tests.Unit {
	[TestFixture]
	public class CommitWalkerTests {

		private CommitWalker _commitWalker;
		private Mock<ICommitRepository> _commitRepository;
		private Mock<IUpdateRepository> _updateRepository;

		[SetUp]
		public void Setup() {
			_commitRepository = new Mock<ICommitRepository>( MockBehavior.Strict );
			_updateRepository = new Mock<IUpdateRepository>( MockBehavior.Strict );
			Options options = new Options() {
				GitFolder = @"g:\it\folder",
				LiveStatisticsDisplay = false,
				PauseWhenComplete = false
			};
			_commitWalker = new CommitWalker( _commitRepository.Object, _updateRepository.Object, options );
		}

		[Test]
		public void Test1() {
			Assert.Pass();
		}
	}
}
