// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Pomelo.AspNetCore.WebPages.Razevolution.Directives;
using Pomelo.AspNetCore.WebPages.Razevolution.IR;

namespace Pomelo.AspNetCore.WebPages.Razevolution
{
    public static class RazorCodeDocumentExtensions
    {
        public static IEnumerable<RazorSourceDocument> GetImportedDocuments(this RazorCodeDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return (IEnumerable<RazorSourceDocument>)document.Items[typeof(IEnumerable<RazorSourceDocument>)] ?? Enumerable.Empty<RazorSourceDocument>();
        }

        public static void SetImportedDocuments(
            this RazorCodeDocument document,
            IEnumerable<RazorSourceDocument> importedDocuments)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (importedDocuments == null)
            {
                throw new ArgumentNullException(nameof(importedDocuments));
            }

            document.Items[typeof(IEnumerable<RazorSourceDocument>)] = importedDocuments;
        }

        public static IEnumerable<RazorDirectiveDescriptor> GetDirectiveDescriptors(this RazorCodeDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return (IEnumerable<RazorDirectiveDescriptor>)document.Items[typeof(IEnumerable<RazorDirectiveDescriptor>)] ?? Enumerable.Empty<RazorDirectiveDescriptor>();
        }

        public static void SetDirectiveDescriptors(
            this RazorCodeDocument document,
            IEnumerable<RazorDirectiveDescriptor> directiveDescriptors)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (directiveDescriptors == null)
            {
                throw new ArgumentNullException(nameof(directiveDescriptors));
            }

            document.Items[typeof(IEnumerable<RazorDirectiveDescriptor>)] = directiveDescriptors;
        }

        public static RazorSyntaxTree GetSyntaxTree(this RazorCodeDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return (RazorSyntaxTree)document.Items[typeof(RazorSyntaxTree)];
        }

        public static void SetSyntaxTree(this RazorCodeDocument document, RazorSyntaxTree syntaxTree)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (syntaxTree == null)
            {
                throw new ArgumentNullException(nameof(syntaxTree));
            }

            document.Items[typeof(RazorSyntaxTree)] = syntaxTree;
        }

        public static void AddVirtualSyntaxTree(this RazorCodeDocument document, RazorSyntaxTree syntaxTree)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (syntaxTree == null)
            {
                throw new ArgumentNullException(nameof(syntaxTree));
            }

            GetVirtualSyntaxTrees(document).Add(syntaxTree);
        }

        public static IList<RazorSyntaxTree> GetVirtualSyntaxTrees(this RazorCodeDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var items = (IList<RazorSyntaxTree>)document.Items[typeof(List<RazorSyntaxTree>)];
            if (items == null)
            {
                items = new List<RazorSyntaxTree>();
                document.Items[typeof(List<RazorSyntaxTree>)] = items;
            }

            return items;
        }

        public static RazorChunkTree GetChunkTree(this RazorCodeDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return (RazorChunkTree)document.Items[typeof(RazorChunkTree)];
        }

        public static void SetChunkTree(this RazorCodeDocument document, RazorChunkTree chunkTree)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (chunkTree == null)
            {
                throw new ArgumentNullException(nameof(chunkTree));
            }

            document.Items[typeof(RazorChunkTree)] = chunkTree;
        }

        public static GeneratedCSharpDocument GetGeneratedCSharpDocument(this RazorCodeDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return (GeneratedCSharpDocument)document.Items[typeof(GeneratedCSharpDocument)];
        }

        public static void SetGeneratedCSharpDocument(this RazorCodeDocument document, GeneratedCSharpDocument code)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }

            document.Items[typeof(GeneratedCSharpDocument)] = code;
        }

        public static IEnumerable<IRazorDirective> GetCSharpRenderingDirectives(this RazorCodeDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return (IEnumerable<IRazorDirective>)document.Items[typeof(IEnumerable<IRazorDirective>)];
        }

        public static void SetCSharpRenderingDirectives(this RazorCodeDocument document, IEnumerable<IRazorDirective> directives)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (directives == null)
            {
                throw new ArgumentNullException(nameof(directives));
            }

            document.Items[typeof(IEnumerable<IRazorDirective>)] = directives;
        }

        public static GeneratedClassInfo GetClassName(this RazorCodeDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return (GeneratedClassInfo)document.Items[typeof(GeneratedClassInfo)];
        }

        public static RazorCodeDocument WithClassName(this RazorCodeDocument document, string @namespace, string @class)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (@namespace == null)
            {
                throw new ArgumentNullException(nameof(@namespace));
            }

            if (@class == null)
            {
                throw new ArgumentNullException(nameof(@class));
            }

            var classInfo = new GeneratedClassInfo()
            {
                Class = @class,
                Namespace = @namespace,
            };

            document.Items[typeof(GeneratedClassInfo)] = classInfo;
            return document;
        }

        public static string GetChecksumBytes(this RazorCodeDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return (string)document.Items[typeof(Checksum)];
        }

        // TODO: This needs to be set somewhere
        public static void SetChecksumBytes(this RazorCodeDocument document, string bytes)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            document.Items[typeof(Checksum)] = bytes;
        }

        public static CSharpSourceTree GetSourceTree(this RazorCodeDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return (CSharpSourceTree)document.Items[typeof(CSharpSourceTree)];
        }

        public static void SetSourceTree(this RazorCodeDocument document, CSharpSourceTree sourceTree)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (sourceTree == null)
            {
                throw new ArgumentNullException(nameof(sourceTree));
            }

            document.Items[typeof(CSharpSourceTree)] = sourceTree;
        }
    }
}
