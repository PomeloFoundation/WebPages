// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Pomelo.AspNetCore.WebPages.Compilation;
using Microsoft.AspNetCore.Razor.Chunks;
using Microsoft.AspNetCore.Razor.Chunks.Generators;

namespace Pomelo.AspNetCore.WebPages.Razevolution
{
    public class DefaultChunkTreeLoweringFeature : IChunkTreeLoweringFeature
    {
        private readonly PageRazorEngineHost _host;

        public DefaultChunkTreeLoweringFeature(PageRazorEngineHost host)
        {
            _host = host;
        }

        public RazorEngine Engine { get; set; }

        public RazorChunkTree Execute(RazorCodeDocument document, RazorSyntaxTree syntaxTree)
        {
            var classInfo = document.GetClassName();
            
            var generator = new RazorChunkGenerator(classInfo.Class, classInfo.Namespace, document.Source.Filename, _host);
            syntaxTree.Root.Accept(generator);
            foreach (var error in syntaxTree.Diagnostics)
            {
                generator.VisitError(error);
            }
            generator.OnComplete();

            return new DefaultRazorChunkTree(generator.Context.ChunkTreeBuilder.Root);
        }

        private class DefaultRazorChunkTree : RazorChunkTree
        {
            public DefaultRazorChunkTree(ChunkTree root)
            {
                Root = root;
            }

            public override ChunkTree Root { get; }
        }
    }
}
