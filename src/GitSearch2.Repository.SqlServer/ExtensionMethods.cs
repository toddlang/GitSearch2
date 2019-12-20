using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GitSearch2.Repository.SqlServer {
	public static class ExtensionMethods {
		public static void AddSqlServer( this IServiceCollection services, IConfigurationSection config ) {
			services.Configure<SqlServerOptions>( config );
			services.AddSingleton<IDb, SqlServerDb>();
			services.AddSingleton<ICommitRepository, CommitSqlServerRepository>();
			services.AddSingleton<IUpdateRepository, UpdateSqlServerRepository>();
		}

		public static void AddSqlServer( this IServiceCollection services, SqlServerOptions options ) {
			services.AddSingleton( options );
			services.AddSingleton<IDb, SqlServerDb>();
			services.AddSingleton<ICommitRepository, CommitSqlServerRepository>();
			services.AddSingleton<IUpdateRepository, UpdateSqlServerRepository>();
		}
	}
}
