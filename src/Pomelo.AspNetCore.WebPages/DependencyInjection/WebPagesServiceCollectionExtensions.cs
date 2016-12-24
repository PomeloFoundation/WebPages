using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pomelo.AspNetCore.WebPages;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RazorPagesServiceCollectionExtensions
    {
        public static IMvcCoreBuilder AddWebPages(this IServiceCollection self, Action<MvcOptions> mvcOptions = null, Action<WebPagesOptions> webPagesOptions = null)
        {
            IMvcCoreBuilder mvcCoreBuilder;
            if (mvcOptions != null)
                mvcCoreBuilder = self.AddMvcCore(mvcOptions);
            else
                mvcCoreBuilder = self.AddMvcCore();
            mvcCoreBuilder.AddAuthorization()
                .AddViews()
                .AddRazorViewEngine();
            if (webPagesOptions != null)
                mvcCoreBuilder.AddWebPages(webPagesOptions);
            else
                mvcCoreBuilder.AddWebPages();
            return mvcCoreBuilder;
        }
    }
}
