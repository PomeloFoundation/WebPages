// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.CodeGenerators;

namespace Pomelo.AspNetCore.WebPages.Razevolution.CSharpRendering
{
    public static class CSharpCodeWriterExtensions
    {
        public static CSharpCodeWriter WriteLineNumberDirective(this CSharpCodeWriter writer, MappingLocation location, string file)
        {
            if (location.FilePath != null)
            {
                file = location.FilePath;
            }

            if (writer.Builder.Length >= writer.NewLine.Length && !writer.IsAfterNewLine)
            {
                writer.WriteLine();
            }

            var lineNumberAsString = (location.LineIndex + 1).ToString(CultureInfo.InvariantCulture);
            return writer.Write("#line ").Write(lineNumberAsString).Write(" \"").Write(file).WriteLine("\"");
        }

        public static IDisposable BuildLinePragma(this CSharpCodeWriter writer, MappingLocation documentLocation)
        {
            return new LinePragmaWriter(writer, documentLocation);
        }

        public static IDisposable NoIndent(this CSharpCodeWriter writer)
        {
            var currentIndent = writer.CurrentIndent;
            writer.ResetIndent();
            var scope = new ActionScope(() =>
            {
                writer.SetIndent(currentIndent);
            });

            return scope;
        }

        public static CSharpLineMappingWriter BuildCodeMapping(this CSharpCodeWriter writer, MappingLocation documentLocation)
        {
            // TODO: Update the primary API to accept mapping locations
            var sourceLocation = new SourceLocation(
                documentLocation.FilePath,
                documentLocation.AbsoluteIndex,
                documentLocation.LineIndex,
                documentLocation.CharacterIndex);
            var lineMappingWriter = new CSharpLineMappingWriter(writer, sourceLocation, documentLocation.ContentLength);

            return lineMappingWriter;
        }

        private class ActionScope : IDisposable
        {
            private readonly Action _onDispose;

            public ActionScope(Action onDispose)
            {
                _onDispose = onDispose;
            }

            public void Dispose()
            {
                _onDispose();
            }
        }
    }
}
