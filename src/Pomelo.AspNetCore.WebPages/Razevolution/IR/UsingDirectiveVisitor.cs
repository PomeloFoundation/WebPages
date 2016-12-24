// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.Chunks;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using Microsoft.AspNetCore.Razor.Parser.SyntaxTree;

namespace Pomelo.AspNetCore.WebPages.Razevolution.IR
{
    public class UsingDirectiveVisitor : ChunkVisitor
    {
        private readonly CSharpSourceLoweringContext _context;
        private readonly ISet<string> _addedImports;

        public UsingDirectiveVisitor(ISet<string> addedImports, CSharpSourceLoweringContext context)
        {
            _context = context;
            _addedImports = addedImports;
        }

        protected override void Visit(UsingChunk chunk)
        {
            if (chunk == null)
            {
                throw new ArgumentNullException(nameof(chunk));
            }

            // We don't want to attempt to add duplicate namespace imports.
            if (!_addedImports.Add(chunk.Namespace))
            {
                return;
            }

            var documentContent = ((Span)chunk.Association).Content.Trim();
            var documentLocation = new MappingLocation(chunk.Start, documentContent.Length);
            var importNamespace = new ImportNamespace
            {
                // TODO: The document content has the entire "using X" string, this only has the X. Are mapping locations accurate?
                Namespace = chunk.Namespace,
                DocumentLocation = documentLocation
            };

            _context.Builder.Add(importNamespace);
        }
    }
}