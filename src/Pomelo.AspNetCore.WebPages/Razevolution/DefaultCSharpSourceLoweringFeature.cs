// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Pomelo.AspNetCore.WebPages.Razevolution.IR;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.Chunks;

namespace Pomelo.AspNetCore.WebPages.Razevolution
{
    public class DefaultCSharpSourceLoweringFeature : ICSharpSourceLoweringFeature
    {
        // See http://msdn.microsoft.com/en-us/library/system.codedom.codechecksumpragma.checksumalgorithmid.aspx
        private const string Sha1AlgorithmId = "{ff1816ec-aa5e-4d10-87f7-6f4963833460}";
        private readonly RazorEngineHost _host;

        public DefaultCSharpSourceLoweringFeature(RazorEngineHost host)
        {
            _host = host;
        }

        public RazorEngine Engine { get; set; }

        public CSharpSourceTree Execute(RazorCodeDocument document, RazorChunkTree chunkTree)
        {
            var builder = new SourceTreeBuilder();
            var context = new CSharpSourceLoweringContext
            {
                Builder = builder,
                Host = _host,
                SourceFileName = document.Source.Filename
            };

            var checksum = new Checksum
            {
                FileName = document.Source.Filename,
                Guid = Sha1AlgorithmId,
                Bytes = document.GetChecksumBytes()
            };
            builder.Add(checksum);

            var classInfo = document.GetClassName();
            using (builder.BuildBlock<NamespaceDeclaration>(declaration => declaration.Namespace = classInfo.Namespace))
            {
                AddNamespaceImports(chunkTree, context);

                using (builder.BuildBlock<ViewClassDeclaration>(
                    declaration =>
                    {
                        declaration.Accessor = "public";
                        declaration.Name = classInfo.Class;

                        var baseTypeVisitor = new BaseTypeVisitor();
                        baseTypeVisitor.Accept(chunkTree.Root);
                        declaration.BaseTypeName = baseTypeVisitor.BaseTypeName ?? _host.DefaultBaseClass;
                    }))
                {
                    new TagHelperFieldDeclarationVisitor(context).Accept(chunkTree.Root);
                    new FunctionsDirectiveVisitor(context).Accept(chunkTree.Root);

                    using (builder.BuildBlock<ExecuteMethodDeclaration>(
                        declaration =>
                        {
                            declaration.Accessor = "public";
                            declaration.Modifiers = new[] { "override", "async" };
                            declaration.ReturnTypeName = typeof(Task).FullName;
                            declaration.Name = _host.GeneratedClassContext.ExecuteMethodName;
                        }))
                    {
                        new ExecuteMethodBodyVisitor(context).Accept(chunkTree.Root);
                    }
                }
            }

            return builder.Root;
        }

        private void AddNamespaceImports(RazorChunkTree chunkTree, CSharpSourceLoweringContext context)
        {
            var defaultImports = _host.NamespaceImports;

            foreach (var import in defaultImports)
            {
                var importSource = new ImportNamespace
                {
                    Namespace = import
                };
                context.Builder.Add(importSource);
            }

            new UsingDirectiveVisitor(defaultImports, context).Accept(chunkTree.Root);
        }

        private class BaseTypeVisitor : ChunkVisitor
        {
            public string BaseTypeName { get; set; }

            protected override void Visit(SetBaseTypeChunk chunk)
            {
                BaseTypeName = chunk.TypeName;
            }
        }
    }
}