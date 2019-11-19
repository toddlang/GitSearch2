using System;
using CommandLine;
using GitSearch2.Repository;
using GitSearch2.Repository.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace GitSearch2.Indexer {
	public sealed class Program {
		public static void Main( string[] args ) {
			Parser.Default.ParseArguments<Options>( args )
			  .WithParsed( opts => {

				  SqliteOptions options = new SqliteOptions() {
					  ConnectionString = $"Data Source={opts.Database}"
				  };

				  IServiceProvider services = new ServiceCollection()
					.AddSingleton( opts )
					.AddSingleton( options )
					.AddSingleton<ICommitWalker, CommitWalker>()
					.AddSingleton<IStatisticsDisplay, StatisticsDisplay>()
					.AddSingleton<INameParser, RemoteNameParser>()
					.AddSingleton<ICommitRepository, CommitSqliteRepository>()
					.AddSingleton<IUpdateRepository, UpdateSqliteRepository>()
					.AddSingleton<IGitRepoProvider, CyclingGitRepoProvider>()
					.BuildServiceProvider();

				  services.InitializeRepositories();


				  DateTimeOffset start = DateTimeOffset.Now;

				  ICommitWalker walker = services.GetService<ICommitWalker>();
				  walker.Run();

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
