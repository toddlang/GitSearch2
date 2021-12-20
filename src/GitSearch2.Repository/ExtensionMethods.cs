using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace GitSearch2.Repository {
	public static class ExtensionMethods {
		public static IApplicationBuilder UseRepositories( this IApplicationBuilder builder ) {
			builder.ApplicationServices.InitializeRepositories();
			return builder;
		}

		public static IServiceProvider InitializeRepositories( this IServiceProvider services ) {
			ICommitRepository commitRepository = services.GetService<ICommitRepository>();
			commitRepository.Initialize();

			IUpdateRepository updateRepository = services.GetService<IUpdateRepository>();
			updateRepository.Initialize();

			return services;
		}
	}
}
