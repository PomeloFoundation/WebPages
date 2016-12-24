// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Razor;

namespace Pomelo.AspNetCore.WebPages.Razevolution
{
    public class DefaultRazorEngine : RazorEngine
    {
        public DefaultRazorEngine(IRazorEngineFeature[] features, IRazorEnginePhase[] phases)
        {
            Features = features;
            Phases = phases;

            for (var i = 0; i < features.Length; i++)
            {
                features[i].Engine = this;
            }

            for (var i = 0; i < phases.Length; i++)
            {
                phases[i].Engine = this;
            }
        }

        public override IReadOnlyList<IRazorEngineFeature> Features { get; }

        public override IReadOnlyList<IRazorEnginePhase> Phases { get; }

        public override RazorCodeDocument CreateCodeDocument(RazorSourceDocument source)
        {
            return new DefaultRazorCodeDocument(source);
        }

        public override void ExecutePhase(IRazorEnginePhase phase, RazorCodeDocument document)
        {
            phase.Execute(document);
        }

        public override void Process(RazorCodeDocument document)
        {
            for (var i = 0; i < Phases.Count; i++)
            {
                var phase = Phases[i];
                ExecutePhase(phase, document);
            }
        }

        private class DefaultRazorCodeDocument : RazorCodeDocument
        {
            public DefaultRazorCodeDocument(RazorSourceDocument source)
            {
                Source = source;

                ErrorSink = new ErrorSink();
                Items = new Items();
            }

            public override ErrorSink ErrorSink { get; }

            public override ItemCollection Items { get; }

            public override RazorSourceDocument Source { get; }
        }

        private class Items : RazorCodeDocument.ItemCollection
        {
            private readonly Dictionary<object, object> _items;

            public Items()
            {
                _items = new Dictionary<object, object>();
            }

            public override object this[object key]
            {
                get
                {
                    object value;
                    _items.TryGetValue(key, out value);
                    return value;
                }
                set
                {
                    _items[key] = value;
                }
            }
        }
    }
}
