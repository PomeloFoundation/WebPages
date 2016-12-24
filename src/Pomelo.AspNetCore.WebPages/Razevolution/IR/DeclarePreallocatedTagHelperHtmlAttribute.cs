// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Pomelo.AspNetCore.WebPages.Razevolution.IR
{
    public class DeclarePreallocatedTagHelperHtmlAttribute : ICSharpSource
    {
        public string VariableName { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public HtmlAttributeValueStyle ValueStyle { get; set; }
    }
}
