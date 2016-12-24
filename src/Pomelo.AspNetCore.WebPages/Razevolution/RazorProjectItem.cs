// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.IO;

namespace Pomelo.AspNetCore.WebPages.Razevolution
{
    public abstract class RazorProjectItem
    {
        public abstract string BasePath { get; }
        
        public abstract string Path { get; }

        public abstract string PhysicalPath { get; }

        public abstract Stream Read();

        public string CominedPath
        {
            get
            {
                if (BasePath == "/")
                {
                    return Path;
                }
                else
                {
                    return BasePath + Path;
                }
            }
        }

        public string Extension
        {
            get
            {
                var index = Filename.LastIndexOf('.');
                if (index == -1)
                {
                    return null;
                }
                else
                {
                    return Filename.Substring(index);
                }
            }
        }

        public string Filename
        {
            get
            {
                var index = Path.LastIndexOf('/');
                return Path.Substring(index + 1);
            }
        }
        
        public string PathWithoutExtension
        {
            get
            {
                var index = Path.LastIndexOf('.');
                if (index == -1)
                {
                    return Path;
                }
                else
                {
                    return Path.Substring(0, index);
                }
            }
        }
    }
}
