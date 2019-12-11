using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GitSearch2.Repository.Sqlite {
	public static class ExtensionMethods {
		public static void AddSqlite( this IServiceCollection services, IConfigurationSection config ) {
			services.Configure<SqliteOptions>( config );
			services.AddSingleton<ICommitRepository, CommitSqliteRepository>();
			services.AddSingleton<IUpdateRepository, UpdateSqliteRepository>();
		}
	}
}
