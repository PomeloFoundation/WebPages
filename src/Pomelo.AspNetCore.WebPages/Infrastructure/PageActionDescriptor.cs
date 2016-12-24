﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.Abstractions;

namespace Pomelo.AspNetCore.WebPages.Infrastructure
{
    public class PageActionDescriptor : ActionDescriptor
    {
        public string RelativePath { get; set; }

        public string ViewEnginePath { get; set; }
    }
}
