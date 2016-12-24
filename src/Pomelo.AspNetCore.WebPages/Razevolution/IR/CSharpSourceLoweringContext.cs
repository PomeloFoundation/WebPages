// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor;

namespace Pomelo.AspNetCore.WebPages.Razevolution.IR
{
    public class CSharpSourceLoweringContext
    {
        public SourceTreeBuilder Builder { get; set; }

        public RazorEngineHost Host { get; set; }

        public string SourceFileName { get; set; }
    }
}