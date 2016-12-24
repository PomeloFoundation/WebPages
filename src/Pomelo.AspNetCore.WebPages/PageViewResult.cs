// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Pomelo.AspNetCore.WebPages.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;

namespace Pomelo.AspNetCore.WebPages
{
    public class PageViewResult : IActionResult
    {
        public PageViewResult(Page page)
        {
            Page = page;
        }

        public PageViewResult(Page page, object model)
        {
            Page = page;
            Model = model;
        }

        public string ContentType { get; set; }

        public object Model { get; }

        public Page Page { get; }

        public int? StatusCode { get; set; }

        public Task ExecuteResultAsync(ActionContext context)
        {
            if (!object.ReferenceEquals(context, Page.PageContext))
            {
                throw new InvalidOperationException();
            }

            var executor = context.HttpContext.RequestServices.GetRequiredService<PageResultExecutor>();
            return executor.ExecuteAsync((PageContext)context, this);
        }
    }
}
