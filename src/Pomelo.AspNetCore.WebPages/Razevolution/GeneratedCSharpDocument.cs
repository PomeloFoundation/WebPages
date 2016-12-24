// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.CodeGenerators;

namespace Pomelo.AspNetCore.WebPages.Razevolution
{
    public class GeneratedCSharpDocument
    {
        public string GeneratedCode { get; set; }

        public IReadOnlyList<LineMapping> LineMappings { get; set; }
    }
}
