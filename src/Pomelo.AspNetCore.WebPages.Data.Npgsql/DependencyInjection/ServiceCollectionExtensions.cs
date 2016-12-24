using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pomelo.AspNetCore.WebPages.Data;
using Pomelo.AspNetCore.WebPages.Data.Npgsql;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNpgsql(this IServiceCollection self, Action<DatabaseOptions> buildOptions)
        {
            var options = new DatabaseOptions();
            buildOptions(options);
            return self.AddSingleton<IDatabaseFactory, NpgsqlDatabaseFactory>()
                .AddSingleton(options);
        }
    }
}
