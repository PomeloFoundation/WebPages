// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.AspNetCore.Razor.CodeGenerators;

namespace Pomelo.AspNetCore.WebPages.Razevolution.IR
{
    [DebuggerDisplay("{Code, nq}")]
    public class RenderStatement : ICSharpSource, ISourceMapped
    {
        public MappingLocation DocumentLocation { get; set; }

        public string Code { get; set; }

        public int Padding { get; set; }
    }
}
