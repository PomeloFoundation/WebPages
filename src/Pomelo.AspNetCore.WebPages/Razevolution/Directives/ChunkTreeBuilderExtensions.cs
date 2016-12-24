// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Chunks;
using Microsoft.AspNetCore.Razor.Parser.SyntaxTree;

namespace Pomelo.AspNetCore.WebPages.Razevolution.Directives
{
    public static class ChunkTreeBuilderExtensions
    {
        public static void AddDirectiveToken(
            this ChunkTreeBuilder builder,
            string value,
            RazorDirectiveTokenDescriptor tokenDescriptor,
            SyntaxTreeNode association)
        {
            builder.AddChunk(
                new RazorDirectiveTokenChunk
                {
                    Value = value,
                    Descriptor = tokenDescriptor,
                },
                association);
        }
    }
}
