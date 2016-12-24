// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Pomelo.AspNetCore.WebPages.Razevolution.Directives;

namespace Pomelo.AspNetCore.WebPages.Razevolution.IR
{
    public class RazorSingleLineDirective : IRazorDirective
    {
        public string Name { get; set; }

        public IList<RazorDirectiveToken> Tokens { get; set; }
    }
}
