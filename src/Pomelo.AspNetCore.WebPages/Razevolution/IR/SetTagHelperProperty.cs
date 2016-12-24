// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Pomelo.AspNetCore.WebPages.Razevolution.IR
{
    [DebuggerDisplay("{System.Linq.Enumerable.Last(TagHelperTypeName.Split('.')), nq}.{PropertyName, nq} = ...[{(Value as CSharpBlock)?.Children.Count ?? 1, nq}]...;")]
    public class SetTagHelperProperty : ICSharpSource, ISourceMapped
    {
        public string TagHelperTypeName { get; set; }

        public string PropertyName { get; set; }

        public string AttributeName { get; set; }

        public CSharpBlock Value { get; set; }

        public HtmlAttributeValueStyle ValueStyle { get; set; }

        public TagHelperAttributeDescriptor AssociatedDescriptor { get; set; }

        public MappingLocation DocumentLocation { get; set; }
    }
}
