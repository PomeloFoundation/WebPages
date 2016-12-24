// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.AspNetCore.Razor.CodeGenerators;

namespace Pomelo.AspNetCore.WebPages.Razevolution.IR
{
    [DebuggerDisplay("using {Namespace, nq};")]
    public class ImportNamespace : ICSharpSource, ISourceMapped
    {
        public MappingLocation DocumentLocation { get; set; }

        public string Namespace { get; set; }
    }
}
