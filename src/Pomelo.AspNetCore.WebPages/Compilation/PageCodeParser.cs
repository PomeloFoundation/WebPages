// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Razor;
using Pomelo.AspNetCore.WebPages.Razevolution.Directives;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.Chunks.Generators;
using Microsoft.AspNetCore.Razor.Editor;
using Microsoft.AspNetCore.Razor.Parser;
using Microsoft.AspNetCore.Razor.Parser.SyntaxTree;
using Microsoft.AspNetCore.Razor.Tokenizer.Symbols;

namespace Pomelo.AspNetCore.WebPages.Compilation
{
    public class PageCodeParser : CSharpCodeParser
    {
        private const string ModelKeyword = "model";
        private const string InjectKeyword = "inject";
        private bool _modelStatementFound;

        public PageCodeParser(IEnumerable<RazorDirectiveDescriptor> additionalDirectives)
        {
            var modelBuilder = new RazorDirectiveBuilder(ModelKeyword);
            modelBuilder
                .AddType()
                .AddLiteral(";", optional: true);
            var modelDescriptor = modelBuilder.Build();

            var injectBuilder = new RazorDirectiveBuilder(InjectKeyword);
            injectBuilder
                .AddType()
                .AddMember()
                .AddLiteral(";", optional: true);
            var injectDescriptor = injectBuilder.Build();

            var newDirectiveDescriptors = new List<RazorDirectiveDescriptor>(additionalDirectives);
            newDirectiveDescriptors.Add(modelDescriptor);
            newDirectiveDescriptors.Add(injectDescriptor);

            foreach (var descriptor in newDirectiveDescriptors)
            {
                MapDirectives(() => HandleDirective(descriptor), descriptor.Name);
            }
        }

        protected void HandleDirective(RazorDirectiveDescriptor descriptor)
        {
            Context.CurrentBlock.Type = BlockType.Directive;
            Context.CurrentBlock.ChunkGenerator = new RazorDirectiveChunkGenerator(descriptor);
            AssertDirective(descriptor.Name);

            AcceptAndMoveNext();
            Output(SpanKind.MetaCode, AcceptedCharacters.None);

            for (var i = 0; i < descriptor.Tokens.Count; i++)
            {
                var tokenDescriptor = descriptor.Tokens[i];
                AcceptWhile(IsSpacingToken(includeNewLines: false, includeComments: true));

                if (tokenDescriptor.Type == RazorDirectiveTokenType.Member || tokenDescriptor.Type == RazorDirectiveTokenType.Type)
                {
                    Output(SpanKind.Code, AcceptedCharacters.WhiteSpace);
                }
                else
                {
                    Output(SpanKind.Markup, AcceptedCharacters.WhiteSpace);
                }

                var outputKind = SpanKind.Markup;
                switch (tokenDescriptor.Type)
                {
                    case RazorDirectiveTokenType.Type:
                        if (!NamespaceOrTypeName())
                        {
                            continue;
                        }

                        outputKind = SpanKind.Code;
                        break;
                    case RazorDirectiveTokenType.Member:
                        if (At(CSharpSymbolType.Identifier))
                        {
                            AcceptAndMoveNext();
                        }
                        else
                        {
                            Context.OnError(
                                CurrentLocation,
                                $"TODO: Directive {descriptor.Name} expects an identifier.",
                                CurrentSymbol.Content.Length);
                            continue;
                        }

                        outputKind = SpanKind.Code;
                        break;
                    case RazorDirectiveTokenType.String:
                        AcceptAndMoveNext();
                        break;
                    case RazorDirectiveTokenType.Literal:
                        if (string.Equals(CurrentSymbol.Content, tokenDescriptor.Value, StringComparison.Ordinal))
                        {
                            AcceptAndMoveNext();
                        }
                        else if (!tokenDescriptor.Optional)
                        {
                            Context.OnError(
                                CurrentLocation,
                                $"TODO: Expected literal '{tokenDescriptor.Value}'.",
                                CurrentSymbol.Content.Length);
                        }
                        break;
                }

                Span.ChunkGenerator = new RazorDirectiveTokenChunkGenerator(tokenDescriptor);
                Output(outputKind, AcceptedCharacters.NonWhiteSpace);
            }

            AcceptWhile(IsSpacingToken(includeNewLines: false, includeComments: true));

            switch (descriptor.Type)
            {
                case RazorDirectiveDescriptorType.SingleLine:
                    if (At(CSharpSymbolType.NewLine))
                    {
                        AcceptAndMoveNext();
                    }
                    else if (!EndOfFile)
                    {
                        Context.OnError(
                            CurrentLocation,
                            $"Unexpected literal following directive {descriptor.Name}.",
                            CurrentSymbol.Content.Length);
                    }
                    break;
                case RazorDirectiveDescriptorType.RazorBlock:
                    ParseDirectiveBlock(descriptor, parseChildren: (startingBraceLocation) =>
                    {
                        // When transitioning to the HTML parser we no longer want to act as if we're in a nested C# state.
                        // For instance, if <div>@hello.</div> is in a nested C# block we don't want the trailing '.' to be handled
                        // as C#; it should be handled as a period because it's wrapped in markup.
                        var wasNested = IsNested;
                        IsNested = false;
                        using (PushSpanConfig())
                        {
                            Context.SwitchActiveParser();
                            Context.MarkupParser.ParseSection(Tuple.Create("{", "}"), caseSensitive: true);
                            Context.SwitchActiveParser();
                        }
                        Initialize(Span);
                        IsNested = wasNested;
                        NextToken();
                    });
                    break;
                case RazorDirectiveDescriptorType.CodeBlock:
                    ParseDirectiveBlock(descriptor, parseChildren: (startingBraceLocation) =>
                    {
                        Balance(BalancingModes.NoErrorOnFailure, CSharpSymbolType.LeftBrace, CSharpSymbolType.RightBrace, startingBraceLocation);
                        Span.ChunkGenerator = new StatementChunkGenerator();
                        Output(SpanKind.Code);
                    });
                    break;
            }
        }

        private void ParseDirectiveBlock(RazorDirectiveDescriptor descriptor, Action<SourceLocation> parseChildren)
        {
            if (EndOfFile)
            {
                Context.OnError(
                    CurrentLocation,
                    $"Unexpected end of file following directive {descriptor.Name}. Expected '{{'",
                    CurrentSymbol.Content.Length);
            }
            else if (!At(CSharpSymbolType.LeftBrace))
            {
                Context.OnError(
                    CurrentLocation,
                    $"Unexpected literal '{CurrentSymbol.Content.Length}' following directive {descriptor.Name}. Expected '{{'",
                    CurrentSymbol.Content.Length);
            }
            else
            {

                var editHandler = new AutoCompleteEditHandler(Language.TokenizeString, autoCompleteAtEndOfSpan: true);
                Span.EditHandler = editHandler;
                var startingBraceLocation = CurrentLocation;
                AcceptAndMoveNext();
                Span.ChunkGenerator = SpanChunkGenerator.Null;
                Output(SpanKind.MetaCode, AcceptedCharacters.None);

                parseChildren(startingBraceLocation);

                Span.ChunkGenerator = SpanChunkGenerator.Null;
                if (!Optional(CSharpSymbolType.RightBrace))
                {
                    editHandler.AutoCompleteString = "}";
                    Context.OnError(
                        startingBraceLocation,
                        // TODO: This is RazorResources.FormatParseError_Expected_EndOfBlock_Before_EOF
                        string.Format(
                            "The {0} block is missing a closing \"{1}\" character.  Make sure you have a matching \"{1}\" character for all the \"{2}\" characters within this block, and that none of the \"{1}\" characters are being interpreted as markup.",
                            descriptor.Name,
                            Language.GetSample(CSharpSymbolType.RightBrace),
                            Language.GetSample(CSharpSymbolType.LeftBrace)),
                        length: 1 /* } */);
                }
                else
                {
                    Span.EditHandler.AcceptedCharacters = AcceptedCharacters.None;
                }
                CompleteBlock(insertMarkerIfNecessary: false, captureWhitespaceToEndOfLine: true);
                Span.ChunkGenerator = SpanChunkGenerator.Null;
                Output(SpanKind.MetaCode, AcceptedCharacters.None);
            }
        }

        protected virtual void InjectDirective()
        {
            // @inject MyApp.MyService MyServicePropertyName
            Context.CurrentBlock.Type = BlockType.Directive;
            AssertDirective(InjectKeyword);

            var start = CurrentLocation;
            AcceptAndMoveNext();
            Output(SpanKind.MetaCode);

            AcceptWhile(IsSpacingToken(includeNewLines: false, includeComments: true));
            Output(SpanKind.Code);

            if (!NamespaceOrTypeName())
            {
                Context.OnError(start, "need a type name", InjectKeyword.Length);

                // On error, recover at the next line
                AcceptUntil(CSharpSymbolType.NewLine);
                return;
            }

            // We parsed the whitespace + type name in the current span, so let's extract the type name.
            // We have to do a GetContent() here because the name is potentially made up of multiple
            // tokens.
            var typeName = Span.GetContent().Value;

            AcceptWhile(IsSpacingToken(includeNewLines: false, includeComments: true));

            if (!At(CSharpSymbolType.Identifier))
            {
                Context.OnError(start, "need a property name", InjectKeyword.Length);

                // On error, recover at the next line
                AcceptUntil(CSharpSymbolType.NewLine);
                return;
            }

            var propertyName = CurrentSymbol.Content;
            AcceptAndMoveNext();

            AcceptWhile(IsSpacingToken(includeNewLines: false, includeComments: true));

            Optional(CSharpSymbolType.Semicolon);

            AcceptWhile(IsSpacingToken(includeNewLines: false, includeComments: true));

            if (At(CSharpSymbolType.NewLine))
            {
                AcceptAndMoveNext();
            }
            else if (EndOfFile)
            {
                // Do nothing
            }
            else
            {
                Context.OnError(start, "need a newline", InjectKeyword.Length);

                // On Error, recover at the next line
                AcceptUntil(CSharpSymbolType.NewLine);
                return;
            }

            Span.ChunkGenerator = new InjectParameterGenerator(typeName, propertyName);

            CompleteBlock();
            Output(SpanKind.Code, AcceptedCharacters.AnyExceptNewline);
        }
    }
}
