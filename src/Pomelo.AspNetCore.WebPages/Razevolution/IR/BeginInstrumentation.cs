// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.CodeGenerators;

namespace Pomelo.AspNetCore.WebPages.Razevolution.IR
{
    public class BeginInstrumentation : ICSharpSource
    {
        public MappingLocation DocumentLocation { get; set; }

        public bool Literal { get; set; }
    }
}
