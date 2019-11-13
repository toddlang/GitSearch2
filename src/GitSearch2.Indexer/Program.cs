using System;
using CommandLine;
using GitSearch2.Repository;
using GitSearch2.Repository.Sqlite;

namespace GitSearch2.Indexer {
	public sealed class Program {
		public static void Main( string[] args ) {
			Parser.Default.ParseArguments<Options>( args )
			  .WithParsed( opts => {
				  SqliteOptions options = new SqliteOptions() {
					  ConnectionString = "Data Source=webapp.sqlite3"
				  };

				  DateTimeOffset start = DateTimeOffset.Now;

				  ICommitRepository commitRepo = new CommitSqliteRepository( options );
				  commitRepo.Initialize();
				  IUpdateRepository updateRepo = new UpdateSqliteRepository( options );
				  updateRepo.Initialize();

				  ICommitWalker visitor = new CommitWalker(
					  commitRepo,
					  updateRepo,
					  opts.GitFolder,
					  opts.LiveStatisticsDisplay );
				  
				  visitor.Run();

				  DateTimeOffset stop = DateTimeOffset.Now;

				  Console.WriteLine( $"Completed in {stop.Subtract( start ).TotalSeconds:F0} seconds." );

				  if (opts.PauseWhenComplete) {
					  Console.WriteLine( "Press any key to finish..." );
					  Console.ReadKey();
				  }
			  } );
		}
	}
}
