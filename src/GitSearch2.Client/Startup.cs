using System;
using GitSearch2.Client.Service;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace GitSearch2.Client {
#pragma warning disable CA1822
	public sealed class Startup {
		public void ConfigureServices( IServiceCollection services ) {
			services.AddSingleton<IJsonConverter, JsonConverter>();
			services.AddSingleton<IGitQueryService, GitQueryService>();
		}

		public void Configure( IComponentsApplicationBuilder app ) {
			app.UseLocalTimeZone();
			app.AddComponent<App>( "app" );
		}
	}
#pragma warning restore CA1822
}
