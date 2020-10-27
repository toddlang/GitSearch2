using System;

namespace GitSearch2.Shared {
	public sealed class GitQuery : IEquatable<GitQuery> {

		public GitQuery(
			string searchTerm,
			int startRecord,
			int maximumRecords
		) {
			if( string.IsNullOrWhiteSpace( searchTerm ) ) {
				throw new ArgumentException( "Search term not specified.", nameof( searchTerm ) );
			}

			if( startRecord < 0 ) {
				throw new ArgumentException( "Invalid start record.  Must be greater than or equal to zero.", nameof( startRecord ) );
			}

			if( maximumRecords <= 0 ) {
				throw new ArgumentException( "Maximum records not specified.  Must be greater than zero.", nameof( maximumRecords ) );
			}

			SearchTerm = searchTerm;
			StartRecord = startRecord;
			MaximumRecords = maximumRecords;
		}

		public string SearchTerm { get; }

		public int StartRecord { get; }

		public int MaximumRecords { get; }

		public bool Equals( GitQuery other ) {
			if( other is null ) {
				return false;
			}

			if( ReferenceEquals( other, this ) ) {
				return true;
			}

			return
				string.Equals( SearchTerm, other.SearchTerm, StringComparison.Ordinal )
				&& StartRecord == other.StartRecord
				&& MaximumRecords == other.MaximumRecords;
		}

		public override bool Equals( object obj ) {
			return Equals( obj as GitQuery );
		}

		public override int GetHashCode() {
			return HashCode.Combine( SearchTerm, StartRecord, MaximumRecords );
		}
	}
}
