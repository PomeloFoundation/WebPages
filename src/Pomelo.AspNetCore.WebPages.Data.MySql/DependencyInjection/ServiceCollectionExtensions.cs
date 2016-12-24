using System;
using Pomelo.AspNetCore.WebPages.Data;
using Pomelo.AspNetCore.WebPages.Data.MySql;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMySql(this IServiceCollection self, Action<DatabaseOptions> buildOptions)
        {
            var options = new DatabaseOptions();
            buildOptions(options);
            return self.AddSingleton<IDatabaseFactory, MySqlDatabaseFactory>()
                .AddSingleton(options);
        }
    }
}
