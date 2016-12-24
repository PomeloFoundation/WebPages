// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Pomelo.AspNetCore.WebPages.Razevolution.IR;

namespace Pomelo.AspNetCore.WebPages.Razevolution.Directives
{
    public class RazorDirectiveDiscoveryPass : ICSharpSourceTreePass
    {
        public RazorEngine Engine { get; set; }

        public int Order { get; } = int.MaxValue;

        public CSharpSourceTree Execute(RazorCodeDocument document, CSharpSourceTree sourceTree)
        {
            var activeDirectives = document.GetCSharpRenderingDirectives();
            if (activeDirectives == null)
            {
                var discoveredDirectives = new List<IRazorDirective>();
                AddDiscoveredDirectives(sourceTree, discoveredDirectives);

                document.SetCSharpRenderingDirectives(discoveredDirectives);
            }

            return sourceTree;
        }

        private void AddDiscoveredDirectives(ICSharpSource source, List<IRazorDirective> directives)
        {
            if (source is IRazorDirective)
            {
                directives.Add((IRazorDirective)source);
            }

            if (source is CSharpBlock)
            {
                var block = (CSharpBlock)source;
                for (var i = 0; i < block.Children.Count; i++)
                {
                    AddDiscoveredDirectives(block.Children[i], directives);
                }
            }
        }
    }
}
