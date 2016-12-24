// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Pomelo.AspNetCore.WebPages.Infrastructure
{
    public class DefaultPageActivator : IPageActivator
    {
        public object Create(PageContext context)
        {
            return ActivatorUtilities.CreateInstance(
                context.HttpContext.RequestServices, 
                context.ActionDescriptor.PageType.AsType());
        }

        public void Release(PageContext context, object page)
        {
            (page as IDisposable)?.Dispose();
        }
    }
}
