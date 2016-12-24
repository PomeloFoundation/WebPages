// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Pomelo.AspNetCore.WebPages.Razevolution.IR
{
    [DebuggerDisplay("AddTagHelperHtmlAttribute({Name,nq}=\"...[{(Value as CSharpBlock)?.Children.Count ?? 1, nq}]...\")")]
    public class AddTagHelperHtmlAttribute : ICSharpSource
    {
        public string Name { get; set; }

        public IList<ICSharpSource> ValuePieces { get; set; }

        public HtmlAttributeValueStyle ValueStyle { get; set; }
    }
}
