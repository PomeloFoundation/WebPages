// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Pomelo.AspNetCore.WebPages.Razevolution.Directives;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.Chunks;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Microsoft.AspNetCore.Razor.Parser.SyntaxTree;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Pomelo.AspNetCore.WebPages.Razevolution.IR
{
    public class ExecuteMethodBodyVisitor : ChunkVisitor
    {
        private readonly CSharpSourceLoweringContext _context;
        private readonly RazevolutionPaddingBuilder _paddingBuilder;

        public ExecuteMethodBodyVisitor(CSharpSourceLoweringContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _context = context;
            _paddingBuilder = new RazevolutionPaddingBuilder(context.Host);
        }

        protected override void Visit(RazorDirectiveTokenChunk chunk)
        {
            var filePath = chunk.Start.FilePath ?? _context.SourceFileName;
            var fileMappedLocation = new SourceLocation(filePath, chunk.Start.AbsoluteIndex, chunk.Start.LineIndex, chunk.Start.CharacterIndex);
            var documentLocation = new MappingLocation(fileMappedLocation, chunk.Value.Length);
            var token = new RazorDirectiveToken()
            {
                Descriptor = chunk.Descriptor,
                Value = chunk.Value,
                DocumentLocation = documentLocation
            };

            _context.Builder.Add(token);
        }

        protected override void Visit(RazorDirectiveChunk chunk)
        {
            var tokensBlock = new CSharpBlock();
            using (_context.Builder.UseBlock(tokensBlock))
            {
                Accept(chunk.Children);
            }

            IRazorDirective directive;
            var tokens = tokensBlock.Children.OfType<RazorDirectiveToken>().ToList();
            if (chunk.Descriptor.Type == RazorDirectiveDescriptorType.SingleLine)
            {
                directive = new RazorSingleLineDirective()
                {
                    Name = chunk.Name,
                    Tokens = tokens,
                };
            }
            else
            {
                var directiveChildren = tokensBlock.Children.Except(tokens);
                var directiveBlock = new RazorBlockDirective()
                {
                    Name = chunk.Name,
                    Tokens = tokens,
                };

                directiveBlock.Children.AddRange(directiveChildren);
                directive = directiveBlock;
            }

            _context.Builder.Add(directive);
        }

        protected override void Visit(ModelChunk chunk)
        {
            // TODO: Remove this ModelChunk handling point, make the model directive more generic.

            var modelTokens = new List<RazorDirectiveToken>()
            {
                new RazorDirectiveToken
                {
                    Descriptor = new RazorDirectiveTokenDescriptor { Type = RazorDirectiveTokenType.Type },
                    Value = chunk.ModelType,
                }
            };
            var modelDirective = new RazorSingleLineDirective()
            {
                Name = "model",
                Tokens = modelTokens,
            };
            _context.Builder.Add(modelDirective);
        }

        protected override void Visit(InjectChunk chunk)
        {
            // TODO: Remove this InjectChunk handling point, make the inject directive more generic.
            var injectTokens = new List<RazorDirectiveToken>
            {
                new RazorDirectiveToken
                {
                    Descriptor = new RazorDirectiveTokenDescriptor { Type = RazorDirectiveTokenType.Type },
                    Value = chunk.TypeName,
                },
                new RazorDirectiveToken
                {
                    Descriptor = new RazorDirectiveTokenDescriptor { Type = RazorDirectiveTokenType.Member },
                    Value = chunk.MemberName,
                }
            };

            var injectDirective = new RazorSingleLineDirective()
            {
                Name = "inject",
                Tokens = injectTokens,
            };
            _context.Builder.Add(injectDirective);
        }

        protected override void Visit(TagHelperChunk chunk)
        {
            using (_context.Builder.BuildBlock<RenderTagHelper>(renderTagHelper =>
            {
                renderTagHelper.DocumentLocation = CreateMappingLocation(chunk.Start, chunk.Association.Length);
            }))
            {
                AddTagHelperStructure(chunk.TagName, chunk.TagMode, chunk.Children);

                var descriptors = chunk.Descriptors.Distinct(TypeBasedTagHelperDescriptorComparer.Default);
                AddTagHelperCreation(descriptors);

                AddTagHelperAttributes(chunk.Attributes, descriptors);

                AddExecuteTagHelpers();
            }
        }

        protected override void Visit(ParentLiteralChunk chunk)
        {
            var text = chunk.GetText();

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var renderHtml = new RenderHtml
            {
                Html = text,
                DocumentLocation = CreateMappingLocation(chunk.Start, chunk.Children.Sum(child => child.Association?.Length ?? 0)),
            };
            _context.Builder.Add(renderHtml);
        }

        protected override void Visit(LiteralChunk chunk)
        {
            if (string.IsNullOrEmpty(chunk.Text))
            {
                return;
            }

            var renderHtml = new RenderHtml
            {
                Html = chunk.Text,
                DocumentLocation = CreateMappingLocation(chunk.Start, chunk.Association.Length),
            };
            _context.Builder.Add(renderHtml);
        }

        protected override void Visit(ExpressionBlockChunk chunk)
        {
            var firstChildExpression = chunk.Children.FirstOrDefault() as ExpressionChunk;
            var padding = 0;
            MappingLocation documentLocation = null;
            if (firstChildExpression != null)
            {
                padding = _paddingBuilder.CalculateExpressionPadding((Span)firstChildExpression.Association);
                documentLocation = CreateMappingLocation(firstChildExpression.Start, firstChildExpression.Code.Length);
            }

            var expressionBlock = new CSharpBlock();
            using (_context.Builder.UseBlock(expressionBlock))
            {
                Accept(chunk.Children);
            }

            var renderExpression = new RenderExpression
            {
                Expression = expressionBlock,
                Padding = padding,
                DocumentLocation = documentLocation
            };

            _context.Builder.Add(renderExpression);
        }

        protected override void Visit(ExpressionChunk chunk)
        {
            var expressionPiece = new CSharpSource
            {
                Code = chunk.Code,
            };
            _context.Builder.Add(expressionPiece);
        }

        protected override void Visit(StatementChunk chunk)
        {
            var documentLocation = CreateMappingLocation(chunk.Start, chunk.Code.Length);
            var padding = _paddingBuilder.CalculateStatementPadding((Span)chunk.Association);
            var statement = new RenderStatement
            {
                DocumentLocation = documentLocation,
                Code = chunk.Code,
                Padding = padding,
            };
            _context.Builder.Add(statement);
        }

        protected override void Visit(DynamicCodeAttributeChunk chunk)
        {
            var value = new CSharpBlock();
            using (_context.Builder.UseBlock(value))
            {
                Accept(chunk.Children);
            }

            var attributePiece = new ConditionalAttributePiece
            {
                DocumentLocation = CreateMappingLocation(chunk.Start, chunk.Association.Length),
                Prefix = chunk.Prefix,
                Value = value
            };
            _context.Builder.Add(attributePiece);
        }

        protected override void Visit(LiteralCodeAttributeChunk chunk)
        {
            if (chunk.Value == null)
            {
                // TODO: This is a hack, this should really be handled at the chunk generation level
                var dynamicCodeAttributeChunk = new DynamicCodeAttributeChunk
                {
                    Children = chunk.Children,
                    Association = chunk.Association,
                    Prefix = chunk.Prefix,
                    Start = chunk.Start
                };

                Visit(dynamicCodeAttributeChunk);

                return;
            }

            var attributePiece = new LiteralAttributePiece
            {
                DocumentLocation = CreateMappingLocation(chunk.Start, chunk.Association.Length),
                Prefix = chunk.Prefix,
                Value = chunk.Value,
            };

            _context.Builder.Add(attributePiece);
        }

        protected override void Visit(CodeAttributeChunk chunk)
        {
            var valueBlock = new CSharpBlock();
            using (_context.Builder.UseBlock(valueBlock))
            {
                Accept(chunk.Children);
            }

            var attribute = new RenderConditionalAttribute
            {
                DocumentLocation = CreateMappingLocation(chunk.Start, chunk.Association.Length),
                Name = chunk.Attribute,
                Prefix = chunk.Prefix,
                ValuePieces = valueBlock.Children,
                Suffix = chunk.Suffix,
            };
            _context.Builder.Add(attribute);
        }

        protected override void Visit(SectionChunk chunk)
        {
            using (_context.Builder.BuildBlock<RenderSection>(sectionBlock =>
            {
                sectionBlock.Name = chunk.Name;
            }))
            {
                Accept(chunk.Children);
            }
        }

        protected override void Visit(TemplateChunk chunk)
        {
            using (_context.Builder.BuildBlock<Template>())
            {
                Accept(chunk.Children);
            };
        }

        private void AddExecuteTagHelpers()
        {
            var executeTagHelpers = new ExecuteTagHelpers();
            _context.Builder.Add(executeTagHelpers);

            // TODO: Should we pass enough information to not render the outputiscontentmodified if statement?
        }

        private void AddTagHelperAttributes(IList<TagHelperAttributeTracker> attributes, IEnumerable<TagHelperDescriptor> descriptors)
        {
            var renderedBoundAttributeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var attribute in attributes)
            {
                var attributeValueChunk = attribute.Value;
                var associatedDescriptors = descriptors.Where(descriptor =>
                    descriptor.Attributes.Any(attributeDescriptor => attributeDescriptor.IsNameMatch(attribute.Name)));

                if (associatedDescriptors.Any() && renderedBoundAttributeNames.Add(attribute.Name))
                {
                    if (attributeValueChunk == null)
                    {
                        // Minimized attributes are not valid for bound attributes. TagHelperBlockRewriter has already
                        // logged an error if it was a bound attribute; so we can skip.
                        continue;
                    }

                    foreach (var associatedDescriptor in associatedDescriptors)
                    {
                        var valueBlock = new CSharpBlock();
                        using (_context.Builder.UseBlock(valueBlock))
                        {
                            Accept(attribute.Value);
                        }

                        var associatedAttributeDescriptor = associatedDescriptor.Attributes.First(
                            attributeDescriptor => attributeDescriptor.IsNameMatch(attribute.Name));
                        var associationBlock = attribute.Value.Association as Block;
                        var documentLocation = attribute.Value.Start; // TODO: Do we need to handle @ at the beginning of non-string TH attributes?
                        var contentLength = attribute.Value.Association.Length;
                        var setTagHelperProperty = new SetTagHelperProperty
                        {
                            PropertyName = associatedAttributeDescriptor.PropertyName,
                            AttributeName = attribute.Name,
                            TagHelperTypeName = associatedDescriptor.TypeName,
                            Value = valueBlock,
                            DocumentLocation = CreateMappingLocation(documentLocation, contentLength),
                            AssociatedDescriptor = associatedAttributeDescriptor,
                            ValueStyle = attribute.ValueStyle
                        };
                        _context.Builder.Add(setTagHelperProperty);
                    }
                }
                else
                {
                    var addHtmlAttribute = new AddTagHelperHtmlAttribute
                    {
                        Name = attribute.Name,
                        ValueStyle = attribute.ValueStyle
                    };

                    if (attributeValueChunk != null)
                    {
                        var valueBlock = new CSharpBlock();
                        using (_context.Builder.UseBlock(valueBlock))
                        {
                            Accept(attribute.Value);
                        }
                        addHtmlAttribute.ValuePieces = valueBlock.Children;
                    }

                    _context.Builder.Add(addHtmlAttribute);
                }
            }
        }

        private void AddTagHelperCreation(IEnumerable<TagHelperDescriptor> descriptors)
        {
            foreach (var descriptor in descriptors)
            {
                var creatTagHelper = new CreateTagHelper
                {
                    TagHelperTypeName = descriptor.TypeName,
                    AssociatedDescriptor = descriptor,
                };
                _context.Builder.Add(creatTagHelper);
            }
        }

        private void AddTagHelperStructure(string tagName, TagMode tagMode, IList<Chunk> children)
        {
            using (_context.Builder.BuildBlock<InitializeTagHelperStructure>(
                            declaration =>
                            {
                                declaration.TagName = tagName;
                                declaration.TagMode = tagMode;
                            }))
            {
                Accept(children);
            }
        }


        private MappingLocation CreateMappingLocation(SourceLocation location, int contentLength)
        {
            // TODO: Code smell, should refactor mapping location to take a file path
            var filePath = location.FilePath ?? _context.SourceFileName;
            var fileMappedLocation = new SourceLocation(filePath, location.AbsoluteIndex, location.LineIndex, location.CharacterIndex);

            return new MappingLocation(fileMappedLocation, contentLength);
        }
    }
}
