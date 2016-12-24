// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Pomelo.AspNetCore.WebPages.Razevolution;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Mvc;

namespace Pomelo.AspNetCore.WebPages.Infrastructure
{
    public class PageActionDescriptorProvider : IActionDescriptorProvider
    {
        private readonly RazorProject _project;
        private readonly MvcOptions _options;
        private readonly WebPagesOptions _pagesOptions;

        public PageActionDescriptorProvider(
            RazorProject project,
            IOptions<MvcOptions> options,
            IOptions<WebPagesOptions> pagesOptions)
        {
            _project = project;
            _options = options.Value;
            _pagesOptions = pagesOptions.Value;
        }

        public int Order { get; set; }

        public void OnProvidersExecuting(ActionDescriptorProviderContext context)
        {
            foreach (var item in EnumerateItems())
            {
                if (item.Filename.StartsWith("_"))
                {
                    // Pages like _PageImports should not be routable.
                    continue;
                }
                
                AddActionDescriptors(context.Results, item);
            }
        }

        public void OnProvidersExecuted(ActionDescriptorProviderContext context)
        {
        }

        private void AddActionDescriptors(IList<ActionDescriptor> actions, RazorProjectItem item)
        {
            var model = new Microsoft.AspNetCore.Mvc.ApplicationModels.PageModel(item.CominedPath, item.PathWithoutExtension);

            model.Selectors.Add(new SelectorModel()
            {
                AttributeRouteModel = new AttributeRouteModel()
                {
                    Template = GetRouteTemplate(item),
                }
            });

            foreach (var convention in _pagesOptions.Conventions)
            {
                convention.Apply(model);
            }

            var filters = new List<FilterDescriptor>(_options.Filters.Count + model.Filters.Count);
            for (var i = 0; i < _options.Filters.Count; i++)
            {
                filters.Add(new FilterDescriptor(_options.Filters[i], FilterScope.Global));
            }

            for (var i = 0; i < model.Filters.Count; i++)
            {
                filters.Add(new FilterDescriptor(model.Filters[i], FilterScope.Action));
            }

            foreach (var selector in model.Selectors)
            {
                actions.Add(new PageActionDescriptor()
                {
                    AttributeRouteInfo = new AttributeRouteInfo()
                    {
                        Name = selector.AttributeRouteModel.Name,
                        Order = selector.AttributeRouteModel.Order ?? 0,
                        Template = selector.AttributeRouteModel.Template,
                    },
                    DisplayName = $"Page: {item.Path}",
                    FilterDescriptors = filters,
                    Properties = new Dictionary<object, object>(model.Properties),
                    RelativePath = item.CominedPath,
                    RouteValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "route", item.PathWithoutExtension },
                    },
                    ViewEnginePath = item.Path,
                });
            }
        }

        private IEnumerable<RazorProjectItem> EnumerateItems()
        {
            return _project.EnumerateItems(_pagesOptions.PagesPath, ".cshtml");
        }

        private string GetRouteTemplate(RazorProjectItem item)
        {
            var source = item.ToSourceDocument();
            var syntaxTree = RazorParser.Parse(source);

            var template = PageDirectiveFeature.GetRouteTemplate(syntaxTree);

            if (template != null && template.Length > 0 && template[0] == '/')
            {
                return template.Substring(1);
            }

            if (template != null && template.Length > 1 && template[0] == '~' && template[1] == '/')
            {
                return template.Substring(1);
            }

            var @base = item.PathWithoutExtension.Substring(1);
            if (string.Equals("Index", @base, StringComparison.OrdinalIgnoreCase))
            {
                @base = string.Empty;
            }

            if (@base == string.Empty && string.IsNullOrEmpty(template))
            {
                return string.Empty;
            }
            else if (string.IsNullOrEmpty(template))
            {
                return @base;
            }
            else if (@base == string.Empty)
            {
                return template;
            }
            else
            {
                return @base + "/" + template;
            }
        }
    }
}
