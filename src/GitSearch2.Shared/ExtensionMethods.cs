using Microsoft.Extensions.DependencyInjection;

namespace GitSearch2.Shared {
	public static class ExtensionMethods {

		public static IServiceCollection AddSharedServices( this IServiceCollection services ) {
			services.AddSingleton<IRepoWebsiteIdentifier, D2LRepoWebsiteIdentifier>();

			return services;
		}
	}
}
