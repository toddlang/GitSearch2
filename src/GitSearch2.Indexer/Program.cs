using System;
using CommandLine;
using GitSearch2.Repository;
using GitSearch2.Repository.Sqlite;
using GitSearch2.Repository.SqlServer;
using Microsoft.Extensions.DependencyInjection;

namespace GitSearch2.Indexer {
	public sealed class Program {
		public static void Main( string[] args ) {
			Parser.Default.ParseArguments<Options>( args )
			  .WithParsed( opts => {

				  IServiceCollection services = new ServiceCollection()
					.AddSingleton( opts )
					.AddSingleton<ICommitWalker, CommitWalker>()
					.AddSingleton<IStatisticsDisplay, StatisticsDisplay>()
					.AddSingleton<INameParser, RemoteNameParser>()
					.AddSingleton<IGitRepoProvider, CyclingGitRepoProvider>()
					.AddSingleton<IExecutor, LoopingExecutor>();

				  switch (opts.Database) {
					  case Database.Sqlite: {
							  SqliteOptions options = new SqliteOptions() {
								  ConnectionString = opts.Connection
							  };

							  services.AddSqlite( options );
						  }
						  break;
					  case Database.SqlServer: {
							  SqlServerOptions options = new SqlServerOptions() {
								  ConnectionString = opts.Connection
							  };

							  services.AddSqlServer( options );
						  }
						  break;
					  default:
						  Console.WriteLine( "Unknown database backend specified." );
						  return;
				  }

				  IServiceProvider provider = services.BuildServiceProvider();
				  provider.InitializeRepositories();

				  DateTimeOffset start = DateTimeOffset.Now;

				  IExecutor executor = provider.GetService<IExecutor>();
				  executor.Run();

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
