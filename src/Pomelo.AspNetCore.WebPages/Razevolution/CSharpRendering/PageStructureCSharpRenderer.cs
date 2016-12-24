// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Pomelo.AspNetCore.WebPages.Razevolution.IR;

namespace Pomelo.AspNetCore.WebPages.Razevolution.CSharpRendering
{
    public class PageStructureCSharpRenderer : ICSharpRenderer
    {
        public RazorEngine Engine { get; set; }

        public int Order { get; } = 2;

        public bool TryRender(ICSharpSource source, CSharpRenderingContext context)
        {
            if (source is NamespaceDeclaration)
            {
                Render((NamespaceDeclaration)source, context);
            }
            else if (source is ExecuteMethodDeclaration)
            {
                Render((ExecuteMethodDeclaration)source, context);
            }
            else if (source is ViewClassDeclaration)
            {
                Render((ViewClassDeclaration)source, context);
            }
            else
            {
                return false;
            }

            return true;
        }

        private void Render(NamespaceDeclaration source, CSharpRenderingContext context)
        {
            context.Writer
                .Write("namespace ")
                .WriteLine(source.Namespace);

            using (context.Writer.BuildScope())
            {
                context.Writer.WriteLineHiddenDirective();
                context.Render(source.Children);
            }
        }

        private void Render(ExecuteMethodDeclaration source, CSharpRenderingContext context)
        {
            context.Writer
                .WriteLine("#pragma warning disable 1998")
                .Write(source.Accessor)
                .Write(" ");

            for (var i = 0; i < source.Modifiers.Count; i++)
            {
                context.Writer.Write(source.Modifiers[i]);

                if (i + 1 < source.Modifiers.Count)
                {
                    context.Writer.Write(" ");
                }
            }

            context.Writer
                .Write(" ")
                .Write(source.ReturnTypeName)
                .Write(" ")
                .Write(source.Name)
                .WriteLine("()");

            using (context.Writer.BuildScope())
            {
                context.Render(source.Children);
            }

            context.Writer.WriteLine("#pragma warning restore 1998");
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
                context.Render(source.Children);
            }
        }
    }
}