// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Pomelo.AspNetCore.WebPages.Compilation.Rewriters
{
    public class CallBaseConstructorRewriter : CSharpSyntaxRewriter
    {
        private readonly INamedTypeSymbol _class;
        private readonly INamedTypeSymbol _base;

        public CallBaseConstructorRewriter(INamedTypeSymbol @class, INamedTypeSymbol @base)
        {
            _class = @class;
            _base = @base;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (node.Identifier.Text != _class.Name)
            {
                // Could be a nested class, anyway it's not a match.
                return node;
            }

            if (_base.Constructors.Length > 1)
            {
                throw new InvalidOperationException("Base class can have at most one constructor.");
            }

            var constructor = _base.Constructors[0];

            var parameters = new ParameterSyntax[constructor.Parameters.Length];
            var arguments = new ArgumentSyntax[constructor.Parameters.Length];

            for (var i = 0; i < constructor.Parameters.Length; i++)
            {
                var typeFullName = constructor.Parameters[i].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                var parameterName = "param" + i;

                parameters[i] = Parameter(
                    attributeLists: List<AttributeListSyntax>(),
                    modifiers: TokenList(),
                    type: ParseTypeName(typeFullName),
                    identifier: Identifier(parameterName),
                    @default: null);

                arguments[i] = Argument(IdentifierName(parameterName));
            }

            return node.AddMembers(
                ConstructorDeclaration(
                    attributeLists: List<AttributeListSyntax>(),
                    modifiers: TokenList(Token(SyntaxKind.PublicKeyword)),
                    identifier: Identifier(_class.Name),
                    parameterList: ParameterList(SeparatedList(parameters)),
                    initializer: ConstructorInitializer(
                        SyntaxKind.BaseConstructorInitializer,
                        ArgumentList(SeparatedList(arguments))),
                    body: Block()));
        }
    }
}
