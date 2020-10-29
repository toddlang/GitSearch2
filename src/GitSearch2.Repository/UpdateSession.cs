using System;

namespace GitSearch2.Repository {
	public sealed record UpdateSession {

		public static readonly UpdateSession None = new UpdateSession( Guid.Empty, null, null, null, null, int.MinValue );

		public UpdateSession(
			Guid session,
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

		public Guid Session { get; }

		public string Repo { get; }

		public string Project { get; }

		public DateTime? Started { get; }

		public DateTime? Finished { get; }

		public int? CommitsWritten { get; }
	}
}
