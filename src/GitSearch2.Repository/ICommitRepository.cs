using System.Collections.Generic;
using System.Threading.Tasks;
using GitSearch2.Shared;

namespace GitSearch2.Repository {
	public interface ICommitRepository {
		void Initialize();

		IEnumerable<CommitDetails> Search( string term, int limit );

		Task<IEnumerable<CommitDetails>> SearchAsync( string term, int limit );

		bool ContainsCommit( string commitId, string project, string repo );

		Task<bool> ContainsCommitAsync( string commitId, string project, string repo );

		void Add( CommitDetails commit );

		Task AddAsync( CommitDetails commit );

		int CountCommits();

		Task<int> CountCommitsAsync();
	}
}
