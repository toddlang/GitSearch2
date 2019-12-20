using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using GitSearch2.Repository;
using GitSearch2.Repository.Sqlite;
using LibGit2Sharp;
using NUnit.Framework;

namespace GitSearch2.Indexer.Tests.Integration {

	[TestFixture]
	public sealed class CommitWalkerTests {

		private string _repoPath; // Points to the .git folder
		private string _workingFolder; // Points to the folder above the .git
		private string _testFolder;
		private IRepository _repository;
		private ICommitWalker _walker;
		private ICommitRepository _commitRepository;
		private IGitRepoProvider _gitRepoProvider;

		[SetUp]
		public void SetUp() {
			_testFolder = Path.Combine( "DELETEME", Guid.NewGuid().ToString( "N" ) );
			_repoPath = LibGit2Sharp.Repository.Init( Path.Combine( _testFolder, "gitrepo" ), false );
			_workingFolder = Directory.GetParent( Directory.GetParent( _repoPath ).FullName ).FullName;
			_repository = new LibGit2Sharp.Repository( _repoPath );

			var sqliteOptions = new SqliteOptions() {
				ConnectionString = $"Data Source={Path.Combine( _testFolder, "integrationtests.sqlite" )}"
			};
			SqliteDb db = new SqliteDb( sqliteOptions );
			_commitRepository = new CommitSqliteRepository( db );
			_commitRepository.Initialize();
			var repoNameParser = new LocalNameParser();
			var statisticsDisplay = new TestStatisticsDisplay();
			var options = new Options() {
				GitFolder = _repoPath
			};
			_gitRepoProvider = new CyclingGitRepoProvider( options );
			_walker = new CommitWalker(
				_commitRepository,
				repoNameParser,
				statisticsDisplay,
				_gitRepoProvider );

			Signature author = new Signature( "Test", "test@localhost.com", DateTimeOffset.Now );
			Signature committer = author;
			_repository.Commit( "Initial commit", author, committer );
		}

		[TearDown]
		public void TearDown() {
			_repository.Dispose();
			RecursiveDelete( new DirectoryInfo( _testFolder ) );
		}

		[OneTimeTearDown]
		public void OneTimeTearDown() {
			Directory.Delete( "DELETEME", true );
		}

		[Test]
		public void Run_EmptyRepository_NoWorkDone() {
			_walker.Run();

			Assert.AreEqual( 0, _commitRepository.CountCommits() );
		}

		[Test]
		public void Run_OneMainlineCommit_OneCommitWritten() {
			AddCommit( "master", "1" );

			_walker.Run();

			Assert.AreEqual( 1, _commitRepository.CountCommits() );
		}

		[Test]
		public void Run_OneMergeCommit_OneCommitWritten() {
			AddCommit( "develop", "1" );
			MergeBranch( "develop", "master" );

			_walker.Run();

			Assert.AreEqual( 1, _commitRepository.CountCommits() );
		}

		[Test]
		public void Run_ContainsMergeFromMaster_OneCommitWritten() {
			AddCommit( "develop", "1" );
			AddCommit( "master", "2" );
			MergeBranch( "master", "develop" );
			AddCommit( "master", "3" );
			MergeBranch( "develop", "master" );

			_walker.Run();

			Assert.AreEqual( 3, _commitRepository.CountCommits() );
		}

		private void MergeBranch( string source, string target ) {
			Branch sourceBranch = Commands.Checkout( _repository, source );
			Commands.Checkout( _repository, target );
			Signature merger = new Signature( "Test", "test@localhost.com", DateTimeOffset.Now );
			MergeOptions options = new MergeOptions() {
				FastForwardStrategy = FastForwardStrategy.NoFastForward,				
			};
			_repository.Merge( sourceBranch, merger, options );
		}

		private void AddCommit( string branch, string message, int files = 1 ) {
			Branch branchHandle = _repository.Branches[branch];
			if( branchHandle is null ) {
				_repository.CreateBranch( branch );
			}
			Commands.Checkout( _repository, branch );

			for( int i = 0; i < files; i++ ) {
				string guid = Guid.NewGuid().ToString( "N" );
				string filename = $"{guid}.txt";
				string fullname = Path.Combine( _workingFolder, filename );
				File.WriteAllText( fullname, "Text content" );
				_repository.Index.Add( filename );
			}
			_repository.Index.Write();
			Signature author = new Signature( "Test", "test@localhost.com", DateTimeOffset.Now );
			Signature committer = author;

			_repository.Commit( message, author, committer );
		}

		// Read-only objects are created during the execution of the git code,
		// so this ensures that we can clean them up properly.
		private static void RecursiveDelete( DirectoryInfo baseDir ) {
			if( !baseDir.Exists ) {
				return;
			}

			foreach( DirectoryInfo dir in baseDir.EnumerateDirectories() ) {
				RecursiveDelete( dir );
			}
			FileInfo[] files = baseDir.GetFiles();
			foreach( FileInfo file in files ) {
				file.IsReadOnly = false;
				file.Delete();
			}
			baseDir.Delete();
		}
	}
}
