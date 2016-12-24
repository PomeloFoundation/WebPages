// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Razor.Parser.SyntaxTree;

namespace Pomelo.AspNetCore.WebPages.Razevolution
{
    public class PageDirectiveFeature : IRazorEngineFeature, ISyntaxTreePass
    {
        public static string GetRouteTemplate(RazorSyntaxTree syntaxTree)
        {
            Block expression;
            Span route;
            if (TryFindPageDirectiveBlocks(syntaxTree.Root, out expression, out route) &&
                route != null)
            {
                var trimmed = route.Content.Trim();
                return trimmed.Substring(1, trimmed.Length - 2);
            }

            return null;
        }

        public RazorEngine Engine { get; set; }

        public int Order => 200;

        public RazorSyntaxTree Execute(RazorCodeDocument document, RazorSyntaxTree syntaxTree)
        {
            Block expression;
            Span route;
            if (!TryFindPageDirectiveBlocks(syntaxTree.Root, out expression, out route))
            {
                return syntaxTree;
            }

            var builder = new BlockBuilder(syntaxTree.Root);
            builder.Children.Remove(expression);
            builder.Children.Remove(route);

            return RazorSyntaxTree.Create(builder.Build(), syntaxTree.Diagnostics);
        }

        private static bool TryFindPageDirectiveBlocks(Block root, out Block expression, out Span route)
        {
            // We don't recurse here - so this will only apply to the top-level block.
            for (var i = 0; i < root.Children.Count; i++)
            {
                expression = root.Children[i] as Block;
                if (expression == null || expression.Type != BlockType.Expression || expression.Children.Count < 2)
                {
                    expression = null;
                    route = null;
                    continue;
                }

                // @page will be recognized as an expression block (transition -> code)
                // @page "some/route" will be recognized as an expression block (transition -> code)
                // followed by markup
                var transition = expression.Children[0] as Span;
                var code = expression.Children[1] as Span;
                if (transition == null ||
                    code == null ||
                    transition.Kind != SpanKind.Transition ||
                    code.Kind != SpanKind.Code ||
                    !string.Equals(code.Content, "route", StringComparison.Ordinal))
                {
                    expression = null;
                    route = null;
                    continue;
                }
                
                // The next span *might* be a route template
                route = code.Next;
                var content = route.Content.Trim();
                if (route.Kind != SpanKind.Markup ||
                    content.Length < 2 ||
                    content[0] != '"' ||
                    content[content.Length - 1] != '"')
                {
                    // This is @page without a route directive
                    route = null;
                    return true;
                }

                return true;
            }

            expression = null;
            route = null;
            return false;
        }
    }
}
