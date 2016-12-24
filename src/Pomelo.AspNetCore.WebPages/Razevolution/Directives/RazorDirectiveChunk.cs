// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Chunks;

namespace Pomelo.AspNetCore.WebPages.Razevolution.Directives
{
    public class RazorDirectiveChunk : ParentChunk
    {
        public string Name => Descriptor.Name;

        public RazorDirectiveDescriptor Descriptor { get; set; }
    }
}
