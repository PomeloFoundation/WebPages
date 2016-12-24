// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Pomelo.AspNetCore.WebPages.Razevolution.Directives
{
    public class RazorDirectiveDescriptor
    {
        public string Name { get; set; }

        public RazorDirectiveDescriptorType Type { get; set; }

        public IList<RazorDirectiveTokenDescriptor> Tokens { get; set; }
    }
}
