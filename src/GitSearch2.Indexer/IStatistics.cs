namespace GitSearch2.Indexer {
	public interface IStatistics {
		int Processed { get; }

		int Written { get; }

		int ToVisit { get; }

		int Visited { get; }

		void ProcessedCommit();

		void WroteCommit();
	}
}
