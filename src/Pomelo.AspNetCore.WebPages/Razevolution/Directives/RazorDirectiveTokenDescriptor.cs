// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


namespace Pomelo.AspNetCore.WebPages.Razevolution.Directives
{
    public class RazorDirectiveTokenDescriptor
    {
        public RazorDirectiveTokenType Type { get; set; }

        public string Value { get; set; }

        public bool Optional { get; set; }
    }
}
