// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Pomelo.AspNetCore.WebPages.Razevolution.IR;

namespace Pomelo.AspNetCore.WebPages.Razevolution.CSharpRendering
{
    public static class CSharpRenderingContextExtensions
    {
        public static IEnumerable<IRazorDirective> GetDirectives(this CSharpRenderingContext context)
        {
            var directives = context.Items[typeof(IEnumerable<IRazorDirective>)] as IEnumerable<IRazorDirective>;

            return directives;
        }

        public static void SetDirectives(this CSharpRenderingContext context, IEnumerable<IRazorDirective> directives)
        {
            context.Items[typeof(IEnumerable<IRazorDirective>)] = directives;
        }

        public static IDisposable UseRenderingConventions(
            this CSharpRenderingContext context,
            CSharpRenderingConventions conventions)
        {
            var initialConventions = context.GetRenderingConventions();
            var scope = new ActionScope(() =>
            {
                context.SetRenderingConventions(initialConventions);
            });

            context.SetRenderingConventions(conventions);

            return scope;
        }

        public static CSharpRenderingConventions GetRenderingConventions(this CSharpRenderingContext context)
        {
            var conventions = context.Items[typeof(CSharpRenderingConventions)] as CSharpRenderingConventions;

            if (conventions == null)
            {
                conventions = new CSharpRenderingConventions(context);
                SetRenderingConventions(context, conventions);
            }

            return conventions;
        }

        private static void SetRenderingConventions(
            this CSharpRenderingContext context,
            CSharpRenderingConventions conventions)
        {
            context.Items[typeof(CSharpRenderingConventions)] = conventions;
        }

        public static IDisposable UseRenderTagHelperContext(
            this CSharpRenderingContext context,
            RenderTagHelperContext renderTagHelperContext)
        {
            var initialContext = context.GetRenderTagHelperContext();
            var scope = new ActionScope(() =>
            {
                context.SetRenderTagHelperContext(initialContext);
            });

            context.SetRenderTagHelperContext(renderTagHelperContext);

            return scope;
        }

        public static RenderTagHelperContext GetRenderTagHelperContext(this CSharpRenderingContext context)
        {
            var renderTagHelperContext = context.Items[typeof(RenderTagHelperContext)] as RenderTagHelperContext;

            return renderTagHelperContext;
        }

        private static void SetRenderTagHelperContext(
            this CSharpRenderingContext context,
            RenderTagHelperContext renderTagHelperContext)
        {
            context.Items[typeof(RenderTagHelperContext)] = renderTagHelperContext;
        }



        // TODO: We have other impls like this, should we .Internal and share?
        private class ActionScope : IDisposable
        {
            private readonly Action _onDispose;

            public ActionScope(Action onDispose)
            {
                _onDispose = onDispose;
            }

            public void Dispose()
            {
                _onDispose();
            }
        }
    }
}
