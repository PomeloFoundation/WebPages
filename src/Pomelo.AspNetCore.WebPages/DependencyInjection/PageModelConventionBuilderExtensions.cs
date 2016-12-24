// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Pomelo.AspNetCore.WebPages;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class PageModelConventionBuilderExtensions
    {
        public static IPageModelConventionBuilder SetPagesRoot(this IPageModelConventionBuilder builder, string path)
        {
            if (builder is WebPagesOptions)
            {
                (builder as WebPagesOptions).PagesPath = path;
            }
            return builder;
        }

        public static IPageModelConventionBuilder AllowAnonymousToPage(this IPageModelConventionBuilder builder, string path)
        {
            builder.Add(new PageConvention(path, (m) => m.Filters.Add(new AllowAnonymousFilter())));
            return builder;
        }

        public static IPageModelConventionBuilder AllowAnonymousToFolder(this IPageModelConventionBuilder builder, string path)
        {
            builder.Add(new FolderConvention(path, (m) => m.Filters.Add(new AllowAnonymousFilter())));
            return builder;
        }

        public static IPageModelConventionBuilder AuthorizePage(this IPageModelConventionBuilder builder, string path, string policy)
        {
            var filter = new ConstructableAuthorizationFilter(new AuthorizeAttribute(policy));
            builder.Add(new PageConvention(path, (m) => m.Filters.Add(filter)));
            return builder;
        }

        public static IPageModelConventionBuilder AuthorizeFolder(this IPageModelConventionBuilder builder, string path, string policy)
        {
            var filter = new ConstructableAuthorizationFilter(new AuthorizeAttribute(policy));
            builder.Add(new FolderConvention(path, (m) => m.Filters.Add(filter)));
            return builder;
        }

        private class ConstructableAuthorizationFilter : IFilterFactory
        {
            private readonly AuthorizeAttribute _attribute;

            public ConstructableAuthorizationFilter(AuthorizeAttribute attribute)
            {
                _attribute = attribute;
            }

            public bool IsReusable => true;

            public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
            {
                var service = serviceProvider.GetRequiredService<IAuthorizationPolicyProvider>();
                return new AuthorizeFilter(service, new IAuthorizeData[] { _attribute });
            }
        }

        private class PageConvention : IPageModelConvention
        {
            private readonly string _path;
            private readonly Action<Microsoft.AspNetCore.Mvc.ApplicationModels.PageModel> _action;

            public PageConvention(string path, Action<Microsoft.AspNetCore.Mvc.ApplicationModels.PageModel> action)
            {
                _path = path;
                _action = action;
            }

            public void Apply(Microsoft.AspNetCore.Mvc.ApplicationModels.PageModel model)
            {
                if (string.Equals(_path, model.ViewEnginePath, StringComparison.OrdinalIgnoreCase))
                {
                    _action(model);
                }
            }
        }

        private class FolderConvention : IPageModelConvention
        {
            private readonly string _path;
            private readonly Action<Microsoft.AspNetCore.Mvc.ApplicationModels.PageModel> _action;

            public FolderConvention(string path, Action<Microsoft.AspNetCore.Mvc.ApplicationModels.PageModel> action)
            {
                _path = path;
                _action = action;
            }

            public void Apply(Microsoft.AspNetCore.Mvc.ApplicationModels.PageModel model)
            {

                if (_path == "/" ||
                    
                    (model.ViewEnginePath.StartsWith(_path, StringComparison.OrdinalIgnoreCase) &&
                    model.ViewEnginePath.Length > _path.Length &&
                    model.ViewEnginePath[_path.Length] == '/'))
                {
                    _action(model);
                }
            }
        }
    }
}
