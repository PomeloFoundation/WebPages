// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Chunks;

namespace Pomelo.AspNetCore.WebPages.Razevolution
{
    public abstract class RazorChunkTree
    {
        public abstract ChunkTree Root { get; }
    }
}
