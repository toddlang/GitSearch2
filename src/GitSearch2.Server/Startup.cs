using System;
using System.Linq;
using GitSearch2.Repository;
using GitSearch2.Repository.Sqlite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Utf8Json.AspNetCoreMvcFormatter;

namespace GitSearch2.Server {
#pragma warning disable CA1822
	public class Startup {

		public Startup(IConfiguration configuration) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices( IServiceCollection services ) {
			// TODO: Remove this when Utf8Json.JsonInputFormatter uses
			// DeserializeAsync inside the ReadAsync call.
			// Problem here: https://github.com/neuecc/Utf8Json/blob/master/src/Utf8Json.AspNetCoreMvcFormatter/Formatter.cs#L80
			// Issue raised here: https://github.com/neuecc/Utf8Json/issues/97
			// Alternatively, we could use this branch: https://github.com/DSilence/Utf8Json/tree/feature/formatters
			services.Configure<KestrelServerOptions>( options =>
			{
				options.AllowSynchronousIO = true;
			} );
			services.Configure<IISServerOptions>( options => {
				options.AllowSynchronousIO = true;
			} );

			IConfigurationSection databaseOptions = Configuration.GetSection( "Database" );
			IConfigurationSection connectionOptions = databaseOptions.GetSection( "Connection" );
			services.Configure<SqliteOptions>( connectionOptions.GetSection( "Sqlite" ) );
			services.AddSingleton<ICommitRepository, CommitSqliteRepository>();
			services.AddSingleton<IUpdateRepository, UpdateSqliteRepository>();

			services.AddMvc( options => {
				options.InputFormatters.RemoveType<SystemTextJsonInputFormatter>();
				options.InputFormatters.Add( new JsonInputFormatter() );
				options.OutputFormatters.RemoveType<SystemTextJsonOutputFormatter>();
				options.OutputFormatters.Add( new JsonOutputFormatter() );
			} );

			services.AddResponseCompression( opts => {
				opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
					new[] { "application/octet-stream" } );
			} );
		}

		public void Configure(
			IApplicationBuilder app,
			IWebHostEnvironment env
		) {
			app.UseResponseCompression();

			if( env.IsDevelopment() ) {
				app.UseDeveloperExceptionPage();
				app.UseBlazorDebugging();
			}

			app.UseClientSideBlazorFiles<Client.Startup>();

			app.UseSerilogRequestLogging();

			app.UseRouting();

			app.UseEndpoints( endpoints => {
				endpoints.MapDefaultControllerRoute();
				endpoints.MapFallbackToClientSideBlazor<Client.Startup>( "index.html" );
			} );

			app.UseRepositories();
		}
	}
#pragma warning restore CA1822
}
