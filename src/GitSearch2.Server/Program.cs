using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
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
				BuildWebHost( args ).Run();
			} catch( Exception ex ) {
				Log.Fatal( ex, "Application startup failed." );
			} finally {
				Log.CloseAndFlush();
			}
		}

		public static IWebHost BuildWebHost( string[] args ) =>
			WebHost.CreateDefaultBuilder( args )
				.UseSerilog()
				.UseConfiguration( new ConfigurationBuilder()
					.AddCommandLine( args )
					.Build() )
				.UseStartup<Startup>()
				.Build();
	}
}
