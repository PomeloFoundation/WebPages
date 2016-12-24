// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Hosting;
using Pomelo.AspNetCore.WebPages;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public class DefaultWebPagesOptionsSetup : IConfigureOptions<WebPagesOptions>
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public DefaultWebPagesOptionsSetup(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public void Configure(WebPagesOptions options)
        {
            options.DefaultNamespace = _hostingEnvironment.ApplicationName;

            if (_hostingEnvironment.ContentRootFileProvider != null)
            {
                options.FileProviders.Add(_hostingEnvironment.ContentRootFileProvider);
            }

        }
    }
}
