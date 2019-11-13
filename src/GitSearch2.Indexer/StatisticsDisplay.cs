using System;

namespace GitSearch2.Indexer {

	internal sealed class StatisticsDisplay : IStatisticsDisplay {

		private readonly int _cursorTop;
		private readonly bool _liveDisplay;
		private int _updates;

		public StatisticsDisplay( bool liveDisplay ) {
			_cursorTop = Console.CursorTop;
			_liveDisplay = liveDisplay;
		}

		void IStatisticsDisplay.UpdateStatistics( IStatistics statistics ) {
			_updates += 1;
			_updates %= 10000;
			if (_liveDisplay) {
				Console.CursorTop = _cursorTop;
				Console.WriteLine( $"Unique: {statistics.Visited}          " );
				Console.WriteLine( $"Visited: {statistics.Processed}          " );
				Console.WriteLine( $"Queued: {statistics.ToVisit}          " );
				Console.WriteLine( $"Written: {statistics.Written}          " );
			} else if (_updates == 0) {
				Console.WriteLine( $"Unique / Visited / Queued / Written : {statistics.Visited} {statistics.Processed} {statistics.ToVisit} {statistics.Written }" );
			}
		}
	}

}
