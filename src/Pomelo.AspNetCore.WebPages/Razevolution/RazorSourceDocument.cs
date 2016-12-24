// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;

namespace Pomelo.AspNetCore.WebPages.Razevolution
{
    public abstract class RazorSourceDocument
    {
        public static RazorSourceDocument ReadFrom(Stream stream, string filename)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (filename == null)
            {
                throw new ArgumentNullException(nameof(filename));
            }

            return ReadFromInternal(stream, encoding: null, filename: filename);
        }

        public static RazorSourceDocument ReadFrom(Stream stream, Encoding encoding, string filename)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            if (filename == null)
            {
                throw new ArgumentNullException(nameof(filename));
            }

            return ReadFromInternal(stream, encoding, filename);
        }

        private static RazorSourceDocument ReadFromInternal(Stream stream, Encoding encoding, string filename)
        {
            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);

            return new BufferedDocument(memoryStream, encoding, filename);
        }

        public abstract string Filename { get; }

        public abstract TextReader CreateReader();

        private class BufferedDocument : RazorSourceDocument
        {
            private Encoding _encoding;
            private string _filename;
            private MemoryStream _stream;

            public BufferedDocument(MemoryStream stream, Encoding encoding, string filename)
            {
                _stream = stream;
                _encoding = encoding;
                _filename = filename;
            }

            public override string Filename => _filename;

            public override TextReader CreateReader()
            {
                var copy = new MemoryStream(_stream.ToArray());

                return _encoding == null 
                    ? new StreamReader(copy, detectEncodingFromByteOrderMarks: true)
                    : new StreamReader(copy, _encoding);
            }
        }
    }
}
