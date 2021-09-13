using System.Diagnostics.CodeAnalysis;

namespace GitSearch2.Indexer {
	internal sealed class RepoProjectName : IEquatable<RepoProjectName> {

		public RepoProjectName(
			string repo,
			string project
		) {
			Repo = repo;
			Project = project;
		}

		public string Repo { get; }

		public string Project { get; }

		public bool Equals( [AllowNull] RepoProjectName other ) {
			if( other is null ) {
				return false;
			}

			if( ReferenceEquals( other, this ) ) {
				return true;
			}

			return string.Equals( Repo, other.Repo, StringComparison.OrdinalIgnoreCase )
				&& string.Equals( Project, other.Project, StringComparison.OrdinalIgnoreCase );
		}

		public override bool Equals( object obj ) {
			return Equals( obj as RepoProjectName );
		}

		public override int GetHashCode() {
			return HashCode.Combine( Repo, Project );
		}
	}
}
