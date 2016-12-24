// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;

namespace Pomelo.AspNetCore.WebPages.Razevolution.IR
{
    public class PreallocatedTagHelperAttributePass : ICSharpSourceTreePass
    {
        public RazorEngine Engine { get; set; }

        public int Order { get; }

        public CSharpSourceTree Execute(RazorCodeDocument document, CSharpSourceTree sourceTree)
        {
            new PreallocatedAttributeWalker().Walk(sourceTree);

            return sourceTree;
        }

        private class PreallocatedAttributeWalker
        {
            private const string PreAllocatedAttributeVariablePrefix = "__tagHelperAttribute_";

            private ViewClassDeclaration _classDeclaration;
            private int _variableCountOffset;

            public void Walk(CSharpBlock block)
            {
                for (var i = 0; i < block.Children.Count; i++)
                {
                    var current = block.Children[i];

                    if (current is ViewClassDeclaration)
                    {
                        _classDeclaration = (ViewClassDeclaration)current;
                        _variableCountOffset = _classDeclaration.Children.Count;
                    }

                    if (current is CSharpBlock && !(current is RenderTagHelper))
                    {
                        Walk((CSharpBlock)current);
                    }
                    else
                    {
                        WalkSource(current);
                    }
                }
            }

            private void WalkSource(ICSharpSource source)
            {
                if (source is RenderTagHelper)
                {
                    var renderTagHelper = (RenderTagHelper)source;
                    for (var i = 0; i < renderTagHelper.Children.Count; i++)
                    {
                        var current = renderTagHelper.Children[i];
                        if (current is AddTagHelperHtmlAttribute)
                        {
                            HandleUnboundAttribute((AddTagHelperHtmlAttribute)current, i, renderTagHelper);
                        }
                        else if (current is SetTagHelperProperty)
                        {
                            HandleBoundAttribute((SetTagHelperProperty)current, i, renderTagHelper);
                        }
                        else if (current is CSharpBlock)
                        {
                            Walk((CSharpBlock)current);
                        }
                    }
                }
            }

            private void HandleUnboundAttribute(AddTagHelperHtmlAttribute attribute, int attributeIndex, RenderTagHelper parent)
            {
                if (attribute.ValuePieces != null && (attribute.ValuePieces.Count != 1 || !(attribute.ValuePieces.First() is RenderHtml)))
                {
                    return;
                }

                var plainTextValue = (attribute.ValuePieces?.First() as RenderHtml)?.Html;
                DeclarePreallocatedTagHelperHtmlAttribute declaration = null;

                for (var i = 0; i < _classDeclaration.Children.Count; i++)
                {
                    var current = _classDeclaration.Children[i];

                    if (current is DeclarePreallocatedTagHelperHtmlAttribute)
                    {
                        var existingDeclaration = (DeclarePreallocatedTagHelperHtmlAttribute)current;

                        if (string.Equals(existingDeclaration.Name, attribute.Name, StringComparison.Ordinal) &&
                            string.Equals(existingDeclaration.Value, plainTextValue, StringComparison.Ordinal) &&
                            existingDeclaration.ValueStyle == attribute.ValueStyle)
                        {
                            declaration = existingDeclaration;
                            break;
                        }
                    }
                }

                if (declaration == null)
                {
                    var variableCount = _classDeclaration.Children.Count - _variableCountOffset;
                    var preAllocatedAttributeVariableName = PreAllocatedAttributeVariablePrefix + variableCount;
                    declaration = new DeclarePreallocatedTagHelperHtmlAttribute
                    {
                        VariableName = preAllocatedAttributeVariableName,
                        Name = attribute.Name,
                        Value = plainTextValue,
                        ValueStyle = attribute.ValueStyle,
                    };
                    _classDeclaration.Children.Insert(0, declaration);
                }

                var addPreAllocatedAttribute = new AddPreallocatedTagHelperHtmlAttribute
                {
                    AttributeVariableName = declaration.VariableName
                };

                parent.Children[attributeIndex] = addPreAllocatedAttribute;
            }

            private void HandleBoundAttribute(SetTagHelperProperty attribute, int attributeIndex, RenderTagHelper parent)
            {
                if (!attribute.AssociatedDescriptor.IsStringProperty ||
                    attribute.Value.Children.Count != 1 ||
                    !(attribute.Value.Children.First() is RenderHtml))
                {
                    return;
                }
                var plainTextValue = (attribute.Value.Children.First() as RenderHtml).Html;

                DeclarePreallocatedTagHelperAttribute declaration = null;

                for (var i = 0; i < _classDeclaration.Children.Count; i++)
                {
                    var current = _classDeclaration.Children[i];

                    if (current is DeclarePreallocatedTagHelperAttribute)
                    {
                        var existingDeclaration = (DeclarePreallocatedTagHelperAttribute)current;

                        if (string.Equals(existingDeclaration.Name, attribute.AttributeName, StringComparison.Ordinal) &&
                            string.Equals(existingDeclaration.Value, plainTextValue, StringComparison.Ordinal) &&
                            existingDeclaration.ValueStyle == attribute.ValueStyle)
                        {
                            declaration = existingDeclaration;
                            break;
                        }
                    }
                }

                if (declaration == null)
                {
                    var variableCount = _classDeclaration.Children.Count - _variableCountOffset;
                    var preAllocatedAttributeVariableName = PreAllocatedAttributeVariablePrefix + variableCount;
                    declaration = new DeclarePreallocatedTagHelperAttribute
                    {
                        VariableName = preAllocatedAttributeVariableName,
                        Name = attribute.AttributeName,
                        Value = plainTextValue,
                        ValueStyle = attribute.ValueStyle,
                    };
                    _classDeclaration.Children.Insert(0, declaration);
                }

                var setPreallocatedProperty = new SetPreallocatedTagHelperProperty
                {
                    AttributeVariableName = declaration.VariableName,
                    AttributeName = attribute.AttributeName,
                    TagHelperTypeName = attribute.TagHelperTypeName,
                    PropertyName = attribute.PropertyName,
                    AssociatedDescriptor = attribute.AssociatedDescriptor,
                };

                parent.Children[attributeIndex] = setPreallocatedProperty;
            }
        }
    }
}
