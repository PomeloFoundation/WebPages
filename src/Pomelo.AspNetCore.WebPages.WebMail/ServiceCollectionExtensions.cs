using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWebMail(this IServiceCollection self)
        {
            return self.AddSingleton<WebMail>()
                .AddSingleton<IContentTypeProvider, FileExtensionContentTypeProvider>();
        }
    }
}
