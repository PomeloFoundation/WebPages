// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Pomelo.AspNetCore.WebPages.Razevolution.IR
{
    public class CSharpBlock : ICSharpSource
    {
        public List<ICSharpSource> Children { get; set; } = new List<ICSharpSource>();
    }
}
