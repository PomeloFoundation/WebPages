// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.Chunks;
using Microsoft.AspNetCore.Razor.CodeGenerators;

namespace Pomelo.AspNetCore.WebPages.Razevolution.IR
{
    public class FunctionsDirectiveVisitor : ChunkVisitor
    {
        private readonly CSharpSourceLoweringContext _context;

        public FunctionsDirectiveVisitor(CSharpSourceLoweringContext context)
        {
            _context = context;
        }

        protected override void Visit(TypeMemberChunk chunk)
        {
            if (string.IsNullOrEmpty(chunk.Code))
            {
                return;
            }

            var documentLocation = CreateMappingLocation(chunk.Start, chunk.Code.Length);
            var statement = new RenderStatement
            {
                Code = chunk.Code,
                DocumentLocation = documentLocation,
            };
            _context.Builder.Add(statement);
        }

        private MappingLocation CreateMappingLocation(SourceLocation location, int contentLength)
        {
            // TODO: Code smell, should refactor mapping location to take a file path
            var filePath = location.FilePath ?? _context.SourceFileName;
            var fileMappedLocation = new SourceLocation(filePath, location.AbsoluteIndex, location.LineIndex, location.CharacterIndex);

            return new MappingLocation(fileMappedLocation, contentLength);
        }
    }
}