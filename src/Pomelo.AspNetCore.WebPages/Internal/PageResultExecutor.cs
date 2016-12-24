// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Pomelo.AspNetCore.WebPages.ModelBinding;

namespace Pomelo.AspNetCore.WebPages.Internal
{
    public class PageResultExecutor
    {
        public static readonly string DefaultContentType = "text/html; charset=utf-8";

        private readonly HtmlEncoder _htmlEncoder;
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly IRazorPageActivator _razorPageActivator;
        private readonly PageArgumentBinder _pageArgumentBinder;

        public PageResultExecutor(
            IHttpResponseStreamWriterFactory writerFactory, 
            IRazorViewEngine razorViewEngine,
            IRazorPageActivator razorPageActivator,
            HtmlEncoder htmlEncoder,
            PageArgumentBinder pageArgumentBinder)
        {
            WriterFactory = writerFactory;
            _razorViewEngine = razorViewEngine;
            _razorPageActivator = razorPageActivator;
            _htmlEncoder = htmlEncoder;
            _pageArgumentBinder = pageArgumentBinder;
        }

        protected IHttpResponseStreamWriterFactory WriterFactory { get; }

        public Task ExecuteAsync(PageContext pageContext, PageViewResult result)
        {
            if (result.Model != null)
            {
                result.Page.PageContext.ViewData.Model = result.Model;
            }

            var view = new RazorView(_razorViewEngine, _razorPageActivator, new IRazorPage[0], result.Page, _htmlEncoder);
            return ExecuteAsync(pageContext, view, result.ContentType, result.StatusCode);
        }

        public virtual async Task ExecuteAsync(
            PageContext pageContext,
            RazorView view,
            string contentType,
            int? statusCode)
        {
            if (pageContext == null)
            {
                throw new ArgumentNullException(nameof(pageContext));
            }

            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            var razorPageType = view.RazorPage.GetType();
            var result  = await _pageArgumentBinder.BindModelAsync(pageContext, razorPageType, view.RazorPage, razorPageType.Name);
            var response = pageContext.HttpContext.Response;

            string resolvedContentType = null;
            Encoding resolvedContentTypeEncoding = null;
            ResponseContentTypeHelper.ResolveContentTypeAndEncoding(
                contentType,
                response.ContentType,
                DefaultContentType,
                out resolvedContentType,
                out resolvedContentTypeEncoding);

            response.ContentType = resolvedContentType;

            if (statusCode != null)
            {
                response.StatusCode = statusCode.Value;
            }

            using (var writer = WriterFactory.CreateWriter(response.Body, resolvedContentTypeEncoding))
            {
                pageContext.Writer = writer;

                await view.RenderAsync(pageContext);
                ((Page)view.RazorPage).Dispose();

                // Perf: Invoke FlushAsync to ensure any buffered content is asynchronously written to the underlying
                // response asynchronously. In the absence of this line, the buffer gets synchronously written to the
                // response as part of the Dispose which has a perf impact.
                await writer.FlushAsync();
            }
        }
    }
}
