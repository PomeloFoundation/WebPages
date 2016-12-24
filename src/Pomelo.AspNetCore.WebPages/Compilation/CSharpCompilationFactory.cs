// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CSharp;

namespace Pomelo.AspNetCore.WebPages.Compilation
{
    public abstract class CSharpCompilationFactory
    {
        public abstract CSharpCompilation Create();
    }
}
