using System;
using Pomelo.AspNetCore.WebPages.Data;
using Pomelo.AspNetCore.WebPages.Data.Oracle;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOracle(this IServiceCollection self, Action<DatabaseOptions> buildOptions)
        {
            var options = new DatabaseOptions();
            buildOptions(options);
            return self.AddSingleton<IDatabaseFactory, OracleDatabaseFactory>()
                .AddSingleton(options);
        }
    }
}
