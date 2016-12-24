// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Pomelo.AspNetCore.WebPages.Razevolution.IR;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Pomelo.AspNetCore.WebPages.Razevolution.CSharpRendering
{
    public class RuntimeCSharpRenderer : ICSharpRenderer
    {
        private const string ExecutionContextVariableName = "__tagHelperExecutionContext";
        private const string StringValueBufferVariableName = "__tagHelperStringValueBuffer";
        private const string ScopeManagerVariableName = "__tagHelperScopeManager";
        private const string RunnerVariableName = "__tagHelperRunner";

        public RazorEngine Engine { get; set; }

        public int Order { get; }

        public bool TryRender(ICSharpSource source, CSharpRenderingContext context)
        {
            if (source is Checksum)
            {
                Render((Checksum)source, context);
            }
            else if (source is CSharpSource)
            {
                Render((CSharpSource)source, context);
            }
            else if (source is RenderHtml)
            {
                Render((RenderHtml)source, context);
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
            else if (source is AddPreallocatedTagHelperHtmlAttribute)
            {
                Render((AddPreallocatedTagHelperHtmlAttribute)source, context);
            }
            else if (source is AddTagHelperHtmlAttribute)
            {
                Render((AddTagHelperHtmlAttribute)source, context);
            }
            else if (source is SetPreallocatedTagHelperProperty)
            {
                Render((SetPreallocatedTagHelperProperty)source, context);
            }
            else if (source is SetTagHelperProperty)
            {
                Render((SetTagHelperProperty)source, context);
            }
            else if (source is ExecuteTagHelpers)
            {
                Render((ExecuteTagHelpers)source, context);
            }
            else if (source is DeclarePreallocatedTagHelperHtmlAttribute)
            {
                Render((DeclarePreallocatedTagHelperHtmlAttribute)source, context);
            }
            else if (source is DeclarePreallocatedTagHelperAttribute)
            {
                Render((DeclarePreallocatedTagHelperAttribute)source, context);
            }
            else if (source is BeginInstrumentation)
            {
                Render((BeginInstrumentation)source, context);
            }
            else if (source is EndInstrumentation)
            {
                Render((EndInstrumentation)source, context);
            }
            else if (source is ImportNamespace)
            {
                Render((ImportNamespace)source, context);
            }
            else if (source is RenderConditionalAttribute)
            {
                Render((RenderConditionalAttribute)source, context);
            }
            else if (source is LiteralAttributePiece)
            {
                Render((LiteralAttributePiece)source, context);
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
            else if (source is Template)
            {
                Render((Template)source, context);
            }
            else if (source is DeclareTagHelperFields)
            {
                Render((DeclareTagHelperFields)source, context);
            }
            else
            {
                return false;
            }

            return true;
        }

        private static void Render(Checksum source, CSharpRenderingContext context)
        {
            if (!string.IsNullOrEmpty(source.Bytes))
            {
                context.Writer
                .Write("#pragma checksum \"")
                .Write(source.FileName)
                .Write("\" \"")
                .Write(source.Guid)
                .Write("\" \"")
                .Write(source.Bytes)
                .WriteLine("\"");
            }
        }

        private static void Render(CSharpSource source, CSharpRenderingContext context)
        {
            context.Writer.Write(source.Code);
        }

        private static void Render(RenderHtml source, CSharpRenderingContext context)
        {
            const int MaxStringLiteralLength = 1024;

            var charactersConsumed = 0;
            var renderingConventions = context.GetRenderingConventions();

            // Render the string in pieces to avoid Roslyn OOM exceptions at compile time: https://github.com/aspnet/External/issues/54
            while (charactersConsumed < source.Html.Length)
            {
                string textToRender;
                if (source.Html.Length <= MaxStringLiteralLength)
                {
                    textToRender = source.Html;
                }
                else
                {
                    var charactersToSubstring = Math.Min(MaxStringLiteralLength, source.Html.Length - charactersConsumed);
                    textToRender = source.Html.Substring(charactersConsumed, charactersToSubstring);
                }

                renderingConventions
                    .StartWriteLiteralMethod()
                    .WriteStringLiteral(textToRender)
                    .WriteEndMethodInvocation();

                charactersConsumed += textToRender.Length;
            }
        }

        private static void Render(RenderExpression source, CSharpRenderingContext context)
        {
            IDisposable linePragmaScope = null;
            if (source.DocumentLocation != null)
            {
                linePragmaScope = context.Writer.BuildLinePragma(source.DocumentLocation);
            }

            var renderingConventions = context.GetRenderingConventions();
            renderingConventions.StartWriteMethod();
            context.Render(source.Expression.Children);
            context.Writer.WriteEndMethodInvocation();

            linePragmaScope?.Dispose();
        }

        private static void Render(BeginInstrumentation source, CSharpRenderingContext context)
        {
            context.Writer
                .WriteStartMethodInvocation(context.CodeLiterals.BeginContextMethodName)
                .Write(source.DocumentLocation.AbsoluteIndex.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator()
                .Write(source.DocumentLocation.ContentLength.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator()
                .Write(source.Literal ? "true" : "false")
                .WriteEndMethodInvocation();
        }

        private static void Render(EndInstrumentation source, CSharpRenderingContext context)
        {
            context.Writer.WriteMethodInvocation(context.CodeLiterals.EndContextMethodName);
        }

        private static void Render(ImportNamespace source, CSharpRenderingContext context)
        {
            context.Writer.WriteUsing(source.Namespace);
        }

        private static void Render(RenderConditionalAttribute source, CSharpRenderingContext context)
        {
            var valuePieceCount = source.ValuePieces.Count(piece => piece is LiteralAttributePiece || piece is ConditionalAttributePiece);
            var prefixLocation = source.DocumentLocation.AbsoluteIndex;
            var suffixLocation = source.DocumentLocation.AbsoluteIndex + source.DocumentLocation.ContentLength - source.Suffix.Length;
            var renderingConventions = context.GetRenderingConventions();
            renderingConventions
                .StartBeginWriteAttributeMethod()
                .WriteStringLiteral(source.Name)
                .WriteParameterSeparator()
                .WriteStringLiteral(source.Prefix)
                .WriteParameterSeparator()
                .Write(prefixLocation.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator()
                .WriteStringLiteral(source.Suffix)
                .WriteParameterSeparator()
                .Write(suffixLocation.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator()
                .Write(valuePieceCount.ToString(CultureInfo.InvariantCulture))
                .WriteEndMethodInvocation();

            context.Render(source.ValuePieces);

            renderingConventions
                .StartEndWriteAttributeMethod()
                .WriteEndMethodInvocation();
        }

        private static void Render(LiteralAttributePiece source, CSharpRenderingContext context)
        {
            var prefixLocation = source.DocumentLocation.AbsoluteIndex;
            var valueLocation = source.DocumentLocation.AbsoluteIndex + source.Prefix.Length;
            var valueLength = source.DocumentLocation.ContentLength - source.Prefix.Length;
            var renderingConventions = context.GetRenderingConventions();
            renderingConventions
                .StartWriteAttributeValueMethod()
                .WriteStringLiteral(source.Prefix)
                .WriteParameterSeparator()
                .Write(prefixLocation.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator()
                .WriteStringLiteral(source.Value)
                .WriteParameterSeparator()
                .Write(valueLocation.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator()
                .Write(valueLength.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator()
                .WriteBooleanLiteral(true)
                .WriteEndMethodInvocation();
        }

        private static void Render(ConditionalAttributePiece source, CSharpRenderingContext context)
        {
            const string ValueWriterName = "__razor_attribute_value_writer";

            var expressionValue = source.Value.Children.First() as RenderExpression;
            var linePragma = expressionValue != null ? context.Writer.BuildLinePragma(source.DocumentLocation) : null;
            var prefixLocation = source.DocumentLocation.AbsoluteIndex;
            var valueLocation = source.DocumentLocation.AbsoluteIndex + source.Prefix.Length;
            var valueLength = source.DocumentLocation.ContentLength - source.Prefix.Length;
            var renderingConventions = context.GetRenderingConventions();
            renderingConventions
                .StartWriteAttributeValueMethod()
                .WriteStringLiteral(source.Prefix)
                .WriteParameterSeparator()
                .Write(prefixLocation.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator();

            if (expressionValue != null)
            {
                Debug.Assert(source.Value.Children.Count == 1);

                RenderExpressionInline(expressionValue.Expression, context);
            }
            else
            {
                // Not an expression; need to buffer the result.
                context.Writer.WriteStartNewObject(context.CodeLiterals.TemplateTypeName);

                var redirectConventions = new CSharpRedirectRenderingConventions(ValueWriterName, context);
                using (context.UseRenderingConventions(redirectConventions))
                using (context.Writer.BuildAsyncLambda(endLine: false, parameterNames: ValueWriterName))
                {
                    context.Render(source.Value.Children);
                }

                context.Writer.WriteEndMethodInvocation(false);
            }

            context.Writer
                .WriteParameterSeparator()
                .Write(valueLocation.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator()
                .Write(valueLength.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator()
                .WriteBooleanLiteral(false)
                .WriteEndMethodInvocation();

            linePragma?.Dispose();
        }

        private static void Render(RenderSection source, CSharpRenderingContext context)
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

        private static void Render(RenderStatement source, CSharpRenderingContext context)
        {
            if (string.IsNullOrWhiteSpace(source.Code))
            {
                return;
            }

            if (source.DocumentLocation != null)
            {
                using (context.Writer.BuildLinePragma(source.DocumentLocation))
                {
                    context.Writer.WriteLine(source.Code);
                }
            }
            else
            {
                context.Writer.WriteLine(source.Code);
            }
        }

        private static void Render(Template source, CSharpRenderingContext context)
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

        private static void Render(DeclarePreallocatedTagHelperHtmlAttribute source, CSharpRenderingContext context)
        {
            context.Writer
                .Write("private static readonly global::")
                .Write(context.CodeLiterals.GeneratedTagHelperContext.TagHelperAttributeTypeName)
                .Write(" ")
                .Write(source.VariableName)
                .Write(" = ")
                .WriteStartNewObject("global::" + context.CodeLiterals.GeneratedTagHelperContext.TagHelperAttributeTypeName)
                .WriteStringLiteral(source.Name);

            if (source.ValueStyle == HtmlAttributeValueStyle.Minimized)
            {
                context.Writer.WriteEndMethodInvocation();
            }
            else
            {
                context.Writer
                    .WriteParameterSeparator()
                    .WriteStartNewObject("global::" + context.CodeLiterals.GeneratedTagHelperContext.EncodedHtmlStringTypeName)
                    .WriteStringLiteral(source.Value)
                    .WriteEndMethodInvocation(endLine: false)
                    .WriteParameterSeparator()
                    .Write($"global::{typeof(HtmlAttributeValueStyle).FullName}.{source.ValueStyle}")
                    .WriteEndMethodInvocation();
            }
        }

        private static void Render(DeclarePreallocatedTagHelperAttribute source, CSharpRenderingContext context)
        {
            context.Writer
                .Write("private static readonly global::")
                .Write(context.CodeLiterals.GeneratedTagHelperContext.TagHelperAttributeTypeName)
                .Write(" ")
                .Write(source.VariableName)
                .Write(" = ")
                .WriteStartNewObject("global::" + context.CodeLiterals.GeneratedTagHelperContext.TagHelperAttributeTypeName)
                .WriteStringLiteral(source.Name)
                .WriteParameterSeparator()
                .WriteStringLiteral(source.Value)
                .WriteParameterSeparator()
                .Write($"global::{typeof(HtmlAttributeValueStyle).FullName}.{source.ValueStyle}")
                .WriteEndMethodInvocation();
        }

        private static void Render(DeclareTagHelperFields source, CSharpRenderingContext context)
        {
            var tagHelperCodeLiterals = context.CodeLiterals.GeneratedTagHelperContext;
            context.Writer.WriteLineHiddenDirective();

            // Need to disable the warning "X is assigned to but never used." for the value buffer since
            // whether it's used depends on how a TagHelper is used.
            context.Writer
                .WritePragma("warning disable 0414")
                .Write("private ")
                .WriteVariableDeclaration("string", StringValueBufferVariableName, value: null)
                .WritePragma("warning restore 0414");

            context.Writer
                .Write("private global::")
                .WriteVariableDeclaration(
                    tagHelperCodeLiterals.ExecutionContextTypeName,
                    ExecutionContextVariableName,
                    value: null);

            context.Writer
                .Write("private global::")
                .Write(tagHelperCodeLiterals.RunnerTypeName)
                .Write(" ")
                .Write(RunnerVariableName)
                .Write(" = new global::")
                .Write(tagHelperCodeLiterals.RunnerTypeName)
                .WriteLine("();");

            const string backedScopeManageVariableName = "__backed" + ScopeManagerVariableName;
            context.Writer
                .Write("private global::")
                .WriteVariableDeclaration(
                    tagHelperCodeLiterals.ScopeManagerTypeName,
                    backedScopeManageVariableName,
                    value: null);

            context.Writer
                .Write("private global::")
                .Write(tagHelperCodeLiterals.ScopeManagerTypeName)
                .Write(" ")
                .WriteLine(ScopeManagerVariableName);

            using (context.Writer.BuildScope())
            {
                context.Writer.WriteLine("get");
                using (context.Writer.BuildScope())
                {
                    context.Writer
                        .Write("if (")
                        .Write(backedScopeManageVariableName)
                        .WriteLine(" == null)");

                    using (context.Writer.BuildScope())
                    {
                        context.Writer
                            .WriteStartAssignment(backedScopeManageVariableName)
                            .WriteStartNewObject(tagHelperCodeLiterals.ScopeManagerTypeName)
                            .Write(tagHelperCodeLiterals.StartTagHelperWritingScopeMethodName)
                            .WriteParameterSeparator()
                            .Write(tagHelperCodeLiterals.EndTagHelperWritingScopeMethodName)
                            .WriteEndMethodInvocation();
                    }

                    context.Writer.WriteReturn(backedScopeManageVariableName);
                }
            }

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

        private static void Render(RenderTagHelper source, CSharpRenderingContext context)
        {
            var renderTagHelperContext = new RenderTagHelperContext();
            using (context.UseRenderTagHelperContext(renderTagHelperContext))
            {
                context.Render(source.Children);
            }
        }

        private void Render(InitializeTagHelperStructure source, CSharpRenderingContext context)
        {
            // Call into the tag helper scope manager to start a new tag helper scope.
            // Also capture the value as the current execution context.
            context.Writer
                .WriteStartAssignment(ExecutionContextVariableName)
                .WriteStartInstanceMethodInvocation(
                    ScopeManagerVariableName,
                    context.CodeLiterals.GeneratedTagHelperContext.ScopeManagerBeginMethodName);

            // Assign a unique ID for this instance of the source HTML tag. This must be unique
            // per call site, e.g. if the tag is on the view twice, there should be two IDs.
            context.Writer.WriteStringLiteral(source.TagName)
                   .WriteParameterSeparator()
                   .Write("global::")
                   .Write(typeof(TagMode).FullName)
                   .Write(".")
                   .Write(source.TagMode.ToString())
                   .WriteParameterSeparator()
                   .WriteStringLiteral(GenerateUniqueTagHelperId())
                   .WriteParameterSeparator();

            // We remove and redirect writers so TagHelper authors can retrieve content.
            var nonRedirectedConventions = new CSharpRenderingConventions(context);
            using (context.UseRenderingConventions(nonRedirectedConventions))
            using (context.Writer.BuildAsyncLambda(endLine: false))
            {
                context.Render(source.Children);
            }

            context.Writer.WriteEndMethodInvocation();
        }

        private static void Render(CreateTagHelper source, CSharpRenderingContext context)
        {
            var tagHelperVariableName = GetTagHelperVariableName(source.TagHelperTypeName);

            // Create the tag helper
            context.Writer
                .WriteStartAssignment(tagHelperVariableName)
                .WriteStartMethodInvocation(
                    context.CodeLiterals.GeneratedTagHelperContext.CreateTagHelperMethodName,
                    "global::" + source.TagHelperTypeName)
                .WriteEndMethodInvocation();

            context.Writer.WriteInstanceMethodInvocation(
                ExecutionContextVariableName,
                context.CodeLiterals.GeneratedTagHelperContext.ExecutionContextAddMethodName,
                tagHelperVariableName);
        }

        private static void Render(AddPreallocatedTagHelperHtmlAttribute source, CSharpRenderingContext context)
        {
            context.Writer
                .WriteStartInstanceMethodInvocation(
                    ExecutionContextVariableName,
                    context.CodeLiterals.GeneratedTagHelperContext.ExecutionContextAddHtmlAttributeMethodName)
                .Write(source.AttributeVariableName)
                .WriteEndMethodInvocation();
        }

        private static void Render(AddTagHelperHtmlAttribute source, CSharpRenderingContext context)
        {
            var attributeValueStyleParameter = $"global::{typeof(HtmlAttributeValueStyle).FullName}.{source.ValueStyle}";
            var isConditionalAttributeValue = source.ValuePieces.Any(child => child is ConditionalAttributePiece);

            // All simple text and minimized attributes will be pre-allocated.
            if (isConditionalAttributeValue)
            {
                // Dynamic attribute value should be run through the conditional attribute removal system. It's
                // unbound and contains C#.

                // TagHelper attribute rendering is buffered by default. We do not want to write to the current
                // writer.
                var valuePieceCount = source.ValuePieces.Count(piece => piece is LiteralAttributePiece || piece is ConditionalAttributePiece);

                context.Writer
                    .WriteStartMethodInvocation(context.CodeLiterals.GeneratedTagHelperContext.BeginAddHtmlAttributeValuesMethodName)
                    .Write(ExecutionContextVariableName)
                    .WriteParameterSeparator()
                    .WriteStringLiteral(source.Name)
                    .WriteParameterSeparator()
                    .Write(valuePieceCount.ToString(CultureInfo.InvariantCulture))
                    .WriteParameterSeparator()
                    .Write(attributeValueStyleParameter)
                    .WriteEndMethodInvocation();

                var renderingConventions = new TagHelperHtmlAttributeRenderingConventions(context);
                using (context.UseRenderingConventions(renderingConventions))
                {
                    context.Render(source.ValuePieces);
                }

                context.Writer
                    .WriteMethodInvocation(
                        context.CodeLiterals.GeneratedTagHelperContext.EndAddHtmlAttributeValuesMethodName,
                        ExecutionContextVariableName);
            }
            else
            {
                // This is a data-* attribute which includes C#. Do not perform the conditional attribute removal or
                // other special cases used when IsDynamicAttributeValue(). But the attribute must still be buffered to
                // determine its final value.

                // Attribute value is not plain text, must be buffered to determine its final value.
                context.Writer.WriteMethodInvocation(context.CodeLiterals.GeneratedTagHelperContext.BeginWriteTagHelperAttributeMethodName);

                // We're building a writing scope around the provided chunks which captures everything written from the
                // page. Therefore, we do not want to write to any other buffer since we're using the pages buffer to
                // ensure we capture all content that's written, directly or indirectly.
                var nonRedirectedConventions = new CSharpRenderingConventions(context);
                using (context.UseRenderingConventions(nonRedirectedConventions))
                {
                    context.Render(source.ValuePieces);
                }

                context.Writer
                    .WriteStartAssignment(StringValueBufferVariableName)
                    .WriteMethodInvocation(context.CodeLiterals.GeneratedTagHelperContext.EndWriteTagHelperAttributeMethodName)
                    .WriteStartInstanceMethodInvocation(
                        ExecutionContextVariableName,
                        context.CodeLiterals.GeneratedTagHelperContext.ExecutionContextAddHtmlAttributeMethodName)
                    .WriteStringLiteral(source.Name)
                    .WriteParameterSeparator()
                    .WriteStartMethodInvocation(context.CodeLiterals.GeneratedTagHelperContext.MarkAsHtmlEncodedMethodName)
                    .Write(StringValueBufferVariableName)
                    .WriteEndMethodInvocation(endLine: false)
                    .WriteParameterSeparator()
                    .Write(attributeValueStyleParameter)
                    .WriteEndMethodInvocation();
            }
        }

        private static void Render(SetPreallocatedTagHelperProperty source, CSharpRenderingContext context)
        {
            var tagHelperVariableName = GetTagHelperVariableName(source.TagHelperTypeName);
            var propertyValueAccessor = GetTagHelperPropertyAccessor(tagHelperVariableName, source.AttributeName, source.AssociatedDescriptor);
            var attributeValueAccessor = $"{source.AttributeVariableName}.{context.CodeLiterals.GeneratedTagHelperContext.TagHelperAttributeValuePropertyName}";
            context.Writer
                .WriteStartAssignment(propertyValueAccessor)
                .Write("(string)")
                .Write(attributeValueAccessor)
                .WriteLine(";")
                .WriteStartInstanceMethodInvocation(
                    ExecutionContextVariableName,
                    context.CodeLiterals.GeneratedTagHelperContext.ExecutionContextAddTagHelperAttributeMethodName)
                .Write(source.AttributeVariableName)
                .WriteEndMethodInvocation();
        }

        private static void Render(SetTagHelperProperty source, CSharpRenderingContext context)
        {
            var tagHelperVariableName = GetTagHelperVariableName(source.TagHelperTypeName);
            var renderTagHelperContext = context.GetRenderTagHelperContext();

            // Ensure that the property we're trying to set has initialized its dictionary bound properties.
            if (source.AssociatedDescriptor.IsIndexer &&
                renderTagHelperContext.VerifiedPropertyDictionaries.Add(source.AssociatedDescriptor.PropertyName))
            {
                // Throw a reasonable Exception at runtime if the dictionary property is null.
                context.Writer
                    .Write("if (")
                    .Write(tagHelperVariableName)
                    .Write(".")
                    .Write(source.AssociatedDescriptor.PropertyName)
                    .WriteLine(" == null)");
                using (context.Writer.BuildScope())
                {
                    // System is in Host.NamespaceImports for all MVC scenarios. No need to generate FullName
                    // of InvalidOperationException type.
                    context.Writer
                        .Write("throw ")
                        .WriteStartNewObject(nameof(InvalidOperationException))
                        .WriteStartMethodInvocation(context.CodeLiterals.GeneratedTagHelperContext.FormatInvalidIndexerAssignmentMethodName)
                        .WriteStringLiteral(source.AttributeName)
                        .WriteParameterSeparator()
                        .WriteStringLiteral(source.TagHelperTypeName)
                        .WriteParameterSeparator()
                        .WriteStringLiteral(source.AssociatedDescriptor.PropertyName)
                        .WriteEndMethodInvocation(endLine: false)   // End of method call
                        .WriteEndMethodInvocation();   // End of new expression / throw statement
                }
            }

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
                context.Writer.WriteMethodInvocation(context.CodeLiterals.GeneratedTagHelperContext.BeginWriteTagHelperAttributeMethodName);

                var renderingConventions = new CSharpLiteralCodeConventions(context);
                using (context.UseRenderingConventions(renderingConventions))
                {
                    context.Render(source.Value.Children);
                }

                context.Writer
                    .WriteStartAssignment(StringValueBufferVariableName)
                    .WriteMethodInvocation(context.CodeLiterals.GeneratedTagHelperContext.EndWriteTagHelperAttributeMethodName)
                    .WriteStartAssignment(propertyValueAccessor)
                    .Write(StringValueBufferVariableName)
                    .WriteLine(";");
            }
            else
            {
                using (context.Writer.BuildLinePragma(source.DocumentLocation))
                {
                    context.Writer.WriteStartAssignment(propertyValueAccessor);

                    if (source.AssociatedDescriptor.IsEnum &&
                        source.Value.Children.Count == 1 &&
                        source.Value.Children.First() is RenderHtml)
                    {
                        context.Writer
                            .Write("global::")
                            .Write(source.AssociatedDescriptor.TypeName)
                            .Write(".");
                    }

                    RenderTagHelperAttributeInline(source.Value, source.DocumentLocation, context);

                    context.Writer.WriteLine(";");
                }
            }

            // We need to inform the context of the attribute value.
            context.Writer
                .WriteStartInstanceMethodInvocation(
                    ExecutionContextVariableName,
                    context.CodeLiterals.GeneratedTagHelperContext.ExecutionContextAddTagHelperAttributeMethodName)
                .WriteStringLiteral(source.AttributeName)
                .WriteParameterSeparator()
                .Write(propertyValueAccessor)
                .WriteParameterSeparator()
                .Write($"global::{typeof(HtmlAttributeValueStyle).FullName}.{source.ValueStyle}")
                .WriteEndMethodInvocation();
        }

        private static void Render(ExecuteTagHelpers source, CSharpRenderingContext context)
        {
            context.Writer
                .Write("await ")
                .WriteStartInstanceMethodInvocation(RunnerVariableName, context.CodeLiterals.GeneratedTagHelperContext.RunnerRunAsyncMethodName)
                .Write(ExecutionContextVariableName)
                .WriteEndMethodInvocation();

            var tagHelperOutputAccessor =
            $"{ExecutionContextVariableName}.{context.CodeLiterals.GeneratedTagHelperContext.ExecutionContextOutputPropertyName}";

            context.Writer
                .Write("if (!")
                .Write(tagHelperOutputAccessor)
                .Write(".")
                .Write(context.CodeLiterals.GeneratedTagHelperContext.TagHelperOutputIsContentModifiedPropertyName)
                .WriteLine(")");

            using (context.Writer.BuildScope())
            {
                context.Writer
                    .Write("await ")
                    .WriteInstanceMethodInvocation(
                        ExecutionContextVariableName,
                        context.CodeLiterals.GeneratedTagHelperContext.ExecutionContextSetOutputContentAsyncMethodName);
            }

            var renderingConventions = context.GetRenderingConventions();
            renderingConventions
                .StartWriteMethod()
                .Write(tagHelperOutputAccessor)
                .WriteEndMethodInvocation()
                .WriteStartAssignment(ExecutionContextVariableName)
                .WriteInstanceMethodInvocation(
                    ScopeManagerVariableName,
                    context.CodeLiterals.GeneratedTagHelperContext.ScopeManagerEndMethodName);
        }

        protected virtual string GenerateUniqueTagHelperId() => Guid.NewGuid().ToString("N");

        private static void RenderTagHelperAttributeInline(
            ICSharpSource attributeValue,
            MappingLocation documentLocation,
            CSharpRenderingContext context)
        {
            if (attributeValue is CSharpSource)
            {
                context.Writer.Write(((CSharpSource)attributeValue).Code);
            }
            else if (attributeValue is RenderHtml)
            {
                context.Writer.Write(((RenderHtml)attributeValue).Html);
            }
            else if (attributeValue is RenderExpression)
            {
                RenderTagHelperAttributeInline(((RenderExpression)attributeValue).Expression, documentLocation, context);
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

        private class CSharpLiteralCodeConventions : CSharpRenderingConventions
        {
            public CSharpLiteralCodeConventions(CSharpRenderingContext context) : base(context)
            {
            }

            public override CSharpCodeWriter StartWriteMethod() => Writer.WriteStartMethodInvocation(CodeLiterals.WriteLiteralMethodName);
        }

        private class TagHelperHtmlAttributeRenderingConventions : CSharpRenderingConventions
        {
            public TagHelperHtmlAttributeRenderingConventions(CSharpRenderingContext context) : base(context)
            {
            }

            public override CSharpCodeWriter StartWriteAttributeValueMethod() =>
                Writer.WriteStartMethodInvocation(CodeLiterals.GeneratedTagHelperContext.AddHtmlAttributeValueMethodName);
        }

        private static void RenderExpressionInline(ICSharpSource expression, CSharpRenderingContext context)
        {
            if (expression is CSharpSource)
            {
                context.Writer.Write(((CSharpSource)expression).Code);
            }
            else if (expression is RenderExpression)
            {
                RenderExpressionInline(((RenderExpression)expression).Expression, context);
            }
            else if (expression is CSharpBlock)
            {
                var expressionBlock = (CSharpBlock)expression;
                for (var i = 0; i < expressionBlock.Children.Count; i++)
                {
                    RenderExpressionInline(expressionBlock.Children[i], context);
                }
            }
        }
    }
}