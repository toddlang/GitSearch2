using System;

namespace GitSearch2.Repository
{
	public sealed class UpdateSession: IEquatable<UpdateSession> {

		public static UpdateSession None = new UpdateSession( Guid.Empty, null, null, null, null, int.MinValue );

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

		public override bool Equals( object obj ) {
			return Equals( obj as UpdateSession );
		}

		public bool Equals( UpdateSession other ) {
			if (other is null ) {
				return false;
			}

			if (ReferenceEquals( other, this )) {
				return true;
			}

			return Session.Equals( other.Session )
				&& string.Equals( Repo, other.Repo, StringComparison.Ordinal )
				&& string.Equals( Project, other.Project, StringComparison.Ordinal )
				&& Nullable.Equals( Started, other.Started )
				&& Nullable.Equals( Finished, other.Finished )
				&& Nullable.Equals( CommitsWritten, other.CommitsWritten );
		}

		public override int GetHashCode() {
			unchecked {
				int result = 17;
				result = ( result * 31 ) + Session.GetHashCode();
				result = ( result * 31 ) + Repo?.GetHashCode() ?? 0;
				result = ( result * 31 ) + Project?.GetHashCode() ?? 0;
				result = ( result * 31 ) + Started?.GetHashCode() ?? 0;
				result = ( result * 31 ) + Finished?.GetHashCode() ?? 0;
				result = ( result * 31 ) + CommitsWritten?.GetHashCode() ?? 0;

				return result;
			}
		}
	}
}
