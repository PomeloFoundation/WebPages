// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.FileProviders;

namespace Pomelo.AspNetCore.WebPages
{
    public class WebPagesOptions : IPageModelConventionBuilder
    {
        public string PagesPath { get; set; } = "/";

        public CSharpCompilationOptions CompilationOptions { get; } = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

        public IList<IPageModelConvention> Conventions { get; } = new List<IPageModelConvention>();

        public string DefaultNamespace { get; set; }

        public IList<IFileProvider> FileProviders { get; } = new List<IFileProvider>();

        void IPageModelConventionBuilder.Add(IPageModelConvention convention)
        {
            if (convention == null)
            {
                throw new ArgumentNullException(nameof(convention));
            }

            Conventions.Add(convention);
        }
    }
}
