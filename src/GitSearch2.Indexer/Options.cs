using CommandLine;

namespace GitSearch2.Indexer {
	internal sealed class Options {
		[Option( 'i', "input", Required = true, HelpText = "Folder containing git repository to index." )]
		public string GitFolder { get; set; }

		[Option( 'p', "pause", Required = false, Default = false, HelpText = "Pause for a key press when execution is complete." )]
		public bool PauseWhenComplete { get; set; }

		[Option( 'l', "live", Required = false, Default = false, HelpText = "Live statistics display." )]
		public bool LiveStatisticsDisplay { get; set; }

		[Option( 'c', "connection", Required = true, Default = "Data Source=webapp.sqlite3", HelpText = "Connection string to database." )]
		public string Connection { get; set; }

		[Option( 'd', "database", Required = true, Default = Database.Sqlite, HelpText = "Database backend. (Sqlite, SqlServer)" )]
		public Database Database { get; set; }
	}
}
