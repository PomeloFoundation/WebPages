// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.CodeGenerators;

namespace Pomelo.AspNetCore.WebPages.Razevolution.CSharpRendering
{
    public class CSharpRenderingConventions
    {
        public CSharpRenderingConventions(CSharpRenderingContext context)
        {
            Writer = context.Writer;

            CodeLiterals = context.CodeLiterals;
        }

        protected CSharpCodeWriter Writer { get; }

        protected GeneratedClassContext CodeLiterals { get; }

        public virtual CSharpCodeWriter StartWriteMethod() => Writer.WriteStartMethodInvocation(CodeLiterals.WriteMethodName);

        public virtual CSharpCodeWriter StartWriteLiteralMethod() => Writer.WriteStartMethodInvocation(CodeLiterals.WriteLiteralMethodName);

        public virtual CSharpCodeWriter StartBeginWriteAttributeMethod() => Writer.WriteStartMethodInvocation(CodeLiterals.BeginWriteAttributeMethodName);

        public virtual CSharpCodeWriter StartWriteAttributeValueMethod() => Writer.WriteStartMethodInvocation(CodeLiterals.WriteAttributeValueMethodName);

        public virtual CSharpCodeWriter StartEndWriteAttributeMethod() => Writer.WriteStartMethodInvocation(CodeLiterals.EndWriteAttributeMethodName);
    }
}
