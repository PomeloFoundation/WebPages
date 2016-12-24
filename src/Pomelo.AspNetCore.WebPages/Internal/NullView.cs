// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace Pomelo.AspNetCore.WebPages.Internal
{
    public class NullView : IView
    {
        public static readonly NullView Instance = new NullView();

        private NullView()
        {
        }

        public string Path => "";

        public Task RenderAsync(ViewContext context)
        {
            throw new NotImplementedException();
        }
    }
}
