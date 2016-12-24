// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Razor.CodeGenerators;

namespace Pomelo.AspNetCore.WebPages.Razevolution.IR
{
    [DebuggerDisplay("RenderAttribute({Prefix, nq}...[{ValuePieces.Count, nq}]...{Suffix, nq})")]
    public class RenderConditionalAttribute : ICSharpSource, ISourceMapped
    {
        public MappingLocation DocumentLocation { get; set; }

        public string Name { get; set; }

        public string Prefix { get; set; }

        public IList<ICSharpSource> ValuePieces { get; set; }

        public string Suffix { get; set; }
    }
}
