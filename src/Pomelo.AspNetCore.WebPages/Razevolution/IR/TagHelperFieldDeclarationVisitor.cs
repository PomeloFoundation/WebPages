// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.Chunks;

namespace Pomelo.AspNetCore.WebPages.Razevolution.IR
{
    public class TagHelperFieldDeclarationVisitor : ChunkVisitor
    {
        private readonly CSharpSourceLoweringContext _context;
        private readonly HashSet<string> _usedTagHelpers;
        private bool _foundTagHelpers;

        public TagHelperFieldDeclarationVisitor(CSharpSourceLoweringContext context)
        {
            _context = context;
            _usedTagHelpers = new HashSet<string>(StringComparer.Ordinal);
        }

        public override void Accept(Chunk chunk)
        {
            if (chunk is ParentChunk && !(chunk is TagHelperChunk))
            {
                Accept(((ParentChunk)chunk).Children);
            }

            base.Accept(chunk);
        }

        protected override void Visit(TagHelperChunk chunk)
        {
            if (!_foundTagHelpers)
            {
                _foundTagHelpers = true;
                var declareTagHelperFields = new DeclareTagHelperFields
                {
                    UsedTagHelperTypeNames = _usedTagHelpers
                };

                _context.Builder.Add(declareTagHelperFields);
            }

            foreach (var descriptor in chunk.Descriptors)
            {
                if (!_usedTagHelpers.Contains(descriptor.TypeName))
                {
                    _usedTagHelpers.Add(descriptor.TypeName);
                }
            }

            Accept(chunk.Children);
        }
    }
}