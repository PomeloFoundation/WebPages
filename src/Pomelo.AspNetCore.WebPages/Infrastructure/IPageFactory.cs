﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Pomelo.AspNetCore.WebPages.Infrastructure
{
    public interface IPageFactory
    {
        object CreatePage(PageContext context);

        void ReleasePage(PageContext context, object page);
    }
}
