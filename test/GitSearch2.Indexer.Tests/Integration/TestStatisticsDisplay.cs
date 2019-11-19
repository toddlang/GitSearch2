using System;
using System.Collections.Generic;
using System.Text;

namespace GitSearch2.Indexer.Tests.Integration {
	internal sealed class TestStatisticsDisplay : IStatisticsDisplay {
		void IStatisticsDisplay.UpdateStatistics( IStatistics statistics ) {
			// Do nothing
		}
	}
}
