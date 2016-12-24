// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.CodeGenerators;

namespace Pomelo.AspNetCore.WebPages.Razevolution.CSharpRendering
{
    public class CSharpRedirectRenderingConventions : CSharpRenderingConventions
    {
        private readonly string _redirectWriter;

        public CSharpRedirectRenderingConventions(string redirectWriter, CSharpRenderingContext context) : base(context)
        {
            _redirectWriter = redirectWriter;
        }

        public override CSharpCodeWriter StartWriteMethod()
        {
            return Writer
                .WriteStartMethodInvocation(CodeLiterals.WriteToMethodName)
                .Write(_redirectWriter)
                .WriteParameterSeparator();
        }

        public override CSharpCodeWriter StartWriteLiteralMethod()
        {
            return Writer
                .WriteStartMethodInvocation(CodeLiterals.WriteLiteralToMethodName)
                .Write(_redirectWriter)
                .WriteParameterSeparator();
        }

        public override CSharpCodeWriter StartBeginWriteAttributeMethod()
        {
            return Writer
                .WriteStartMethodInvocation(CodeLiterals.BeginWriteAttributeToMethodName)
                .Write(_redirectWriter)
                .WriteParameterSeparator();
        }

        public override CSharpCodeWriter StartWriteAttributeValueMethod()
        {
            return Writer
                .WriteStartMethodInvocation(CodeLiterals.WriteAttributeValueToMethodName)
                .Write(_redirectWriter)
                .WriteParameterSeparator();
        }

        public override CSharpCodeWriter StartEndWriteAttributeMethod()
        {
            return Writer
                .WriteStartMethodInvocation(CodeLiterals.EndWriteAttributeToMethodName)
                .Write(_redirectWriter)
                .WriteParameterSeparator();
        }
    }
}
