﻿// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Razor.CodeGenerators;

namespace Pomelo.AspNetCore.WebPages.Razevolution.CSharpRendering
{
    public class LinePragmaWriter : IDisposable
    {
        private readonly CSharpCodeWriter _writer;
        private readonly int _startIndent;

        public LinePragmaWriter(CSharpCodeWriter writer, MappingLocation documentLocation)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (documentLocation == null)
            {
                throw new ArgumentNullException(nameof(documentLocation));
            }

            _writer = writer;
            _startIndent = _writer.CurrentIndent;
            _writer.ResetIndent();
            _writer.WriteLineNumberDirective(documentLocation, documentLocation.FilePath);
            _writer.SetIndent(_startIndent);
        }

        public void Dispose()
        {
            // Need to add an additional line at the end IF there wasn't one already written.
            // This is needed to work with the C# editor's handling of #line ...
            var builder = _writer.Builder;
            var endsWithNewline = builder.Length > 0 && builder[builder.Length - 1] == '\n';

            // Always write at least 1 empty line to potentially separate code from pragmas.
            _writer.WriteLine();

            // Check if the previous empty line wasn't enough to separate code from pragmas.
            if (!endsWithNewline)
            {
                _writer.WriteLine();
            }

            _writer
                .ResetIndent()
                .WriteLineDefaultDirective()
                .WriteLineHiddenDirective()
                .SetIndent(_startIndent);
        }
    }
}
