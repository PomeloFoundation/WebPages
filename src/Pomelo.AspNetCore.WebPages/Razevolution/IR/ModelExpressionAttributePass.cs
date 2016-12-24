// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Pomelo.AspNetCore.WebPages.Razevolution.IR
{
    public class ModelExpressionAttributePass : ICSharpSourceTreePass
    {
        private readonly GeneratedTagHelperAttributeContext _attributeLiterals;

        public RazorEngine Engine { get; set; }

        public int Order { get; } = 1;

        public ModelExpressionAttributePass(GeneratedTagHelperAttributeContext attributeLiterals)
        {
            _attributeLiterals = attributeLiterals;
        }

        public CSharpSourceTree Execute(RazorCodeDocument document, CSharpSourceTree sourceTree)
        {
            Walk(sourceTree);

            return sourceTree;
        }

        private void Walk(ICSharpSource source)
        {
            var csharpBlock = source as CSharpBlock;
            if (csharpBlock != null)
            {
                for (var i = 0; i < csharpBlock.Children.Count; i++)
                {
                    Walk(csharpBlock.Children[i]);
                }
            }
            else if (source is SetTagHelperProperty)
            {
                var setProperty = (SetTagHelperProperty)source;
                if (!string.Equals(setProperty.AssociatedDescriptor.TypeName, _attributeLiterals.ModelExpressionTypeName, StringComparison.Ordinal))
                {
                    return;
                }

                var csharpPrefix = new StringBuilder();
                csharpPrefix
                    .Append(_attributeLiterals.ModelExpressionProviderPropertyName)
                    .Append(".")
                    .Append(_attributeLiterals.CreateModelExpressionMethodName)
                    .Append("(")
                    .Append(_attributeLiterals.ViewDataPropertyName)
                    .Append(", __model => ");

                if (setProperty.Value.Children.Count == 1 && setProperty.Value.Children.First() is RenderHtml)
                {
                    // Simple attribute value
                    csharpPrefix.Append("__model.");
                }

                // TODO: Code smell, this is mutating the tree. Should we be replacing entirely?
                var csharpPrefixElement = new CSharpSource { Code = csharpPrefix.ToString() };
                setProperty.Value.Children.Insert(0, csharpPrefixElement);

                var csharpSuffixElement = new CSharpSource { Code = ")" };
                setProperty.Value.Children.Add(csharpSuffixElement);
            }
        }
    }
}
