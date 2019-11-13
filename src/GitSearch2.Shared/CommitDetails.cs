using System;
using System.Collections.Generic;
using Utf8Json;

namespace GitSearch2.Shared {
	public sealed class CommitDetails: IEquatable<CommitDetails> {

		[SerializationConstructor]
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
				throw new ArgumentException( nameof( repo ) );
			}

			if (string.IsNullOrWhiteSpace( authorName )) {
				throw new ArgumentException( nameof( authorName ) );
			}

			if (string.IsNullOrWhiteSpace( date )) {
				throw new ArgumentException( nameof( date ) );
			}

			if( string.IsNullOrWhiteSpace( commitId )) {
				throw new ArgumentException( nameof( commitId ) );
			}

			Description = description ?? throw new ArgumentException( nameof( description ) );
			Repo = repo;
			AuthorEmail = authorEmail;
			AuthorName = authorName;
			Date = date;
			Files = files ?? throw new ArgumentException( nameof( files ) );
			CommitId = commitId;
			Project = project;
			PR = pr;
			Commits = commits ?? throw new ArgumentException( nameof( commits ) );
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
			if (other is null) {
				return false;
			}

			if (ReferenceEquals(other, this)) {
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
