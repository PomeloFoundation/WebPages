// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Razor;

namespace Pomelo.AspNetCore.WebPages.Razevolution
{
    public static class RazorErrorExtensions
    {
        public static DiagnosticMessage ToDiagnosticMessage(this RazorError error)
        {
            if (error == null)
            {
                throw new ArgumentNullException(nameof(error));
            }

            var location = error.Location;
            return new DiagnosticMessage(
                message: error.Message,
                formattedMessage: $"{error} ({location.LineIndex},{location.CharacterIndex}) {error.Message}",
                filePath: location.FilePath,
                startLine: location.LineIndex + 1,
                startColumn: location.CharacterIndex,
                endLine: location.LineIndex + 1,
                endColumn: location.CharacterIndex + error.Length);
        }
    }
}
