using System;
using System.Net.Http;
using System.Threading.Tasks;
using GitSearch2.Client.Service;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace GitSearch2.Client {

	public class Program {

		public static async Task Main( string[] args ) {
			var builder = WebAssemblyHostBuilder.CreateDefault( args );
			builder.Services.AddSingleton<IJsonConverter, JsonConverter>();
			builder.Services.AddSingleton<IGitQueryService, GitQueryService>();

			builder.RootComponents.Add<App>( "app" );
			ConfigureServices( builder.Services );
			builder.Services.AddSingleton(
				new HttpClient {
					BaseAddress = new Uri( builder.HostEnvironment.BaseAddress )
				}
			);
			WebAssemblyHost host = builder.Build();

			// Access registered services here

			await host.RunAsync();
		}

		private static void ConfigureServices( IServiceCollection services ) {
			services.AddSingleton<IUrlGenerator, BitBucketUrlGenerator>();
		}
	}
}
