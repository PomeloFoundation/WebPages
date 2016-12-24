// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Pomelo.AspNetCore.WebPages.Razevolution.Directives;
using Microsoft.AspNetCore.Razor.CodeGenerators;

namespace Pomelo.AspNetCore.WebPages.Razevolution.IR
{
    public class RazorDirectiveToken : ICSharpSource, ISourceMapped
    {
        public MappingLocation DocumentLocation { get; set; }

        public RazorDirectiveTokenDescriptor Descriptor { get; set; }

        public string Value { get; set; }
    }
}
