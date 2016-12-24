// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Pomelo.AspNetCore.WebPages.Razevolution.Directives;

namespace Pomelo.AspNetCore.WebPages.Razevolution.IR
{
    public interface IRazorDirective : ICSharpSource
    {
        string Name { get; }

        IList<RazorDirectiveToken> Tokens { get; }
    }
}
