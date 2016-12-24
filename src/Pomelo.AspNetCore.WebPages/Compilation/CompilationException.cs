// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Diagnostics;

namespace Pomelo.AspNetCore.WebPages.Compilation
{
    public class CompilationException : Exception, ICompilationException
    {
        public CompilationException(List<CompilationFailure> compilationFailures)
        {
            CompilationFailures = compilationFailures;
        }

        public IEnumerable<CompilationFailure> CompilationFailures { get; }
    }
}
