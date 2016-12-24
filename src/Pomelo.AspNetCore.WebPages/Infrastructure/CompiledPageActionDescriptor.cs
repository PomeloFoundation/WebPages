// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;

namespace Pomelo.AspNetCore.WebPages.Infrastructure
{
    public class CompiledPageActionDescriptor : PageActionDescriptor
    {
        public CompiledPageActionDescriptor(PageActionDescriptor other)
        {
            ActionConstraints = other.ActionConstraints;
            AttributeRouteInfo = other.AttributeRouteInfo;
            BoundProperties = other.BoundProperties;
            DisplayName = other.DisplayName;
            FilterDescriptors = other.FilterDescriptors;
            Parameters = other.Parameters;
            Properties = other.Properties;
            RelativePath = other.RelativePath;
            RouteValues = other.RouteValues;
            ViewEnginePath = other.ViewEnginePath;
        }

        public IList<HandlerMethodDescriptor> HandlerMethods { get; set; }

        public TypeInfo ModelType { get; set; }

        public TypeInfo PageType { get; set; }
    }
}
