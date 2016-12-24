// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace Pomelo.AspNetCore.WebPages.Infrastructure
{
    public class DefaultPageModelFactory : IPageModelFactory
    {
        private readonly IPageModelActivator _activator;

        public DefaultPageModelFactory(IPageModelActivator activator)
        {
            _activator = activator;
        }

        public object CreateModel(PageContext context)
        {
            var model = (object)_activator.Create(context);

            var properties = model.GetType().GetTypeInfo().GetProperties();
            foreach (var property in properties)
            {
                if (property.GetCustomAttribute(typeof(PageContextAttribute)) != null)
                {
                    property.SetValue(model, context);
                }
            }

            return model;
        }

        public void ReleaseModel(PageContext context, object model)
        {
            _activator.Release(context, model);
        }
    }
}
