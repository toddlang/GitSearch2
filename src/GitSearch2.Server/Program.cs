using System.Security.Authentication;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using Serilog.Events;

namespace GitSearch2.Server {
	public class Program {
		public static void Main( string[] args ) {
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Override( "Microsoft.AspNetCore", LogEventLevel.Warning )
				.Enrich.FromLogContext()
				.WriteTo.RollingFile( @"logs\gitsearch2-{Date}.txt", retainedFileCountLimit: 2, buffered: true, flushToDiskInterval: TimeSpan.FromSeconds( 60 ) )
				.CreateLogger();
			try {
				Log.Information( "Application starting." );
				CreateHostBuilder( args ).Build().Run();
			} catch( Exception ex ) {
				Log.Fatal( ex, "Application startup failed." );
			} finally {
				Log.CloseAndFlush();
			}
		}

		public static IHostBuilder CreateHostBuilder( string[] args ) {
			return Host.CreateDefaultBuilder( args )
				.ConfigureWebHostDefaults( webBuilder => {

					webBuilder.ConfigureKestrel( serverOptions => {
						serverOptions.ConfigureEndpointDefaults( defaults => {
							defaults.Protocols = HttpProtocols.Http1AndHttp2;
						} );

						serverOptions.ConfigureHttpsDefaults( listenOptions => {
							listenOptions.SslProtocols = SslProtocols.Tls12|SslProtocols.Tls13;
						} );
					} );

					webBuilder.UseSerilog();
					webBuilder.UseConfiguration( new ConfigurationBuilder()
						.AddCommandLine( args )
						.Build() );
					webBuilder.UseStartup<Startup>();
				} );
		}
	}
}
