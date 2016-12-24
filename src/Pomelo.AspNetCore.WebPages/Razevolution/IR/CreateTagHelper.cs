// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;

namespace Pomelo.AspNetCore.WebPages.Razevolution.IR
{
    [DebuggerDisplay("CreateTagHelper<{TagHelperTypeName, nq}>()")]
    public class CreateTagHelper : ICSharpSource
    {
        public string TagHelperTypeName { get; set; }

        public TagHelperDescriptor AssociatedDescriptor { get; set; }
    }
}
