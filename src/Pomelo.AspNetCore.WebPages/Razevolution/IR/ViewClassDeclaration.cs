// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;

namespace Pomelo.AspNetCore.WebPages.Razevolution.IR
{
    [DebuggerDisplay("{Accessor, nq} class {Name, nq} : {BaseTypeName, nq} {ImplementedInterfaceNames != null ? \", \" + string.Join(\", \", ImplementedInterfaceNames) : string.Empty, nq}")]
    public class ViewClassDeclaration : CSharpBlock
    {
        public string Accessor { get; set; }

        public string Name { get; set; }

        public string BaseTypeName { get; set; }

        public IList<string> ImplementedInterfaceNames { get; set; }
    }
}
