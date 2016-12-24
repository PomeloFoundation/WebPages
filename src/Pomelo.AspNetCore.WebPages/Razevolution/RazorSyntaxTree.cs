// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.Parser.SyntaxTree;

namespace Pomelo.AspNetCore.WebPages.Razevolution
{
    public abstract class RazorSyntaxTree
    {
        public static RazorSyntaxTree Create(Block root, IEnumerable<RazorError> diagnostics)
        {
            return new DefaultRazorSyntaxTree(root, new List<RazorError>(diagnostics));
        }

        public abstract IReadOnlyList<RazorError> Diagnostics { get; }

        public abstract Block Root { get; }

        private class DefaultRazorSyntaxTree : RazorSyntaxTree
        {
            public DefaultRazorSyntaxTree(Block root, IReadOnlyList<RazorError> diagnostics)
            {
                Root = root;
                Diagnostics = diagnostics;
            }

            public override IReadOnlyList<RazorError> Diagnostics { get; }

            public override Block Root { get; }
        }
    }
}
