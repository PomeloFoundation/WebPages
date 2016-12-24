// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;

namespace Pomelo.AspNetCore.WebPages.Razevolution
{
    public class TagHelperFeature : IRazorEngineFeature
    {
        public TagHelperFeature(ITagHelperDescriptorResolver resolver)
        {
            Resolver = resolver;
        }

        public RazorEngine Engine { get; set; }

        public ITagHelperDescriptorResolver Resolver { get; }
    }
}
