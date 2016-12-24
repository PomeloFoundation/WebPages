// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Pomelo.AspNetCore.WebPages.Razevolution.IR
{
    [DebuggerDisplay("BeginTagHelperScope({TagName}, TagMode.{TagMode}, async () => \\{...[{Children.Count}]...\\})]")]
    public class InitializeTagHelperStructure : CSharpBlock
    {
        public string TagName { get; set; }

        public TagMode TagMode { get; set; }
    }
}
