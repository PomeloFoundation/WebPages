// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.CodeGenerators;

namespace Pomelo.AspNetCore.WebPages.Razevolution.CSharpRendering
{
    public static class ErrorSinkExtensions
    {
        public static void OnError(this ErrorSink errorSink, MappingLocation mappingLocation, string message)
        {
            var location = new SourceLocation(
                mappingLocation.FilePath,
                mappingLocation.AbsoluteIndex,
                mappingLocation.LineIndex,
                mappingLocation.CharacterIndex);

            errorSink.OnError(location, message, mappingLocation.ContentLength);
        }
    }
}
