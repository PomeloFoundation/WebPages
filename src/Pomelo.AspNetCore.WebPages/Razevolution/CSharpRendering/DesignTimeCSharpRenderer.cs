// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using Pomelo.AspNetCore.WebPages.Razevolution.Directives;
using Pomelo.AspNetCore.WebPages.Razevolution.IR;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;

namespace Pomelo.AspNetCore.WebPages.Razevolution.CSharpRendering
{
    public class DesignTimeCSharpRenderer : ICSharpRenderer
    {
        private const string DesignTimeVariable = "__o";
        private const string ActionHelper = "__actionHelper";

        private readonly RazevolutionPaddingBuilder _paddingBuilder;

        public RazorEngine Engine { get; set; }

        public int Order { get; }

        public DesignTimeCSharpRenderer(RazorEngineHost host)
        {
            _paddingBuilder = new RazevolutionPaddingBuilder(host);
        }

        public bool TryRender(ICSharpSource source, CSharpRenderingContext context)
        {
            // TODO: Need to handle generic directives and render appropriate design time code to color them.

            if (source is CSharpSource)
            {
                Render((CSharpSource)source, context);
            }
            else if (source is RenderExpression)
            {
                Render((RenderExpression)source, context);
            }
            else if (source is RenderTagHelper)
            {
                Render((RenderTagHelper)source, context);
            }
            else if (source is InitializeTagHelperStructure)
            {
                Render((InitializeTagHelperStructure)source, context);
            }
            else if (source is CreateTagHelper)
            {
                Render((CreateTagHelper)source, context);
            }
            else if (source is SetTagHelperProperty)
            {
                Render((SetTagHelperProperty)source, context);
            }
            else if (source is ImportNamespace)
            {
                Render((ImportNamespace)source, context);
            }
            else if (source is RenderConditionalAttribute)
            {
                Render((RenderConditionalAttribute)source, context);
            }
            else if (source is ConditionalAttributePiece)
            {
                Render((ConditionalAttributePiece)source, context);
            }
            else if (source is RenderSection)
            {
                Render((RenderSection)source, context);
            }
            else if (source is RenderStatement)
            {
                Render((RenderStatement)source, context);
            }
            else if (source is ExecuteMethodDeclaration)
            {
                Render((ExecuteMethodDeclaration)source, context);

                // We can't fully render the execute method declaration. Let another CSharpRenderer do it.
                return false;
            }
            else if (source is DeclareTagHelperFields)
            {
                Render((DeclareTagHelperFields)source, context);
            }
            else if (source is Template)
            {
                Render((Template)source, context);
            }
            else
            {
                return false;
            }

            return true;
        }

        private void Render(IRazorDirective source, CSharpRenderingContext context)
        {
            const string TypeHelper = "__typeHelper";

            for (var i = 0; i < source.Tokens.Count; i++)
            {
                var token = source.Tokens[i];
                var tokenType = token.Descriptor.Type;

                if (token.DocumentLocation == null ||
                    (tokenType != RazorDirectiveTokenType.Type &&
                    tokenType != RazorDirectiveTokenType.Member &&
                    tokenType != RazorDirectiveTokenType.String))
                {
                    continue;
                }

                // Wrap the directive token in a lambda to isolate variable names.
                context.Writer.WriteStartAssignment(ActionHelper);
                using (context.Writer.BuildLambda(endLine: true))
                {
                    switch (tokenType)
                    {
                        case RazorDirectiveTokenType.Type:
                            using (context.Writer.BuildCodeMapping(token.DocumentLocation))
                            {
                                context.Writer.Write(token.Value);
                            }

                            context.Writer
                                .Write(" ")
                                .WriteStartAssignment(TypeHelper)
                                .WriteLine("null;");
                            break;
                        case RazorDirectiveTokenType.Member:
                            context.Writer
                                .Write(typeof(object).FullName)
                                .Write(" ");

                            using (context.Writer.BuildCodeMapping(token.DocumentLocation))
                            {
                                context.Writer.Write(token.Value);
                            }
                            context.Writer.WriteLine(" = null;");
                            break;
                        case RazorDirectiveTokenType.String:
                            context.Writer
                                .Write(typeof(object).FullName)
                                .Write(" ")
                                .WriteStartAssignment(TypeHelper);

                            if (token.Value.StartsWith("\"", StringComparison.Ordinal))
                            {
                                using (context.Writer.BuildCodeMapping(token.DocumentLocation))
                                {
                                    context.Writer.Write(token.Value);
                                }
                            }
                            else
                            {
                                context.Writer.Write("\"");
                                using (context.Writer.BuildCodeMapping(token.DocumentLocation))
                                {
                                    context.Writer.Write(token.Value);
                                }
                                context.Writer.Write("\"");
                            }

                            context.Writer.WriteLine(";");
                            break;
                    }
                }
            }
        }

        private void Render(CSharpSource source, CSharpRenderingContext context)
        {
            context.Writer.Write(source.Code);
        }

        private void Render(RenderExpression source, CSharpRenderingContext context)
        {
            if (source.Expression.Children.Count == 0)
            {
                return;
            }

            if (source.DocumentLocation != null)
            {
                using (context.Writer.BuildLinePragma(source.DocumentLocation))
                using (context.Writer.NoIndent())
                {
                    var paddingString = BuildAssignmentOffsetPadding(source.Padding);

                    context.Writer
                        .Write(paddingString)
                        .WriteStartAssignment(DesignTimeVariable);

                    using (context.Writer.BuildCodeMapping(source.DocumentLocation))
                    {
                        context.Render(source.Expression.Children);
                    }

                    context.Writer.WriteLine(";");
                }
            }
            else
            {
                context.Writer.WriteStartAssignment(DesignTimeVariable);
                context.Render(source.Expression.Children);
                context.Writer.WriteLine(";");
            }
        }

        private void Render(ImportNamespace source, CSharpRenderingContext context)
        {
            context.Writer.WriteUsing(source.Namespace);
        }

        private void Render(RenderConditionalAttribute source, CSharpRenderingContext context)
        {
            context.Render(source.ValuePieces);
        }

        private void Render(ConditionalAttributePiece source, CSharpRenderingContext context)
        {
            context.Render(source.Value.Children);
        }

        private void Render(RenderSection source, CSharpRenderingContext context)
        {
            const string SectionWriterName = "__razor_section_writer";

            context.Writer
                .WriteStartMethodInvocation(context.CodeLiterals.DefineSectionMethodName)
                .WriteStringLiteral(source.Name)
                .WriteParameterSeparator();

            var redirectConventions = new CSharpRedirectRenderingConventions(SectionWriterName, context);
            using (context.UseRenderingConventions(redirectConventions))
            using (context.Writer.BuildAsyncLambda(endLine: false, parameterNames: SectionWriterName))
            {
                context.Render(source.Children);
            }

            context.Writer.WriteEndMethodInvocation();
        }

        private void Render(RenderStatement source, CSharpRenderingContext context)
        {
            Debug.Assert(source.Code != null);

            if (source.DocumentLocation != null)
            {
                using (context.Writer.BuildLinePragma(source.DocumentLocation))
                using (context.Writer.NoIndent())
                {
                    var paddingString = _paddingBuilder.BuildPaddingString(source.Padding);

                    context.Writer.Write(paddingString);

                    using (context.Writer.BuildCodeMapping(source.DocumentLocation))
                    {
                        context.Writer.Write(source.Code);
                    }
                }
            }
            else
            {
                context.Writer.WriteLine(source.Code);
            }
        }

        private void Render(ExecuteMethodDeclaration source, CSharpRenderingContext context)
        {
            const string DesignTimeHelperMethodName = "__RazorDesignTimeHelpers__";
            const int DisableVariableNamingWarnings = 219;

            context.Writer
                .Write("private static object @")
                .Write(DesignTimeVariable)
                .WriteLine(";");

            using (context.Writer.BuildMethodDeclaration("private", "void", "@" + DesignTimeHelperMethodName))
            {
                using (context.Writer.BuildDisableWarningScope(DisableVariableNamingWarnings))
                {
                    context.Writer.WriteVariableDeclaration(typeof(Action).FullName, ActionHelper, value: null);

                    var directives = context.GetDirectives();
                    foreach (var directive in directives)
                    {
                        Render(directive, context);
                    }
                }
            }
        }

        private void Render(Template source, CSharpRenderingContext context)
        {
            const string ItemParameterName = "item";
            const string TemplateWriterName = "__razor_template_writer";

            context.Writer
                .Write(ItemParameterName).Write(" => ")
                .WriteStartNewObject(context.CodeLiterals.TemplateTypeName);

            var redirectConventions = new CSharpRedirectRenderingConventions(TemplateWriterName, context);
            using (context.UseRenderingConventions(redirectConventions))
            using (context.Writer.BuildAsyncLambda(endLine: false, parameterNames: TemplateWriterName))
            {
                context.Render(source.Children);
            }

            context.Writer.WriteEndMethodInvocation();
        }

        private void Render(ViewClassDeclaration source, CSharpRenderingContext context)
        {
            context.Writer
                .Write(source.Accessor)
                .Write(" class ")
                .Write(source.Name);

            if (source.BaseTypeName != null || source.ImplementedInterfaceNames != null)
            {
                context.Writer.Write(" : ");
            }

            if (source.BaseTypeName != null)
            {
                context.Writer.Write(source.BaseTypeName);

                if (source.ImplementedInterfaceNames != null)
                {
                    context.Writer.WriteParameterSeparator();
                }
            }

            if (source.ImplementedInterfaceNames != null)
            {
                for (var i = 0; i < source.ImplementedInterfaceNames.Count; i++)
                {
                    context.Writer.Write(source.ImplementedInterfaceNames[i]);

                    if (i + 1 < source.ImplementedInterfaceNames.Count)
                    {
                        context.Writer.WriteParameterSeparator();
                    }
                }
            }

            context.Writer.WriteLine();

            using (context.Writer.BuildScope())
            {
                context.Writer
                    .Write("private static object @")
                    .Write(DesignTimeVariable)
                    .WriteLine(";");

                context.Render(source.Children);
            }
        }

        private void Render(RenderTagHelper source, CSharpRenderingContext context)
        {
            var renderTagHelperContext = new RenderTagHelperContext();
            using (context.UseRenderTagHelperContext(renderTagHelperContext))
            {
                context.Render(source.Children);
            }
        }

        private void Render(InitializeTagHelperStructure source, CSharpRenderingContext context)
        {
            context.Render(source.Children);
        }

        private void Render(CreateTagHelper source, CSharpRenderingContext context)
        {
            var tagHelperVariableName = GetTagHelperVariableName(source.TagHelperTypeName);

            // Create the tag helper
            context.Writer
                .WriteStartAssignment(tagHelperVariableName)
                .WriteStartMethodInvocation(
                    context.CodeLiterals.GeneratedTagHelperContext.CreateTagHelperMethodName,
                    "global::" + source.TagHelperTypeName)
                .WriteEndMethodInvocation();
        }

        private void Render(SetTagHelperProperty source, CSharpRenderingContext context)
        {
            var tagHelperVariableName = GetTagHelperVariableName(source.TagHelperTypeName);
            var renderTagHelperContext = context.GetRenderTagHelperContext();
            var propertyValueAccessor = GetTagHelperPropertyAccessor(tagHelperVariableName, source.AttributeName, source.AssociatedDescriptor);

            string previousValueAccessor;
            if (renderTagHelperContext.RenderedBoundAttributes.TryGetValue(source.AttributeName, out previousValueAccessor))
            {
                context.Writer
                    .WriteStartAssignment(propertyValueAccessor)
                    .Write(previousValueAccessor)
                    .WriteLine(";");

                return;
            }
            else
            {
                renderTagHelperContext.RenderedBoundAttributes[source.AttributeName] = propertyValueAccessor;
            }

            if (source.AssociatedDescriptor.IsStringProperty)
            {
                context.Render(source.Value.Children);
            }
            else
            {
                var firstMappedChild = source.Value.Children.FirstOrDefault(child => child is ISourceMapped) as ISourceMapped;
                var valueStart = firstMappedChild?.DocumentLocation;

                using (context.Writer.BuildLinePragma(source.DocumentLocation))
                using (context.Writer.NoIndent())
                {
                    var assignmentPrefixLength = propertyValueAccessor.Length + " = ".Length;
                    if (source.AssociatedDescriptor.IsEnum &&
                        source.Value.Children.Count == 1 &&
                        source.Value.Children.First() is RenderHtml)
                    {
                        assignmentPrefixLength += $"global::{source.AssociatedDescriptor.TypeName}.".Length;

                        if (valueStart != null)
                        {
                            var padding = Math.Max(valueStart.CharacterIndex - assignmentPrefixLength, 0);
                            var paddingString = _paddingBuilder.BuildPaddingString(padding);

                            context.Writer.Write(paddingString);
                        }

                        context.Writer
                            .WriteStartAssignment(propertyValueAccessor)
                            .Write("global::")
                            .Write(source.AssociatedDescriptor.TypeName)
                            .Write(".");
                    }
                    else
                    {
                        if (valueStart != null)
                        {
                            var padding = Math.Max(valueStart.CharacterIndex - assignmentPrefixLength, 0);
                            var paddingString = _paddingBuilder.BuildPaddingString(padding);

                            context.Writer.Write(paddingString);
                        }

                        context.Writer.WriteStartAssignment(propertyValueAccessor);
                    }

                    RenderTagHelperAttributeInline(source.Value, source.DocumentLocation, context);

                    context.Writer.WriteLine(";");
                }
            }
        }

        private void Render(DeclareTagHelperFields source, CSharpRenderingContext context)
        {
            foreach (var tagHelperTypeName in source.UsedTagHelperTypeNames)
            {
                var tagHelperVariableName = GetTagHelperVariableName(tagHelperTypeName);
                context.Writer
                    .Write("private global::")
                    .WriteVariableDeclaration(
                        tagHelperTypeName,
                        tagHelperVariableName,
                        value: null);
            }
        }

        private string BuildAssignmentOffsetPadding(int padding)
        {
            var offsetPadding = Math.Max(padding - DesignTimeVariable.Length - " = ".Length, 0);
            var paddingString = _paddingBuilder.BuildPaddingString(offsetPadding);

            return paddingString;
        }

        private void RenderTagHelperAttributeInline(
            ICSharpSource attributeValue,
            MappingLocation documentLocation,
            CSharpRenderingContext context)
        {
            if (attributeValue is CSharpSource)
            {
                var source = (CSharpSource)attributeValue;
                if (source.DocumentLocation != null)
                {
                    using (context.Writer.BuildCodeMapping(source.DocumentLocation))
                    {
                        context.Writer.Write(source.Code);
                    }
                }
                else
                {
                    context.Writer.Write(source.Code);
                }
            }
            else if (attributeValue is RenderHtml)
            {
                var source = (RenderHtml)attributeValue;
                if (source.DocumentLocation != null)
                {
                    using (context.Writer.BuildCodeMapping(source.DocumentLocation))
                    {
                        context.Writer.Write(source.Html);
                    }
                }
                else
                {
                    context.Writer.Write(source.Html);
                }
            }
            else if (attributeValue is RenderExpression)
            {
                var source = (RenderExpression)attributeValue;
                using (context.Writer.BuildCodeMapping(source.DocumentLocation))
                {
                    RenderTagHelperAttributeInline(((RenderExpression)attributeValue).Expression, documentLocation, context);
                }
            }
            else if (attributeValue is RenderStatement)
            {
                context.ErrorSink.OnError(
                    documentLocation,
                    "TODO: RazorResources.TagHelpers_CodeBlocks_NotSupported_InAttributes");
            }
            else if (attributeValue is Template)
            {
                context.ErrorSink.OnError(
                    documentLocation,
                    "TODO: RazorResources.FormatTagHelpers_InlineMarkupBlocks_NotSupported_InAttributes(_attributeTypeName)");
            }
            else if (attributeValue is CSharpBlock)
            {
                var expressionBlock = (CSharpBlock)attributeValue;
                for (var i = 0; i < expressionBlock.Children.Count; i++)
                {
                    RenderTagHelperAttributeInline(expressionBlock.Children[i], documentLocation, context);
                }
            }
        }

        private static string GetTagHelperPropertyAccessor(
            string tagHelperVariableName,
            string attributeName,
            TagHelperAttributeDescriptor associatedDescriptor)
        {
            var propertyAccessor = $"{tagHelperVariableName}.{associatedDescriptor.PropertyName}";

            if (associatedDescriptor.IsIndexer)
            {
                var dictionaryKey = attributeName.Substring(associatedDescriptor.Name.Length);
                propertyAccessor += $"[\"{dictionaryKey}\"]";
            }

            return propertyAccessor;
        }

        private static string GetTagHelperVariableName(string tagHelperTypeName) => "__" + tagHelperTypeName.Replace('.', '_');
    }
}