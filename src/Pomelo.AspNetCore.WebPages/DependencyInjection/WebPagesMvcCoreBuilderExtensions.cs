// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Pomelo.AspNetCore.WebPages;
using Pomelo.AspNetCore.WebPages.Compilation;
using Pomelo.AspNetCore.WebPages.Infrastructure;
using Pomelo.AspNetCore.WebPages.Internal;
using Pomelo.AspNetCore.WebPages.ModelBinding;
using Pomelo.AspNetCore.WebPages.Razevolution;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WebPagesMvcCoreBuilderExtensions
    {
        public static IMvcCoreBuilder AddWebPages(this IMvcCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            RegisterFeatures(builder.PartManager);
            RegisterServices(builder.Services);

            return builder;
        }

        public static IMvcCoreBuilder AddWebPages(this IMvcCoreBuilder builder, Action<WebPagesOptions> setupAction)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            RegisterFeatures(builder.PartManager);
            RegisterServices(builder.Services);
            builder.Services.Configure(setupAction);

            return builder;
        }

        private static void RegisterFeatures(ApplicationPartManager partManager)
        {
            partManager.FeatureProviders.Add(new MetadataReferenceFeatureProvider());
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IActionDescriptorProvider, PageActionDescriptorProvider>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IActionInvokerProvider, PageActionInvokerProvider>());

            services.TryAddSingleton<IPageFactory, DefaultPageFactory>();
            services.TryAddSingleton<IPageActivator, DefaultPageActivator>();
            services.TryAddSingleton<IPageModelFactory, DefaultPageModelFactory>();
            services.TryAddSingleton<IPageModelActivator, DefaultPageModelActivator>();
            services.TryAddSingleton<IPageHandlerMethodSelector, DefaultPageHandlerMethodSelector>();
            services.AddWebMail();

            services.TryAddSingleton<RazorProject>((s) =>
            {
                var options = s.GetRequiredService<IOptions<WebPagesOptions>>();
                return RazorProject.Create(new CompositeFileProvider(options.Value.FileProviders));
            });

            services.TryAddSingleton<IPageLoader, DefaultPageLoader>();
            services.TryAddSingleton<PageRazorEngineHost>();
            services.TryAddSingleton<ReferenceManager, ApplicationPartManagerReferenceManager>();
            services.TryAddSingleton<CSharpCompilationFactory, DefaultCSharpCompilationFactory>();

            services.Replace(ServiceDescriptor.Singleton<IRazorPageActivator, HackedRazorPageActivator>()); // Awful Hack

            services.TryAddSingleton<PageArgumentBinder, DefaultPageArgumentBinder>();

            services.TryAddSingleton<PageResultExecutor>();

            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<WebPagesOptions>, DefaultWebPagesOptionsSetup>());

            services.TryAddSingleton<TempDataPropertyProvider>();
        }
    }
}
