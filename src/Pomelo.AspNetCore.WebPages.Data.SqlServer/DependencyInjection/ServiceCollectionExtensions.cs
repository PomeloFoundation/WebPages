using System;
using Pomelo.AspNetCore.WebPages.Data;
using Pomelo.AspNetCore.WebPages.Data.SqlServer;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqlServer(this IServiceCollection self, Action<DatabaseOptions> buildOptions)
        {
            var options = new DatabaseOptions();
            buildOptions(options);
            return self.AddSingleton<IDatabaseFactory, SqlDatabaseFactory>()
                .AddSingleton(options);
        }
    }
}
