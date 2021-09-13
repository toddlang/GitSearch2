using LibGit2Sharp;

namespace GitSearch2.Indexer {
	internal sealed class CyclingGitRepoProvider: IGitRepoProvider {

		private const int DiffCycle = 1000;

		private readonly Options _options;
		private LibGit2Sharp.Repository _gitRepo;
		private int _diffCalls;

		public CyclingGitRepoProvider(
			Options options
		) {
			_options = options;
		}

		/// <summary>
		/// Retrieves a reference to the git repository.
		/// </summary>
		/// <returns>
		/// Returns the reference to the git repository to be used.
		/// </returns>
		/// <remarks>
		/// Unfortunately, LibGit2Sharp loses performance over time when calling
		/// for diffs.  That is...the time it takes to calculate the diff between
		/// commits climbs dramatically the more times it is called.   So, after
		/// a certain number of operations we will recycle the Repository reference
		/// and this seems to clear the problem.
		/// </remarks>
		IRepository IGitRepoProvider.GetRepo() {
			if( _gitRepo == null ) {
				_gitRepo = new LibGit2Sharp.Repository( _options.GitFolder );
				return _gitRepo;
			}

			if( _diffCalls >= DiffCycle ) {
				_gitRepo.Dispose();
				_diffCalls = 0;
				_gitRepo = new LibGit2Sharp.Repository( _options.GitFolder );
				return _gitRepo;
			}

			return _gitRepo;
		}

		/// <summary>
		/// Retrieves a reference to the git repository, optionally refreshing
		/// the currently processed commit to ensure it is pointing at the
		/// correct repository instance.
		/// </summary>
		/// <param name="current">The currently processed commit.</param>
		/// <param name="newCurrent">The current commit now associated with the new repository.</param>
		/// <returns>
		/// Returns the reference to the git repository to be used.
		/// </returns>
		/// <remarks>
		/// Unfortunately, LibGit2Sharp loses performance over time when calling
		/// for diffs.  That is...the time it takes to calculate the diff between
		/// commits climbs dramatically the more times it is called.   So, after
		/// a certain number of operations we will recycle the Repository reference
		/// and this seems to clear the problem.
		/// </remarks>
		IRepository IGitRepoProvider.GetRepo( Commit current, out Commit newCurrent ) {
			if( _diffCalls >= DiffCycle ) {
				ObjectId commitId = current?.Id;
				_gitRepo.Dispose();
				_diffCalls = 0;
				_gitRepo = new LibGit2Sharp.Repository( _options.GitFolder );
				newCurrent = commitId != null ? _gitRepo.Lookup<Commit>( commitId ) : null;
				return _gitRepo;
			}

			newCurrent = current;
			return _gitRepo;
		}

		IEnumerable<string> IGitRepoProvider.GetCommitFiles( Commit commit ) {
			var files = new List<string>();

			CompareOptions options = new LibGit2Sharp.CompareOptions() {
				IncludeUnmodified = false
			};
			// We need to track the calls for diffs since LibGit2Sharp experiences
			// some massive performance loss the more this method is called.
			// Once we reach a pre-set limit, we recycle the repo object and this
			// will clear up the performance problem.
			_diffCalls += 1;
			TreeChanges treeChanges = _gitRepo.Diff.Compare<TreeChanges>( commit.Parents.First().Tree, commit.Tree, options );
			foreach( TreeEntryChanges change in treeChanges ) {
				files.Add( change.Path.Replace( @"\", @"/" ) );
			}

			return files;
		}
	}
}
