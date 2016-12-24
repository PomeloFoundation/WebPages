// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.CodeGenerators;

namespace Pomelo.AspNetCore.WebPages.Razevolution.IR
{
    public class InstrumentationPass : ICSharpSourceTreePass
    {
        private readonly RazorEngineHost _host;

        public RazorEngine Engine { get; set; }

        public int Order { get; }

        public InstrumentationPass(RazorEngineHost host)
        {
            _host = host;
        }

        public CSharpSourceTree Execute(RazorCodeDocument document, CSharpSourceTree sourceTree)
        {
            if (_host.EnableInstrumentation)
            {
                Walk(sourceTree);
            }

            return sourceTree;
        }

        public void Walk(CSharpBlock block)
        {
            for (var i = 0; i < block.Children.Count; i++)
            {
                var literal = false;
                MappingLocation mappingLocation = null;
                var current = block.Children[i];

                if (current is CSharpBlock)
                {
                    Walk((CSharpBlock)current);
                }
                else if (current is RenderHtml)
                {
                    var renderHtml = current as RenderHtml;
                    literal = true;
                    mappingLocation = renderHtml.DocumentLocation;
                }
                else if (current is RenderExpression)
                {
                    var renderExpression = current as RenderExpression;
                    literal = false;
                    mappingLocation = renderExpression.DocumentLocation;
                }
                else if (current is ExecuteTagHelpers)
                {
                    Debug.Assert(block is RenderTagHelper);
                    var renderTagHelper = block as RenderTagHelper;
                    var executeTagHelpers = current as ExecuteTagHelpers;
                    literal = false;
                    mappingLocation = renderTagHelper.DocumentLocation;
                }

                if (mappingLocation != null)
                {
                    var beginInstrumentation = new BeginInstrumentation
                    {
                        DocumentLocation = mappingLocation,
                        Literal = literal,
                    };
                    block.Children.Insert(i, beginInstrumentation);
                    var endInstrumentation = new EndInstrumentation();
                    block.Children.Insert(i + 2, endInstrumentation);
                    i += 2;

                    mappingLocation = null;
                }
            }
        }
    }
}
