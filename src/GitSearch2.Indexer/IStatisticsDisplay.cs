using System;

namespace GitSearch2.Indexer {
	public interface IStatisticsDisplay {

		void UpdateStatistics( IStatistics statistics );
	}
}
