using System;

namespace GitSearch2.Repository
{
	public class RepoProgress {

		public RepoProgress(
			string repo,
			string project,
			int progress,
			DateTimeOffset started,
			DateTimeOffset? finished,
			string lastCommitId
		) {
			Repo = repo;
			Project = project;
			Progress = progress;
			Started = started;
			Finished = finished;
			LastCommitId = lastCommitId;
		}

		public string Repo { get; }

		public string Project { get; }

		public int Progress { get; }

		public DateTimeOffset Started { get; }

		public DateTimeOffset? Finished { get; }

		public string LastCommitId { get; }
	}
}
