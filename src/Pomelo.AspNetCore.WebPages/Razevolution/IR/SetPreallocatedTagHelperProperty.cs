// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;

namespace Pomelo.AspNetCore.WebPages.Razevolution.IR
{
    public class SetPreallocatedTagHelperProperty : ICSharpSource
    {
        public string AttributeVariableName { get; set; }

        public string AttributeName { get; set; }

        public string TagHelperTypeName { get; set; }

        public string PropertyName { get; set; }

        public TagHelperAttributeDescriptor AssociatedDescriptor { get; set; }
    }
}
