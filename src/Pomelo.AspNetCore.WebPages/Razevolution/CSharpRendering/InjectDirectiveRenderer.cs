// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using Pomelo.AspNetCore.WebPages.Razevolution.Directives;
using Pomelo.AspNetCore.WebPages.Razevolution.IR;

namespace Pomelo.AspNetCore.WebPages.Razevolution.CSharpRendering
{
    public class InjectDirectiveRenderer : ICSharpRenderer
    {
        private readonly string _injectAttribute;

        public InjectDirectiveRenderer(string injectAttribute)
        {
            _injectAttribute = $"[{injectAttribute}]";
        }

        public RazorEngine Engine { get; set; }

        public int Order { get; } = 1;

        public bool TryRender(ICSharpSource source, CSharpRenderingContext context)
        {
            if (source is ExecuteMethodDeclaration)
            {
                var directives = context.GetDirectives();
                foreach (var directive in directives)
                {
                    if (!string.Equals(directive.Name, "inject", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var typeName = directive.GetValue(RazorDirectiveTokenType.Type);
                    var memberName = directive.GetValue(RazorDirectiveTokenType.Member);

                    context.Writer
                        .WriteLine(_injectAttribute)
                        .Write("public global::")
                        .Write(typeName)
                        .Write(" ")
                        .Write(memberName)
                        .WriteLine(" { get; private set; }");
                }
            }

            return false;
        }
    }
}
