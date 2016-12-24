// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.AspNetCore.Razor.CodeGenerators;

namespace Pomelo.AspNetCore.WebPages.Razevolution.IR
{
    [DebuggerDisplay("@...[{Children.Count, nq}]...")]
    public class RenderExpression : ICSharpSource, ISourceMapped
    {
        public MappingLocation DocumentLocation { get; set; }

        public CSharpBlock Expression { get; set; }

        public int Padding { get; set; }
    }
}
