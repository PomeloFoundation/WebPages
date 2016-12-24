// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileProviders;

namespace Pomelo.AspNetCore.WebPages.Razevolution
{
    public abstract class RazorProject
    {
        public static readonly RazorProject Empty = new EmptyRazorProject();

        public static RazorProject Create(IFileProvider provider)
        {
            return new DefaultRazorProject(provider);
        }

        public IEnumerable<RazorProjectItem> EnumerateItems(string extension)
        {
            if (extension != null && (extension.Length == 0 || extension[0] != '.'))
            {
                throw new ArgumentException("The extension must begin with a period '.'.");
            }

            return EnumerateItems("/", extension);
        }

        public abstract IEnumerable<RazorProjectItem> EnumerateItems(string path, string extension);

        public abstract RazorProjectItem GetItem(string path);

        public abstract IEnumerable<RazorProjectItem> EnumerateAscending(string path, string extension);

        public class DefaultRazorProject : RazorProject
        {
            public readonly IFileProvider _provider;

            public DefaultRazorProject(IFileProvider provider)
            {
                _provider = provider;
            }

            public override IEnumerable<RazorProjectItem> EnumerateAscending(string path, string extension)
            {
                if (path == null)
                {
                    throw new ArgumentNullException(nameof(path));
                }

                if (path.Length == 0 || path[0] != '/')
                {
                    throw new ArgumentException("The path must begin with a forward slash '/'.");
                }

                if (extension != null && (extension.Length == 0 || extension[0] != '.'))
                {
                    throw new ArgumentException("The extension must begin with a period '.'.");
                }

                var directory = _provider.GetDirectoryContents(path);
                if (directory.Exists)
                {
                    foreach (var file in directory)
                    {
                        if (file.IsDirectory)
                        {
                            continue;
                        }

                        if (extension == null || Path.GetExtension(file.Name) == extension)
                        {
                            yield return new DefaultRazorProjectItem(file, "/", path + "/" + file.Name);
                        }
                    }
                }

                if (path != "/")
                {
                    var parent = path.Substring(0, path.LastIndexOf('/'));
                    if (parent != "")
                    {
                        foreach (var item in EnumerateAscending(parent, extension))
                        {
                            yield return item;
                        }
                    }
                }
            }

            public override IEnumerable<RazorProjectItem> EnumerateItems(string path, string extension)
            {
                if (path == null)
                {
                    throw new ArgumentNullException(nameof(path));
                }

                if (path.Length == 0 || path[0] != '/')
                {
                    throw new ArgumentException("The path must begin with a forward slash '/'.");
                }

                if (extension != null && (extension.Length == 0 || extension[0] != '.'))
                {
                    throw new ArgumentException("The extension must begin with a period '.'.");
                }

                return EnumerateFiles(_provider.GetDirectoryContents(path), path, "", extension);
            }

            public override RazorProjectItem GetItem(string path)
            {
                if (path == null)
                {
                    throw new ArgumentNullException(nameof(path));
                }

                if (path.Length == 0 || path[0] != '/')
                {
                    throw new ArgumentException("The path must begin with a forward slash '/'.");
                }

                var info = _provider.GetFileInfo(path);
                if (info.Exists && !info.IsDirectory)
                {
                    return new DefaultRazorProjectItem(info, "/", path);
                }

                return null;
            }

            private IEnumerable<RazorProjectItem> EnumerateFiles(IDirectoryContents directory, string basePath, string prefix, string extension)
            {
                if (directory.Exists)
                {
                    foreach (var file in directory)
                    {
                        if (file.IsDirectory)
                        {
                            var innerDirectory = _provider.GetDirectoryContents(basePath + file.Name);
                            var children = EnumerateFiles(innerDirectory, basePath, prefix + "/" + file.Name, extension);
                            foreach (var child in children)
                            {
                                yield return child;
                            }
                        }
                        else if (extension == null || Path.GetExtension(file.Name) == extension)
                        {
                            yield return new DefaultRazorProjectItem(file, basePath, prefix + "/" + file.Name);
                        }
                    }
                }
            }

            private class DefaultRazorProjectItem : RazorProjectItem
            {
                private readonly IFileInfo _fileInfo;

                public DefaultRazorProjectItem(IFileInfo fileInfo, string basePath, string path)
                {
                    _fileInfo = fileInfo;
                    BasePath = basePath;
                    Path = path;
                }

                public override string BasePath { get; }

                public override string Path { get; }

                public override string PhysicalPath => _fileInfo.PhysicalPath;

                public override Stream Read()
                {
                    return _fileInfo.CreateReadStream();
                }
            }
        }

        private class EmptyRazorProject : RazorProject
        {
            public override IEnumerable<RazorProjectItem> EnumerateAscending(string path, string extension)
            {
                if (path == null)
                {
                    throw new ArgumentNullException(nameof(path));
                }

                if (path.Length == 0 || path[0] != '/')
                {
                    throw new ArgumentException("The path must begin with a forward slash '/'.");
                }

                if (extension != null && (extension.Length == 0 || extension[0] != '.'))
                {
                    throw new ArgumentException("The extension must begin with a period '.'.");
                }

                return Enumerable.Empty<RazorProjectItem>();
            }

            public override IEnumerable<RazorProjectItem> EnumerateItems(string path, string extension)
            {
                if (path == null)
                {
                    throw new ArgumentNullException(nameof(path));
                }

                if (path.Length == 0 || path[0] != '/')
                {
                    throw new ArgumentException("The path must begin with a forward slash '/'.");
                }

                if (extension != null && (extension.Length == 0 || extension[0] != '.'))
                {
                    throw new ArgumentException("The extension must begin with a period '.'.");
                }

                return Enumerable.Empty<RazorProjectItem>();
            }

            public override RazorProjectItem GetItem(string path)
            {
                if (path == null)
                {
                    throw new ArgumentNullException(nameof(path));
                }

                if (path.Length == 0 || path[0] != '/')
                {
                    throw new ArgumentException("The path must begin with a forward slash '/'.");
                }

                return null;
            }
        }
    }
}
