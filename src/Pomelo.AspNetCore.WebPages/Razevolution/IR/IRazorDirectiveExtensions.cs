// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Pomelo.AspNetCore.WebPages.Razevolution.Directives;

namespace Pomelo.AspNetCore.WebPages.Razevolution.IR
{
    public static class IRazorDirectiveExtensions
    {
        public static string GetValue(this IRazorDirective directive, RazorDirectiveTokenType type)
        {
            return GetValue(directive, type, skip: 0);
        }

        public static string GetValue(this IRazorDirective directive, RazorDirectiveTokenType type, int skip)
        {
            for (var i = 0; i < directive.Tokens.Count; i++)
            {
                var token = directive.Tokens[i];

                if (token.Descriptor.Type == type)
                {
                    if (skip-- == 0)
                    {
                        return token.Value;
                    }
                }
            }

            return null;
        }
    }
}
