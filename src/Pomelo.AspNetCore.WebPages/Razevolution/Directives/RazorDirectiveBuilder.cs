// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Pomelo.AspNetCore.WebPages.Razevolution.Directives
{
    public class RazorDirectiveBuilder
    {
        private readonly List<RazorDirectiveTokenDescriptor> _tokenDescriptors;
        private readonly string _name;
        private readonly RazorDirectiveDescriptorType _type;

        public RazorDirectiveBuilder(string name) : this(name, RazorDirectiveDescriptorType.SingleLine)
        {
        }

        public RazorDirectiveBuilder(string name, RazorDirectiveDescriptorType type)
        {
            _name = name;
            _type = type;
            _tokenDescriptors = new List<RazorDirectiveTokenDescriptor>();
        }

        public RazorDirectiveBuilder AddType()
        {
            var descriptor = new RazorDirectiveTokenDescriptor()
            {
                Type = RazorDirectiveTokenType.Type
            };
            _tokenDescriptors.Add(descriptor);

            return this;
        }

        public RazorDirectiveBuilder AddMember()
        {
            var descriptor = new RazorDirectiveTokenDescriptor()
            {
                Type = RazorDirectiveTokenType.Member
            };
            _tokenDescriptors.Add(descriptor);

            return this;
        }

        public RazorDirectiveBuilder AddString()
        {
            var descriptor = new RazorDirectiveTokenDescriptor()
            {
                Type = RazorDirectiveTokenType.String
            };
            _tokenDescriptors.Add(descriptor);

            return this;
        }

        public RazorDirectiveBuilder AddLiteral(string literal, bool optional)
        {
            var descriptor = new RazorDirectiveTokenDescriptor()
            {
                Type = RazorDirectiveTokenType.Literal,
                Value = literal,
                Optional = optional,
            };
            _tokenDescriptors.Add(descriptor);

            return this;
        }

        public RazorDirectiveDescriptor Build()
        {
            var descriptor = new RazorDirectiveDescriptor
            {
                Name = _name,
                Type = _type,
                Tokens = _tokenDescriptors,
            };

            return descriptor;
        }
    }
}
