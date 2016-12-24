// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

namespace Pomelo.AspNetCore.WebPages.Razevolution.IR
{
    [DebuggerDisplay("namespace {Namespace, nq}")]
    public class NamespaceDeclaration : CSharpBlock
    {
        public string Namespace { get; set; }
    }
}
