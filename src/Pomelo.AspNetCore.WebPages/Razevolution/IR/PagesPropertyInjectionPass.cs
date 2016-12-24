// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Pomelo.AspNetCore.WebPages.Razevolution.Directives;

namespace Pomelo.AspNetCore.WebPages.Razevolution.IR
{
    // TODO: Should we encapsulate injected properties as inject chunks?
    public class PagesPropertyInjectionPass : ICSharpSourceTreePass
    {
        public RazorEngine Engine { get; set; }

        public int Order { get; } = -1;

        public CSharpSourceTree Execute(RazorCodeDocument document, CSharpSourceTree sourceTree)
        {
            var modelType = GetDeclaredModelType(sourceTree);
            var classInfo = document.GetClassName();
            if (modelType == null)
            {
                // Insert a model directive into the system so sub-systems can rely on the model being the class.
                var modelTokens = new List<RazorDirectiveToken>()
                {
                    new RazorDirectiveToken
                    {
                        Descriptor = new RazorDirectiveTokenDescriptor { Type = RazorDirectiveTokenType.Type },
                        Value = classInfo.Class,
                    }
                };
                var modelDirective = new RazorSingleLineDirective()
                {
                    Name = "model",
                    Tokens = modelTokens,
                };
                sourceTree.Children.Insert(0, modelDirective);
                modelType = classInfo.Class;
            }

            // Inject properties needed to execute Razor pages.
            var classDeclaration = FindClassDeclaration(sourceTree);

            var viewDataType = $"global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<{modelType}>";
            classDeclaration.Children.Insert(0, new RenderStatement()
            {
                Code = $"public {modelType} Model => ViewData.Model;",
            });

            var viewDataProperty = new RenderStatement
            {
                Code = $"public new {viewDataType} ViewData => ({viewDataType})base.ViewData;"
            };
            classDeclaration.Children.Insert(0, viewDataProperty);

            var injectHtmlHelper = CreateInjectDirective($"Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<{modelType}>", "Html");
            sourceTree.Children.Insert(0, injectHtmlHelper);

            var injectLogger = CreateInjectDirective($"Microsoft.Extensions.Logging.ILogger<{classInfo.Class}>", "Logger");
            sourceTree.Children.Insert(0, injectLogger);

            return sourceTree;
        }

        private static string GetDeclaredModelType(ICSharpSource source)
        {
            var directive = source as IRazorDirective;
            if (string.Equals(directive?.Name, "model", StringComparison.Ordinal))
            {
                var modelType = directive.GetValue(RazorDirectiveTokenType.Type);
                return modelType;
            }

            var csharpBlock = source as CSharpBlock;
            if (csharpBlock != null)
            {
                for (var i = 0; i < csharpBlock.Children.Count; i++)
                {
                    var child = csharpBlock.Children[i];
                    var modelType = GetDeclaredModelType(child);

                    if (modelType != null)
                    {
                        return modelType;
                    }
                }
            }

            return null;
        }

        private static IRazorDirective CreateInjectDirective(string typeName, string memberName)
        {
            var injectTokens = new List<RazorDirectiveToken>
            {
                new RazorDirectiveToken
                {
                    Descriptor = new RazorDirectiveTokenDescriptor { Type = RazorDirectiveTokenType.Type },
                    Value = typeName,
                },
                new RazorDirectiveToken
                {
                    Descriptor = new RazorDirectiveTokenDescriptor { Type = RazorDirectiveTokenType.Member },
                    Value = memberName,
                }
            };

            var injectDirective = new RazorSingleLineDirective
            {
                Name = "inject",
                Tokens = injectTokens,
            };

            return injectDirective;
        }

        private static ViewClassDeclaration FindClassDeclaration(CSharpBlock block)
        {
            if (block is ViewClassDeclaration)
            {
                return (ViewClassDeclaration)block;
            }

            for (var i = 0; i < block.Children.Count; i++)
            {
                var currentBlock = block.Children[i] as CSharpBlock;
                if (currentBlock == null)
                {
                    continue;
                }

                var classDeclaration = FindClassDeclaration(currentBlock);
                if (classDeclaration != null)
                {
                    return classDeclaration;
                }
            }

            return null;
        }
    }
}
