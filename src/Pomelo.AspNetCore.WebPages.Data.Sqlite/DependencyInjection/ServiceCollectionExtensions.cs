using System;
using Pomelo.AspNetCore.WebPages.Data;
using Pomelo.AspNetCore.WebPages.Data.Sqlite;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqlite(this IServiceCollection self, Action<DatabaseOptions> buildOptions)
        {
            var options = new DatabaseOptions();
            buildOptions(options);
            return self.AddSingleton<IDatabaseFactory, SqliteDatabaseFactory>()
                .AddSingleton(options);
        }
    }
}
