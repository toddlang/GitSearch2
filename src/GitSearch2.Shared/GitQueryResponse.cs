using System.Collections.Generic;

namespace GitSearch2.Shared {
	public sealed class GitQueryResponse {

		public GitQueryResponse(
			IEnumerable<CommitDetails> commits,
			string message
		) {
			Commits = commits;
			Message = message;
		}

		public IEnumerable<CommitDetails> Commits { get; }

		public string Message { get; }
	}
}
