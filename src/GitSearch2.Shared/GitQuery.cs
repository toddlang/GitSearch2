using System;

namespace GitSearch2.Shared {
	public sealed record GitQuery {

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
	}
}
