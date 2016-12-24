// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.Parser.SyntaxTree;

namespace Pomelo.AspNetCore.WebPages.Razevolution
{
    public class VirtualDocumentSyntaxTreePass : ISyntaxTreePass
    {
        public RazorEngine Engine { get; set; }

        public int Order => 0;

        public RazorSyntaxTree Execute(RazorCodeDocument document, RazorSyntaxTree syntaxTree)
        {
            var trees = GetImportedSyntaxTrees(document);
            if (trees.Count == 0)
            {
                return syntaxTree;
            }

            var errors = new List<RazorError>(syntaxTree.Diagnostics);
            var blockBuilder = new BlockBuilder(syntaxTree.Root);

            for (var i = 0; i < trees.Count; i++)
            {
                var tree = trees[i];

                blockBuilder.Children.Insert(i, tree.Root);

                errors.AddRange(tree.Diagnostics);
                foreach (var error in tree.Diagnostics)
                {
                    document.ErrorSink.OnError(error);
                }
            }

            return RazorSyntaxTree.Create(blockBuilder.Build(), errors);
        }

        private static IList<RazorSyntaxTree> GetImportedSyntaxTrees(RazorCodeDocument document)
        {
            var trees = document.GetVirtualSyntaxTrees();

            if (trees.Count > 0)
            {
                return trees;
            }

            var directiveDescriptors = document.GetDirectiveDescriptors();
            var importedDocuments = document.GetImportedDocuments();
            foreach (var importedDocument in importedDocuments)
            {
                var syntaxTree = RazorParser.Parse(importedDocument, directiveDescriptors);

                foreach (var error in syntaxTree.Diagnostics)
                {
                    document.ErrorSink.OnError(error);
                }

                trees.Add(syntaxTree);
            }

            return trees;
        }
    }
}
