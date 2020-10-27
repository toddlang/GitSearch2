using System;
using System.Collections.Generic;

namespace GitSearch2.Shared {
	public sealed class CommitDetails : IEquatable<CommitDetails> {

		public CommitDetails(
			IEnumerable<string> description,
			string repo,
			string authorEmail,
			string authorName,
			string date,
			IEnumerable<string> files,
			string commitId,
			string project,
			string pr,
			IEnumerable<string> commits,
			bool isMerge
		) {
			if( string.IsNullOrWhiteSpace( repo ) ) {
				throw new ArgumentException( "Repo name not specified", nameof( repo ) );
			}

			if( string.IsNullOrWhiteSpace( authorName ) ) {
				throw new ArgumentException( "Author name not specified", nameof( authorName ) );
			}

			if( string.IsNullOrWhiteSpace( date ) ) {
				throw new ArgumentException( "Date not specified", nameof( date ) );
			}

			if( string.IsNullOrWhiteSpace( commitId ) ) {
				throw new ArgumentException( "CommitId not specified.", nameof( commitId ) );
			}

			Description = description ?? throw new ArgumentException( "Description not specified.", nameof( description ) );
			Repo = repo;
			AuthorEmail = authorEmail;
			AuthorName = authorName;
			Date = date;
			Files = files ?? throw new ArgumentException( "Files not specified.", nameof( files ) );
			CommitId = commitId;
			Project = project;
			PR = pr;
			Commits = commits ?? throw new ArgumentException( "Commits not specified.", nameof( commits ) );
			IsMerge = isMerge;
		}

		public IEnumerable<string> Description { get; }

		public string Repo { get; }

		public string AuthorEmail { get; }

		public string AuthorName { get; }

		public string Date { get; }

		public IEnumerable<string> Files { get; }

		public string CommitId { get; }

		public string Project { get; }

		public string PR { get; }

		public IEnumerable<string> Commits { get; }

		public bool IsMerge { get; }

		public bool Equals( CommitDetails other ) {
			if( other is null ) {
				return false;
			}

			if( ReferenceEquals( other, this ) ) {
				return true;
			}

			// For the needs of this code we only care about the commit Id.

			return CommitId.Equals( other.CommitId, StringComparison.Ordinal );
		}

		public override bool Equals( object obj ) {
			return Equals( obj as CommitDetails );
		}

		public override int GetHashCode() {
			return CommitId.GetHashCode();
		}
	}
}
