using CommandLine;
using GitSearch2.Repository;
using GitSearch2.Repository.Sqlite;
using GitSearch2.Repository.SqlServer;
using GitSearch2.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace GitSearch2.Indexer {
	public sealed class Program {
		public static int Main( string[] args ) {
			return Parser.Default.ParseArguments<Options>( args )
				.MapResult(
					opts => {

						IServiceCollection services = new ServiceCollection()
						  .AddSingleton( opts )
						  .AddSingleton<ICommitWalker, CommitWalker>()
						  .AddSingleton<IStatisticsDisplay, StatisticsDisplay>()
						  .AddSingleton<INameParser, RemoteNameParser>()
						  .AddSingleton<IGitRepoProvider, CyclingGitRepoProvider>()
						  .AddSingleton<IExecutor, LoopingExecutor>()
						  .AddSharedServices();

						switch( opts.Database ) {
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
								return -1;
						}

						IServiceProvider provider = services.BuildServiceProvider();

						try {
							provider.InitializeRepositories();

							DateTimeOffset start = DateTimeOffset.Now;

							IExecutor executor = provider.GetService<IExecutor>();
							executor.Run();

							DateTimeOffset stop = DateTimeOffset.Now;

							Console.WriteLine( $"Completed in {stop.Subtract( start ).TotalSeconds:F0} seconds." );

							if( opts.PauseWhenComplete ) {
								Console.WriteLine( "Press any key to finish..." );
								Console.ReadKey();
							}

							return 0;

						} catch( Exception e ) {
							Console.WriteLine( e.Message );

							if( opts.PauseWhenComplete ) {
								Console.WriteLine( "Press any key to finish..." );
								Console.ReadKey();
							}

							return -2;
						}
					},
				_ => 1 ); // This will automatically print an error message based on the meta data if the arguments are incorrect
		}
	}
}
