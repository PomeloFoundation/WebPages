﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Pomelo.AspNetCore.WebPages.Razevolution.Directives;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Microsoft.AspNetCore.Razor.Parser;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Pomelo.AspNetCore.WebPages.Compilation
{
    public class PageRazorEngineHost : RazorEngineHost
    {
        private ITagHelperDescriptorResolver _tagHelperDescriptorResolver;

        private static readonly string[] _defaultNamespaces = new[]
        {
            "System",
            "System.IO",
            "System.Linq",
            "System.Text.RegularExpressions",
            "System.Threading.Tasks",
            "System.Collections.Generic",
            "Microsoft.AspNetCore.Mvc",
            "Microsoft.AspNetCore.Mvc.Rendering",
            "Microsoft.AspNetCore.Mvc.ViewFeatures",
            "Microsoft.Extensions.Logging",
            "Pomelo.AspNetCore.WebPages.Data"
        };
        public PageRazorEngineHost()
            : base(new CSharpRazorCodeLanguage())
        {
            DefaultBaseClass = typeof(Page).FullName;

            GeneratedClassContext = new GeneratedClassContext(
                executeMethodName: "RenderAsync",
                writeMethodName: "Write",
                writeLiteralMethodName: "WriteLiteral",
                writeToMethodName: "WriteTo",
                writeLiteralToMethodName: "WriteLiteralTo",
                templateTypeName: "Microsoft.AspNetCore.Mvc.Razor.HelperResult",
                defineSectionMethodName: "DefineSection",
                generatedTagHelperContext: new GeneratedTagHelperContext
                {
                    ExecutionContextTypeName = typeof(TagHelperExecutionContext).FullName,
                    ExecutionContextAddMethodName = nameof(TagHelperExecutionContext.Add),
                    ExecutionContextAddTagHelperAttributeMethodName =
                        nameof(TagHelperExecutionContext.AddTagHelperAttribute),
                    ExecutionContextAddHtmlAttributeMethodName = nameof(TagHelperExecutionContext.AddHtmlAttribute),
                    ExecutionContextOutputPropertyName = nameof(TagHelperExecutionContext.Output),

                    RunnerTypeName = typeof(TagHelperRunner).FullName,
                    RunnerRunAsyncMethodName = nameof(TagHelperRunner.RunAsync),

                    ScopeManagerTypeName = typeof(TagHelperScopeManager).FullName,
                    ScopeManagerBeginMethodName = nameof(TagHelperScopeManager.Begin),
                    ScopeManagerEndMethodName = nameof(TagHelperScopeManager.End),

                    TagHelperContentTypeName = typeof(TagHelperContent).FullName,

                    // Can't use nameof because RazorPage is not accessible here.
                    CreateTagHelperMethodName = "CreateTagHelper",
                    FormatInvalidIndexerAssignmentMethodName = "InvalidTagHelperIndexerAssignment",
                    StartTagHelperWritingScopeMethodName = "StartTagHelperWritingScope",
                    EndTagHelperWritingScopeMethodName = "EndTagHelperWritingScope",
                    BeginWriteTagHelperAttributeMethodName = "BeginWriteTagHelperAttribute",
                    EndWriteTagHelperAttributeMethodName = "EndWriteTagHelperAttribute",

                    // Can't use nameof because IHtmlHelper is (also) not accessible here.
                    MarkAsHtmlEncodedMethodName = "Html.Raw",
                    BeginAddHtmlAttributeValuesMethodName = "BeginAddHtmlAttributeValues",
                    EndAddHtmlAttributeValuesMethodName = "EndAddHtmlAttributeValues",
                    AddHtmlAttributeValueMethodName = "AddHtmlAttributeValue",
                    HtmlEncoderPropertyName = "HtmlEncoder",
                    TagHelperContentGetContentMethodName = nameof(TagHelperContent.GetContent),
                    TagHelperOutputIsContentModifiedPropertyName = nameof(TagHelperOutput.IsContentModified),
                    TagHelperOutputContentPropertyName = nameof(TagHelperOutput.Content),
                    ExecutionContextSetOutputContentAsyncMethodName = nameof(TagHelperExecutionContext.SetOutputContentAsync),
                    TagHelperAttributeValuePropertyName = nameof(TagHelperAttribute.Value),
                })
            {
                BeginContextMethodName = "BeginContext",
                EndContextMethodName = "EndContext"
            };

            foreach (var x in _defaultNamespaces)
            {
                NamespaceImports.Add(x);
            }
        }

        public override ITagHelperDescriptorResolver TagHelperDescriptorResolver
        {
            get
            {
                // The initialization of the _tagHelperDescriptorResolver needs to be lazy to allow for the setting
                // of DesignTimeMode.
                if (_tagHelperDescriptorResolver == null)
                {
                    _tagHelperDescriptorResolver = new TagHelperDescriptorResolver(DesignTimeMode);
                }

                return _tagHelperDescriptorResolver;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _tagHelperDescriptorResolver = value;
            }
        }

        public override ParserBase DecorateCodeParser(ParserBase incomingCodeParser)
        {
            return new PageCodeParser(additionalDirectives: Enumerable.Empty<RazorDirectiveDescriptor>());
        }

        public override CodeGenerator DecorateCodeGenerator(CodeGenerator incomingBuilder, CodeGeneratorContext context)
        {
            return new PageCodeGenerator(context);
        }
    }
}
