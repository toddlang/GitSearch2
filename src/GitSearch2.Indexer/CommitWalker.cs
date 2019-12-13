using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GitSearch2.Repository;
using GitSearch2.Shared;
using LibGit2Sharp;

namespace GitSearch2.Indexer {

	internal class CommitWalker : ICommitWalker {

		internal const string PreHistoryId = "53ef49c5f0b2711409a7e295a2d515e70850415e";
		internal const string MergePrNumberToken = "Merge pull request #";

		private readonly INameParser _repoNameParser;
		private readonly ICommitRepository _commitRepository;
		private readonly IStatisticsDisplay _statisticsDisplay;
		private readonly IGitRepoProvider _gitRepoProvider;

		private IStatistics _statistics;

		public CommitWalker(
			ICommitRepository commitRepository,
			INameParser repoNameParser,
			IStatisticsDisplay statisticsDisplay,
			IGitRepoProvider gitRepoProvider
		) {
			_commitRepository = commitRepository;
			_repoNameParser = repoNameParser;
			_statisticsDisplay = statisticsDisplay;
			_gitRepoProvider = gitRepoProvider;
		}

		int ICommitWalker.Run() {

			HashSet<string> visited = new HashSet<string>();
			Stack<string> toVisit = new Stack<string>();

			_statistics = new Statistics( visited, toVisit );

			IRepository gitRepo = _gitRepoProvider.GetRepo();
			RepoProjectName name = _repoNameParser.Parse( gitRepo );

			// Short-circuit pre-history so that we never try to walk
			// farther back than this point.
			visited.Add( PreHistoryId );

			Commit current = gitRepo.Commits.FirstOrDefault();

			while( !current?.Sha.Equals( PreHistoryId, StringComparison.Ordinal ) ?? false ) {
				int parentCount = current.Parents.Count();

				// Skip it if we've already seen it, otherwise make a note
				// that we're visiting it.
				if( !visited.Contains( current.Sha ) ) {
					visited.Add( current.Sha );

					// If it has one parent, it's a "real" commit and needs
					// to be written to the DB.   It's a direct commit to
					// master though, so we have no PR and no merge via.
					if( parentCount == 1 ) {
						_statistics.ProcessedCommit();
						WriteCommit( current, name, "", Enumerable.Empty<string>() );

					} else if( parentCount > 1 ) {
						toVisit.Push( current.Sha );

					} else {
						// This is the initial commit, soooo...why are we here?

					}
				}

				current = current.Parents.FirstOrDefault();
				_statisticsDisplay.UpdateStatistics( _statistics );

				gitRepo = _gitRepoProvider.GetRepo( current, out current );
			}

			// Now we run through every commit that we "skipped" in the previous pass
			while( toVisit.Any() ) {
				current = gitRepo.Lookup<Commit>( toVisit.Pop() );
				_statistics.ProcessedCommit();

				var mergeVia = new List<string>() {
					current.Sha
				};
				string prNumber = GetPrNumber( current );

				foreach( Commit target in current.Parents ) {
					if( !visited.Contains( target.Sha ) ) {
						_statistics.ProcessedCommit();
						if( !IsMergeInto( target ) ) {
							WalkMerge( gitRepo, mergeVia, name, prNumber, visited, toVisit, target );

						} else {
							visited.Add( target.Sha );

						}
					}
				}

				_statisticsDisplay.UpdateStatistics( _statistics );

				gitRepo = _gitRepoProvider.GetRepo();
			}

			return _statistics.Written;
		}

		/// <summary>
		/// For the supplied merge node, we will walk all parents finding
		/// all regular commits in its graph until we reach nodes we've
		/// already visited.
		/// </summary>
		/// <param name="gitRepo">
		/// The repository instance to use.
		/// </param>
		/// <param name="mergeVia">
		/// The list of "child" nodes that were walked to reach this point.
		/// </param>
		/// <param name="projectName">
		/// The name of the project in BitBucket
		/// </param>
		/// <param name="repoName">
		/// The repo being processed.
		/// </param>
		/// <param name="prNumber">
		/// The PR number that caused any commits found to be merged.
		/// </param>
		/// <param name="visited">
		/// The list of all nodes we have already visited.
		/// </param>
		/// <param name="target">
		/// The target node being walked.
		/// </param>
		private void WalkMerge(
			IRepository gitRepo,
			List<string> mergeVia,
			RepoProjectName name,
			string prNumber,
			HashSet<string> visited,
			Stack<string> toVisit,
			Commit target
		) {
			int parentCount = target.Parents.Count();
			// We only get in here if a node hasn't been visited before so it's
			// safe to unconditionally add this to the list of visited nodes.
			visited.Add( target.Sha );
			_statistics.ProcessedCommit();

			// We make a new mergevia list adding the current merge node
			// to the history list.
			var newMergeVia = new List<string>( mergeVia ) {
					target.Sha
				};

			_statisticsDisplay.UpdateStatistics( _statistics );

			if( parentCount == 1 ) {
				// As single parent is a "real" commit and can be committed
				// to the database
				WriteCommit( target, name, prNumber, mergeVia );

				Commit current = target.Parents.First();
				while( current != null && !visited.Contains( current.Sha ) && !current.Sha.Equals( PreHistoryId, StringComparison.Ordinal ) ) {
					parentCount = current.Parents.Count();
					if( parentCount == 1 ) {
						visited.Add( current.Sha );
						_statistics.ProcessedCommit();
						WriteCommit( current, name, prNumber, newMergeVia );
					} else {
						WalkMerge( gitRepo, newMergeVia, name, prNumber, visited, toVisit, current );
					}
					current = current.Parents.FirstOrDefault();
					_statisticsDisplay.UpdateStatistics( _statistics );
				}

			} else {

				// We want to note the _actual_ PR that landed the commit,
				// not all of the transitive PRs that made the commit land
				// in master.
				string newPrNumber = GetPrNumber( target );

				// Again, walk each parent.
				foreach( Commit parent in target.Parents ) {
					if( !visited.Contains( parent.Sha ) && !parent.Sha.Equals( PreHistoryId, StringComparison.Ordinal ) ) {
						_statistics.ProcessedCommit();
						if( !IsMergeInto( parent ) ) {
							WalkMerge( gitRepo, newMergeVia, name, newPrNumber, visited, toVisit, parent );
						} else {
							visited.Add( parent.Sha );
						}
					}
				}
			}
		}

		private void WriteCommit(
			Commit commit,
			RepoProjectName name,
			string prNumber,
			IEnumerable<string> mergeVia
		) {
			if( !_commitRepository.ContainsCommit( commit.Sha, name.Project, name.Repo ) ) {
				_statistics.WroteCommit();
				CommitDetails commitInfo = CommitInfoFromCommit( commit, name.Project, name.Repo, prNumber, mergeVia );
				_commitRepository.Add( commitInfo );
			}
		}

		private CommitDetails CommitInfoFromCommit(
			Commit commit,
			string projectName,
			string repoName,
			string prNumber,
			IEnumerable<string> mergeVia
		) {
			IEnumerable<string> files = _gitRepoProvider.GetCommitFiles( commit );

			string[] description = commit.Message.Split( '\n' ).Select( l => l.Trim() ).ToArray();

			var result = new CommitDetails(
				authorEmail: commit.Author.Email,
				authorName: commit.Author.Name,
				commitId: commit.Sha,
				date: commit.Committer.When.ToUniversalTime().ToString( "yyyyMMddTHHmmssfffffffZ", CultureInfo.InvariantCulture ),
				description: description,
				files: files,
				pr: prNumber,
				project: projectName,
				repo: repoName,
				commits: mergeVia,
				isMerge: false
			);

			return result;
		}

		private string GetPrNumber( Commit commit ) {
			string prNumber = string.Empty;
			if( commit.Message.StartsWith( MergePrNumberToken ) ) {
				int prStart = commit.Message.IndexOf( MergePrNumberToken ) + MergePrNumberToken.Length;
				int prEnd = commit.Message.IndexOf( " in " );
				prNumber = commit.Message.Substring( prStart, prEnd - prStart ).Trim();
			}

			return prNumber;
		}

		private bool IsMergeInto( Commit commit ) {
			return ( commit.Message.Contains( "Merge" ) && commit.Message.Contains( "into" ) );
		}
	}
}
