// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Pomelo.AspNetCore.WebPages.Razevolution
{
    public abstract class RazorEngine
    {
        public abstract IReadOnlyList<IRazorEngineFeature> Features { get; }

        public abstract IReadOnlyList<IRazorEnginePhase> Phases { get; }

        public abstract void Process(RazorCodeDocument document);

        public abstract RazorCodeDocument CreateCodeDocument(RazorSourceDocument source);

        public abstract void ExecutePhase(IRazorEnginePhase phase, RazorCodeDocument document);
    }
}
