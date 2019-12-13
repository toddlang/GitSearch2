using System;
using System.Collections.Generic;
using System.Linq;
using GitSearch2.Repository;
using GitSearch2.Shared;
using LibGit2Sharp;
using Moq;
using NUnit.Framework;

namespace GitSearch2.Indexer.Tests.Unit {
	[TestFixture]
	public class CommitWalkerTests {

		private const string ProjectName = "unit";
		private const string RepoName = "tests";
		private const string RepoUrl = "unit/tests.git";

		private ICommitWalker _commitWalker;
		private Mock<ICommitRepository> _commitRepository;
		private Mock<INameParser> _nameParser;
		private Mock<IStatisticsDisplay> _statisticsDisplay;
		private Mock<IGitRepoProvider> _gitRepoProvider;
		private Mock<IRepository> _repository;
		private Mock<IQueryableCommitLog> _commits;

		[SetUp]
		public void Setup() {
			_commitRepository = new Mock<ICommitRepository>( MockBehavior.Strict );
			_nameParser = new Mock<INameParser>( MockBehavior.Strict );
			_statisticsDisplay = new Mock<IStatisticsDisplay>( MockBehavior.Strict );
			_gitRepoProvider = new Mock<IGitRepoProvider>( MockBehavior.Strict );
			_repository = new Mock<IRepository>( MockBehavior.Strict );
			_commits = new Mock<IQueryableCommitLog>( MockBehavior.Strict );
			_statisticsDisplay.Setup( sd => sd.UpdateStatistics( It.IsAny<IStatistics>() ) );

			SetupNameParser( _nameParser );
			SetupRepository( _repository, _commits );
			SetupGitRepoProvider( _gitRepoProvider, _repository.Object );

			_commitWalker = new CommitWalker(
				_commitRepository.Object,
				_nameParser.Object,
				_statisticsDisplay.Object,
				_gitRepoProvider.Object );
		}

		[Test]
		public void Run_EmptyRepository_NoWorkDone() {

			SetupCommitLog( _commits, new List<Commit>() );

			int commitsWritten = _commitWalker.Run();
			Assert.AreEqual( 0, commitsWritten );
		}

		[Test]
		public void Run_OnlyPrehistory_NoWorkDone() {

			var commits = new List<Commit>() {
				CreateCommit(
					_commitRepository,
					_gitRepoProvider,
					_repository,
					true,
					CommitWalker.PreHistoryId,
					new List<Commit>() {
						CreateRootCommit(
							_commitRepository,
							_gitRepoProvider,
							_repository,
							true )
					} )
			};

			SetupCommitLog( _commits, commits );

			int commitsWritten = _commitWalker.Run();
			Assert.AreEqual( 0, commitsWritten );
		}

		[Test]
		public void Run_OneMainlineCommit_OneCommitWritten() {

			var commits = new List<Commit>() {
				CreateCommit(
					_commitRepository,
					_gitRepoProvider,
					_repository,
					false,
					"a94a8fe5ccb19ba61c4c0873d391e987982fbbd3",
					new List<Commit>() {
						CreateRootCommit(
							_commitRepository,
							_gitRepoProvider,
							_repository,
							true )
					} )
			};

			SetupCommitLog( _commits, commits );

			int commitsWritten = _commitWalker.Run();
			Assert.AreEqual( 1, commitsWritten );
		}

		[Test]
		public void Run_MergeWithOneCommit_OneCommitWritten() {

			Commit rootCommit = CreateRootCommit(
							_commitRepository,
							_gitRepoProvider,
							_repository,
							true );

			Commit realCommit = CreateCommit(
					_commitRepository,
					_gitRepoProvider,
					_repository,
					false,
					"a94a8fe5ccb19ba61c4c0873d391e987982fbbd3",
					new List<Commit>() {
						rootCommit
					},
					"real" );

			Commit mergeCommit = CreateCommit(
				_commitRepository,
				_gitRepoProvider,
				_repository,
				false,
				"14091a9f2461267ee7e02525b4f1f2923f1c9849",
				new List<Commit>() {
					rootCommit,
					realCommit
				},
				"1" );

			var commits = new List<Commit>() {
				mergeCommit,
				realCommit,
				rootCommit
			};

			SetupCommitLog( _commits, commits );

			int commitsWritten = _commitWalker.Run();
			Assert.AreEqual( 1, commitsWritten );
		}

		private static Commit CreateRootCommit(
			Mock<ICommitRepository> commitRepo,
			Mock<IGitRepoProvider> gitRepo,
			Mock<IRepository> repository,
			bool includeInRepo
		) {
			return CreateCommit( commitRepo, gitRepo, repository, includeInRepo, "dc76e9f0c0006e8f919e0c515c66dbba3982f785", new List<Commit>(), "root" );
		}

		private static Commit CreateCommit(
			Mock<ICommitRepository> commitRepo,
			Mock<IGitRepoProvider> gitRepo,
			Mock<IRepository> repository,
			bool includeInRepo,
			string sha,
			IEnumerable<Commit> parents
		) {
			return CreateCommit( commitRepo, gitRepo, repository, includeInRepo, sha, parents, "author", "email", DateTimeOffset.Now, "message", new List<string>() );
		}

		private static Commit CreateCommit(
			Mock<ICommitRepository> commitRepo,
			Mock<IGitRepoProvider> gitRepo,
			Mock<IRepository> repository,
			bool includeInRepo,
			string sha,
			IEnumerable<Commit> parents,
			string message
		) {
			return CreateCommit( commitRepo, gitRepo, repository, includeInRepo, sha, parents, "author", "email", DateTimeOffset.Now, message, new List<string>() );
		}

		private static Commit CreateCommit(
			Mock<ICommitRepository> commitRepo,
			Mock<IGitRepoProvider> gitRepo,
			Mock<IRepository> repository,
			bool includeInRepo,
			string sha,
			IEnumerable<Commit> parents,
			string author,
			string email,
			DateTimeOffset when,
			string message,
			IEnumerable<string> files
		) {
			var commit = new Mock<Commit>( MockBehavior.Strict );
			commit
				.SetupGet( c => c.Sha )
				.Returns( sha );

			commit
				.SetupGet( c => c.Id )
				.Returns( new ObjectId( sha ) );

			commit
				.SetupGet( c => c.MessageShort )
				.Returns( message );

			commit
				.SetupGet( c => c.Parents )
				.Returns( parents );

			commit
				.SetupGet( c => c.Author )
				.Returns( new Signature( author, email, when ) );

			commit
				.SetupGet( c => c.Committer )
				.Returns( new Signature( author, email, when ) );

			commitRepo
				.Setup( cr => cr.ContainsCommit( sha, ProjectName, RepoName ) )
				.Returns( includeInRepo );

			if (!includeInRepo) {
				commitRepo
					.Setup( cr => cr.Add( It.IsAny<CommitDetails>() ) );
			}

			if (parents.Count() > 1) {
				repository
					.Setup( r => r.Lookup( sha, ObjectType.Commit ) )
					.Returns( commit.Object );

				commit
					.SetupGet( c => c.Message )
					.Returns( $"{CommitWalker.MergePrNumberToken}{message} in {ProjectName}/{RepoName}" );

			} else {
				commit
					.SetupGet( c => c.Message )
					.Returns( message );
			}

			// We have to use It.IsAny here because using commit.Object will
			// never succeed at matching the setup.
			gitRepo
				.Setup( gr => gr.GetCommitFiles( It.IsAny<Commit>() ) ) // commit.Object
				.Returns( files );

			return commit.Object;
		}

		private static void SetupCommitLog( Mock<IQueryableCommitLog> commitLog, IEnumerable<Commit> commits ) {
			commitLog
				.Setup( cl => cl.GetEnumerator() )
				.Returns( commits.GetEnumerator() );
		}

		private static void SetupNameParser( Mock<INameParser> nameParser ) {
			var repoProjectName = new RepoProjectName( RepoName, ProjectName );
			nameParser
				.Setup( np => np.Parse( It.IsAny<IRepository>() ) )
				.Returns( repoProjectName );
		}

		private static void SetupRepository( Mock<IRepository> repository, Mock<IQueryableCommitLog> commits ) {
			var mockRemote = new Mock<Remote>( MockBehavior.Strict );
			mockRemote
				.SetupGet( mr => mr.Url )
				.Returns( RepoUrl );

			var mockRemotes = new List<Remote>() {
				mockRemote.Object
			};

			var remotes = new Mock<RemoteCollection>( MockBehavior.Strict );
			remotes
				.Setup( r => r.GetEnumerator() )
				.Returns( mockRemotes.GetEnumerator() );

			var network = new Mock<Network>( MockBehavior.Strict );
			network
				.SetupGet( n => n.Remotes )
				.Returns( remotes.Object );

			repository
				.SetupGet( r => r.Network )
				.Returns( network.Object );

			repository
				.SetupGet( r => r.Commits )
				.Returns( commits.Object );
		}

		private static void SetupGitRepoProvider( Mock<IGitRepoProvider> provider, IRepository repository ) {
			provider
				.Setup( p => p.GetRepo() )
				.Returns( repository );

			provider
				.Setup( p => p.GetRepo( It.IsAny<Commit>(), out It.Ref<Commit>.IsAny ))
				.Callback( new GetRepoCallback( (Commit inCommit, out Commit outCommit) => { outCommit = inCommit; } ) )
				.Returns( repository );
		}

		private delegate void GetRepoCallback( Commit inCommit, out Commit outCommit );
	}
}
