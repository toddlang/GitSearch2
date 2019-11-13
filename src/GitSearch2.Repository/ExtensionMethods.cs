using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace GitSearch2.Repository {
	public static class ExtensionMethods {
		public static IApplicationBuilder UseRepositories( this IApplicationBuilder builder ) {
			ICommitRepository commitRepository = builder.ApplicationServices.GetService<ICommitRepository>();
			commitRepository.Initialize();

			IUpdateRepository updateRepository = builder.ApplicationServices.GetService<IUpdateRepository>();
			updateRepository.Initialize();

			return builder;
		}
	}
}
