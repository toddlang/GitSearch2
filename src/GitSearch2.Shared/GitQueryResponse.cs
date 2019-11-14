using System;
using System.Collections.Generic;
using System.Text;
using Utf8Json;

namespace GitSearch2.Shared {
	public sealed class GitQueryResponse {

		[SerializationConstructor]
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
