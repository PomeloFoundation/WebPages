// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Pomelo.AspNetCore.WebPages.Razevolution.IR;

namespace Pomelo.AspNetCore.WebPages.Razevolution
{
    public interface ICSharpSourceLoweringFeature : IRazorEngineFeature
    {
        CSharpSourceTree Execute(RazorCodeDocument document, RazorChunkTree chunkTree);
    }
}