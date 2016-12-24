// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace Pomelo.AspNetCore.WebPages.Infrastructure
{
    public class DefaultPageFactory : IPageFactory
    {
        private readonly IPageActivator _activator;

        public DefaultPageFactory(IPageActivator activator)
        {
            _activator = activator;
        }

        public object CreatePage(PageContext context)
        {
            if (Data.Database.factory == null)
                Data.Database.factory = context.HttpContext.RequestServices.GetService<Data.IDatabaseFactory>();

            var page = (Page)_activator.Create(context);
            page.PageContext = context;

            var properties = page.GetType().GetTypeInfo().GetProperties();
            foreach (var property in properties)
            {
                if (property.GetCustomAttribute(typeof(RazorInjectAttribute)) != null)
                {
                    var service = context.HttpContext.RequestServices.GetRequiredService(property.PropertyType);
                    (service as IViewContextAware)?.Contextualize(context);

                    property.SetValue(page, service);
                }
            }

            page.Binder.BindModelAsync(page.PageContext, page.GetType(), page, "page").Wait();

            return page;
        }

        public void ReleasePage(PageContext context, object page)
        {
            context.Page.Dispose();
            _activator.Release(context, page);
        }
    }
}
