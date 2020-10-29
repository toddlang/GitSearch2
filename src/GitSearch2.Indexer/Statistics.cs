using System.Collections.Generic;

namespace GitSearch2.Indexer {
	internal sealed class Statistics : IStatistics {

		private readonly HashSet<string> _visited;
		private readonly Stack<string> _toVisit;
		private int _processed;
		private int _written;

		public Statistics(
			HashSet<string> visited,
			Stack<string> toVisit
		) {
			_visited = visited;
			_toVisit = toVisit;
		}


		int IStatistics.Processed {
			get {
				return _processed;
			}
		}

		int IStatistics.Written {
			get {
				return _written;
			}
		}

		int IStatistics.Visited {
			get {
				return _visited.Count;
			}
		}

		int IStatistics.ToVisit {
			get {
				return _toVisit.Count;
			}
		}

		void IStatistics.ProcessedCommit() {
			_processed += 1;
		}

		void IStatistics.WroteCommit() {
			_written += 1;
		}
	}
}
