// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor;

namespace Pomelo.AspNetCore.WebPages.Razevolution
{
    public abstract class RazorCodeDocument
    {
        public abstract ErrorSink ErrorSink { get; }

        public abstract ItemCollection Items { get; }

        public abstract RazorSourceDocument Source { get; }

        public abstract class ItemCollection
        {
            public abstract object this[object key] { get; set; }
        }
    }
}
