// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Pomelo.AspNetCore.WebPages.Razevolution
{
    public interface IRazorEngineEvents
    {
        void OnDocumentCreating(RazorEngine engine, RazorSourceDocument source);

        void OnDocumentCreated(RazorEngine engine, RazorCodeDocument document);

        void OnPhaseExecuting(RazorEngine engine, IRazorEnginePhase phase, RazorCodeDocument document);

        void OnPhaseExecuted(RazorEngine engine, IRazorEnginePhase phase, RazorCodeDocument document);
    }
}
