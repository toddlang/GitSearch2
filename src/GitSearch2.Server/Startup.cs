using GitSearch2.Repository;
using GitSearch2.Repository.Sqlite;
using GitSearch2.Repository.SqlServer;
using Serilog;

namespace GitSearch2.Server {
	public class Startup {

		public Startup( IConfiguration configuration ) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices( IServiceCollection services ) {

			services.AddControllers();
			services.AddRazorPages();

			AddRepositories( services );
		}

		public void Configure(
			IApplicationBuilder app,
			IWebHostEnvironment env
		) {
			if( env.IsDevelopment() ) {
				app.UseDeveloperExceptionPage();
				app.UseWebAssemblyDebugging();
			} else {
				app.UseExceptionHandler( "/Error" );
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseBlazorFrameworkFiles();
			app.UseStaticFiles();

			app.UseSerilogRequestLogging();

			app.UseRouting();

			app.UseEndpoints( endpoints => {
				endpoints.MapRazorPages();
				endpoints.MapDefaultControllerRoute();
				endpoints.MapFallbackToFile( "index.html" );
			} );

			app.UseRepositories();
		}

		private void AddRepositories( IServiceCollection services ) {
			IConfigurationSection databaseOptions = Configuration.GetSection( "Database" );
			string source = databaseOptions["Source"];
			IConfigurationSection sourceOptions = databaseOptions.GetSection( source );

			switch( source ) {
				case "Sqlite":
					services.AddSqlite( sourceOptions );
					break;
				case "SqlServer":
					services.AddSqlServer( sourceOptions );
					break;
				default:
					throw new InvalidOperationException();
			}
		}
	}
}
