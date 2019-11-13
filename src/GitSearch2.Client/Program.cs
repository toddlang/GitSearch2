using System;
using Microsoft.AspNetCore.Blazor.Hosting;

namespace GitSearch2.Client {

#pragma warning disable CA1052
	public class Program {
		public static void Main( string[] args ) {
			CreateHostBuilder( args ).Build().Run();
		}

		public static IWebAssemblyHostBuilder CreateHostBuilder( string[] args ) =>
			BlazorWebAssemblyHost.CreateDefaultBuilder()
				.UseBlazorStartup<Startup>();
	}
}
#pragma warning restore
