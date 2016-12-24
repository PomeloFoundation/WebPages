// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Pomelo.AspNetCore.WebPages.Razevolution.IR
{
    public class SourceTreeBuilder
    {
        private readonly Stack<CSharpBlock> _activeScopes;
        private readonly Action _endBlock;

        public SourceTreeBuilder()
        {
            Root = new CSharpSourceTree();
            _activeScopes = new Stack<CSharpBlock>();
            _activeScopes.Push(Root);

            _endBlock = EndBlock;
        }

        public CSharpSourceTree Root { get; }

        private CSharpBlock CurrentScope => _activeScopes.Peek();

        public IDisposable BuildBlock<TBlock>() where TBlock : CSharpBlock, new()
        {
            return BuildBlock<TBlock>(configure: null);
        }

        public IDisposable BuildBlock<TBlock>(Action<TBlock> configure) where TBlock : CSharpBlock, new()
        {
            var csharpBlock = new TBlock();

            configure?.Invoke(csharpBlock);

            Add(csharpBlock);

            return UseBlock(csharpBlock);
        }

        public IDisposable UseBlock(CSharpBlock csharpBlock)
        {
            _activeScopes.Push(csharpBlock);

            var builderScope = new BlockBuilderScope(_endBlock);
            return builderScope;
        }

        private void EndBlock()
        {
            var poppedBlock = _activeScopes.Pop();
        }

        public void Add(ICSharpSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            CurrentScope.Children.Add(source);
        }

        private class BlockBuilderScope : IDisposable
        {
            private readonly Action _onDispose;

            public BlockBuilderScope(Action onDispose)
            {
                _onDispose = onDispose;
            }

            public void Dispose()
            {
                _onDispose();
            }
        }
    }
}
