using System;

namespace GitSearch2.Repository
{
	public sealed class UpdateSession {

		public UpdateSession(
			string session,
			string repo,
			string project,
			DateTime? started,
			DateTime? finished,
			int commitsWritten
		) {
			Session = session;
			Repo = repo;
			Project = project;
			Started = started;
			Finished = finished;
			CommitsWritten = commitsWritten;
		}

		public string Session { get; }

		public string Repo { get; }

		public string Project { get; }

		public DateTime? Started { get; }

		public DateTime? Finished { get; }

		public int? CommitsWritten { get; }
	}
}
