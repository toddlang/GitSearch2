using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GitSearch2.Repository;
using GitSearch2.Repository.Sqlite;
using LibGit2Sharp;
using NUnit.Framework;

namespace GitSearch2.Indexer.Tests.Integration {

	[TestFixture]
	public sealed class CommitWalkerTests {

		private string _repoPath;
		private string _testFolder;
		private IRepository _repository;
		private ICommitWalker _walker;
		private ICommitRepository _commitRepository;

		[SetUp]
		public void SetUp() {
			_testFolder = Guid.NewGuid().ToString( "N" );
			_repoPath = LibGit2Sharp.Repository.Init( Path.Combine(_testFolder, "gitrepo") );
			_repository = new LibGit2Sharp.Repository( _repoPath );

			var sqliteOptions = new SqliteOptions() {
				ConnectionString = $"Data Source={Path.Combine( _testFolder, "integrationtests.sqlite" )}"
			};
			_commitRepository = new CommitSqliteRepository( sqliteOptions );
			_commitRepository.Initialize();
			IUpdateRepository updateRepository = new UpdateSqliteRepository( sqliteOptions );
			updateRepository.Initialize();
			var repoNameParser = new LocalNameParser();
			var statisticsDisplay = new TestStatisticsDisplay();
			var options = new Options() {
				GitFolder = _repoPath
			};
			var gitRepoProvider = new CyclingGitRepoProvider( options );
			_walker = new CommitWalker(
				_commitRepository,
				updateRepository,
				repoNameParser,
				statisticsDisplay,
				gitRepoProvider );
		}

		[TearDown]
		public void TearDown() {
			_repository.Dispose();
			Directory.Delete( _testFolder, true );
		}

		[Test]
		public void Run_EmptyRepository_NoWorkDone() {
			_walker.Run();

			Assert.AreEqual( 0, _commitRepository.CountCommits() );
		}

	}
}
