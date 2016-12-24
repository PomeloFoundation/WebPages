// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;

namespace Pomelo.AspNetCore.WebPages.Infrastructure
{
    public class PageActionInvokerProvider : IActionInvokerProvider
    {
        private readonly DiagnosticListener _diagnosticSource;
        private readonly ILogger _logger;
        private readonly IFilterProvider[] _filterProviders;
        private readonly IPageFactory _factory;
        private readonly IPageModelFactory _modelFactory;
        private readonly IPageLoader _loader;
        private readonly IPageHandlerMethodSelector _selector;
        private readonly IValueProviderFactory[] _valueProviderFactories;
        private readonly IModelMetadataProvider _metadataProvider;
        private readonly ITempDataDictionaryFactory _tempDataFactory;
        private readonly IOptions<MvcViewOptions> _viewOptions;
        private readonly TempDataPropertyProvider _pagePersistedPropertyProvider;

        public PageActionInvokerProvider(
            IPageFactory factory,
            IPageModelFactory modelFactory,
            IPageHandlerMethodSelector selector,
            IPageLoader loader,
            DiagnosticListener diagnosticSource,
            ILoggerFactory loggerFactory,
            IEnumerable<IFilterProvider> filterProviders,
            IModelMetadataProvider metadataProvider,
            ITempDataDictionaryFactory tempDataFactory,
            IOptions<MvcOptions> options,
            IOptions<MvcViewOptions> viewOptions,
            TempDataPropertyProvider pagePersistedPropertyProvider)
        {
            _factory = factory;
            _modelFactory = modelFactory;
            _selector = selector;
            _diagnosticSource = diagnosticSource;
            _loader = loader;
            _metadataProvider = metadataProvider;
            _tempDataFactory = tempDataFactory;
            _viewOptions = viewOptions;
            _pagePersistedPropertyProvider = pagePersistedPropertyProvider;

            _filterProviders = filterProviders.OrderBy(fp => fp.Order).ToArray();
            _logger = loggerFactory.CreateLogger<PageActionInvoker>();
            _valueProviderFactories = options.Value.ValueProviderFactories.ToArray();
        }

        public int Order { get; set; }

        public void OnProvidersExecuting(ActionInvokerProviderContext context)
        {
            var actionDescriptor = context.ActionContext.ActionDescriptor as PageActionDescriptor;
            if (actionDescriptor != null)
            {
                var itemCount = actionDescriptor.FilterDescriptors?.Count ?? 0;
                var filterItems = new List<FilterItem>(itemCount);
                for (var i = 0; i < itemCount; i++)
                {
                    var item = new FilterItem(actionDescriptor.FilterDescriptors[i]);
                    filterItems.Add(item);
                }

                var filterProviderContext = new FilterProviderContext(context.ActionContext, filterItems);

                for (var i = 0; i < _filterProviders.Length; i++)
                {
                    _filterProviders[i].OnProvidersExecuting(filterProviderContext);
                }

                for (var i = _filterProviders.Length - 1; i >= 0; i--)
                {
                    _filterProviders[i].OnProvidersExecuted(filterProviderContext);
                }

                var filters = new IFilterMetadata[filterProviderContext.Results.Count];
                for (var i = 0; i < filterProviderContext.Results.Count; i++)
                {
                    filters[i] = filterProviderContext.Results[i].Filter;
                }

                var compiledType = _loader.Load(actionDescriptor).GetTypeInfo();
                var modelType = compiledType.GetProperty("Model")?.PropertyType.GetTypeInfo();

                var compiledActionDescriptor = new CompiledPageActionDescriptor(actionDescriptor)
                {
                    ModelType = modelType,
                    PageType = compiledType,
                    HandlerMethods = new List<HandlerMethodDescriptor>(),
                };

                if (modelType != null && modelType != compiledType)
                {
                    // If the model and page type are different discover handler methods on the model as well.
                    PopulateHandlerMethodDescriptors(modelType, compiledActionDescriptor);
                }

                PopulateHandlerMethodDescriptors(compiledType, compiledActionDescriptor);

                context.Result = new PageActionInvoker(
                    _diagnosticSource,
                    _logger,
                    _factory,
                    _modelFactory,
                    _selector,
                    _metadataProvider,
                    _tempDataFactory,
                    _viewOptions,
                    filters,
                    _valueProviderFactories,
                    context.ActionContext,
                    compiledActionDescriptor,
                    _pagePersistedPropertyProvider);
            }
        }

        public void OnProvidersExecuted(ActionInvokerProviderContext context)
        {
        }

        private static void PopulateHandlerMethodDescriptors(TypeInfo type, CompiledPageActionDescriptor actionDescriptor)
        {
            var methods = type.GetMethods();
            for (var i = 0; i < methods.Length; i++)
            {
                var method = methods[i];
                if (method.Name.StartsWith("OnGet", StringComparison.Ordinal) ||
                    method.Name.StartsWith("OnPost", StringComparison.Ordinal) ||
                    method.Name.StartsWith("OnPut", StringComparison.Ordinal) ||
                    method.Name.StartsWith("OnDelete", StringComparison.Ordinal))
                {
                    actionDescriptor.HandlerMethods.Add(new HandlerMethodDescriptor()
                    {
                        Method = method,
                    });
                }
            }
        }
    }
}
