// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Microsoft.AspNetCore.Razor.Parser;
using Microsoft.AspNetCore.Razor.Parser.TagHelpers;
using Microsoft.AspNetCore.Razor.Parser.TagHelpers.Internal;

namespace Pomelo.AspNetCore.WebPages.Razevolution
{
    public class TagHelperBinderSyntaxTreePass : ISyntaxTreePass
    {
        public RazorEngine Engine { get; set; }

        public int Order => 100;

        public RazorSyntaxTree Execute(RazorCodeDocument document, RazorSyntaxTree syntaxTree)
        {
            var resolver = Engine.Features.OfType<TagHelperFeature>().FirstOrDefault()?.Resolver;
            if (resolver == null)
            {
                // No TagHelpers, so nothing to do.
                return syntaxTree;
            }

            var visitor = new TagHelperDirectiveSpanVisitor(resolver, document.ErrorSink);
            var descriptors = visitor.GetDescriptors(syntaxTree.Root);

            var rewriter = new TagHelperParseTreeRewriter(new TagHelperDescriptorProvider(descriptors));

            var context = new RewritingContext(syntaxTree.Root, document.ErrorSink);
            rewriter.Rewrite(context);
            return RazorSyntaxTree.Create(context.SyntaxTree, context.ErrorSink.Errors);
        }
    }
}
