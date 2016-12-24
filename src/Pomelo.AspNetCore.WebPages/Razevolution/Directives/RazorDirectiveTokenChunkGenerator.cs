// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Chunks.Generators;
using Microsoft.AspNetCore.Razor.Parser.SyntaxTree;

namespace Pomelo.AspNetCore.WebPages.Razevolution.Directives
{
    public class RazorDirectiveTokenChunkGenerator : SpanChunkGenerator
    {
        private readonly RazorDirectiveTokenDescriptor _tokenDescriptor;

        public RazorDirectiveTokenChunkGenerator(RazorDirectiveTokenDescriptor tokenDescriptor)
        {
            _tokenDescriptor = tokenDescriptor;
        }

        public override void GenerateChunk(Span target, ChunkGeneratorContext context)
        {
            context.ChunkTreeBuilder.AddDirectiveToken(target.Content, _tokenDescriptor, target);
        }
    }
}
