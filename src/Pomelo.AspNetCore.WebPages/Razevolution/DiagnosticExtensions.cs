// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Pomelo.AspNetCore.WebPages.Razevolution
{
    public static class DiagnosticExtensions
    {
        public static DiagnosticMessage ToDiagnosticMessage(this Diagnostic diagnostic)
        {
            if (diagnostic == null)
            {
                throw new ArgumentNullException(nameof(diagnostic));
            }

            var mappedLineSpan = diagnostic.Location.GetMappedLineSpan();
            return new DiagnosticMessage(
                diagnostic.GetMessage(),
                CSharpDiagnosticFormatter.Instance.Format(diagnostic),
                mappedLineSpan.Path,
                mappedLineSpan.StartLinePosition.Line + 1,
                mappedLineSpan.StartLinePosition.Character + 1,
                mappedLineSpan.EndLinePosition.Line + 1,
                mappedLineSpan.EndLinePosition.Character + 1);
        }
    }
}
