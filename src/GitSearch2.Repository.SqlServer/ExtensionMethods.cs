using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GitSearch2.Repository.SqlServer {
	public static class ExtensionMethods {
		public static void AddSqlServer( this IServiceCollection services, IConfigurationSection config ) {
			services.Configure<SqlServerOptions>( config );
			services.AddSingleton<ICommitRepository, CommitSqlServerRepository>();
			services.AddSingleton<IUpdateRepository, UpdateSqlServerRepository>();
		}
	}
}
