// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Pomelo.AspNetCore.WebPages.Compilation;
using Pomelo.AspNetCore.WebPages.Razevolution.Directives;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Microsoft.AspNetCore.Razor.Parser.Internal;
using OldParser = Microsoft.AspNetCore.Razor.Parser;

namespace Pomelo.AspNetCore.WebPages.Razevolution
{
    public abstract class RazorParser
    {
        public static RazorSyntaxTree Parse(RazorSourceDocument document) =>
            Parse(document, additionalDirectives: Enumerable.Empty<RazorDirectiveDescriptor>());

        public static RazorSyntaxTree Parse(RazorSourceDocument document, IEnumerable<RazorDirectiveDescriptor> additionalDirectives)
        {
            if (document == null)
            {
                throw new ArgumentException(nameof(document));
            }

            var codeParser = new PageCodeParser(additionalDirectives);
            var markupParser = new HtmlMarkupParser();

            var parser = new OldParser.RazorParser(codeParser, markupParser, new NullTagHelperDescriptorResolver());

            ParserResults result;
            using (var reader = document.CreateReader())
            {
                result = parser.Parse(reader);
            }

            return RazorSyntaxTree.Create(result.Document, result.ParserErrors);
        }

        // Used to avoid having anything to do with TagHelpers during the parsing phase, because this
        // should not be part of the parsing phase.
        private class NullTagHelperDescriptorResolver : ITagHelperDescriptorResolver
        {
            public IEnumerable<TagHelperDescriptor> Resolve(TagHelperDescriptorResolutionContext resolutionContext)
            {
                return Enumerable.Empty<TagHelperDescriptor>();
            }
        }
    }
}
