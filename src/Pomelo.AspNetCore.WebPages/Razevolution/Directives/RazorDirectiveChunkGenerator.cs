// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Chunks.Generators;
using Microsoft.AspNetCore.Razor.Parser.SyntaxTree;

namespace Pomelo.AspNetCore.WebPages.Razevolution.Directives
{
    public class RazorDirectiveChunkGenerator : ParentChunkGenerator
    {
        private readonly RazorDirectiveDescriptor _descriptor;

        public RazorDirectiveChunkGenerator(RazorDirectiveDescriptor descriptor)
        {
            _descriptor = descriptor;
        }

        public override void GenerateStartParentChunk(Block target, ChunkGeneratorContext context)
        {
            var chunk = context.ChunkTreeBuilder.StartParentChunk<RazorDirectiveChunk>(target);

            chunk.Descriptor = _descriptor;
        }

        public override void GenerateEndParentChunk(Block target, ChunkGeneratorContext context)
        {
            context.ChunkTreeBuilder.EndParentChunk();
        }
    }
}
