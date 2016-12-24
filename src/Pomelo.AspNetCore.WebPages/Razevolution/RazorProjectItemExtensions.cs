// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Pomelo.AspNetCore.WebPages.Razevolution
{
    public static class RazorProjectItemExtensions
    {
        public static RazorSourceDocument ToSourceDocument(this RazorProjectItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            using (var stream = item.Read())
            {
                return RazorSourceDocument.ReadFrom(stream, item.PhysicalPath ?? item.CominedPath);
            }
        }
    }
}
