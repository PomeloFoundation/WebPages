// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;

namespace Pomelo.AspNetCore.WebPages.Razevolution
{
    public class DefaultCSharpSourceLoweringPhase : ICSharpSourceLoweringPhase
    {
        public RazorEngine Engine { get; set; }

        public void Execute(RazorCodeDocument document)
        {
            var chunkTree = document.GetChunkTree();

            if (chunkTree == null)
            {
                throw new InvalidOperationException("Need to create the chunk tree");
            }

            var sourceTree = document.GetSourceTree();

            if (sourceTree == null)
            {
                var loweringFeature = Engine.Features.OfType<DefaultCSharpSourceLoweringFeature>().FirstOrDefault();
                if (loweringFeature == null)
                {
                    throw new InvalidOperationException("Need to create the source tree");
                }

                sourceTree = loweringFeature.Execute(document, chunkTree);

            }

            var passes = Engine.Features.OfType<ICSharpSourceTreePass>().OrderBy(p => p.Order).ToArray();
            foreach (var pass in passes)
            {
                sourceTree = pass.Execute(document, sourceTree);
            }

            document.SetSourceTree(sourceTree);
        }
    }
}
