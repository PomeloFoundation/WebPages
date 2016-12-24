// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Pomelo.AspNetCore.WebPages.Infrastructure;
using Pomelo.AspNetCore.WebPages.Razevolution;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;
using Microsoft.Extensions.Options;
using Xunit;

namespace Pomelo.AspNetCore.WebPages.Compilation
{
    public class DefaultPageCompilationServiceTest
    {
        [Fact]
        public void Load_EmptyPage_InheritsFromPage()
        {
            // Arrange
            var loader = CreateLoader();

            // Act
            var type = loader.Load("");
            
            // Assert
            Assert.Same(typeof(Page), type.GetTypeInfo().BaseType);
        }

        [Fact]
        public void Load_WithInheritsDirective_InheritsFromSpecifiedClass()
        {
            // Arrange
            var loader = CreateLoader();

            // Act
            var type = loader.Load($"@inherits {typeof(DefaultPageCompilationServiceTest) + "." + typeof(MyBaseClass).Name}");

            // Assert
            Assert.Same(typeof(MyBaseClass), type.GetTypeInfo().BaseType);
        }

        [Fact]
        public void Load_WithInheritsDirective_WithGeneratedConstructor()
        {
            // Arrange
            var compiler = CreateLoader();

            // Act
            var type = compiler.Load($"@inherits {typeof(DefaultPageCompilationServiceTest) + "." + typeof(MyBaseClassWithConstuctor).Name}");

            // Assert
            Assert.Same(typeof(MyBaseClassWithConstuctor), type.GetTypeInfo().BaseType);

            var constructor = Assert.Single(type.GetTypeInfo().DeclaredConstructors);
            Assert.Same(typeof(string), Assert.Single(constructor.GetParameters()).ParameterType);
        }

        // Test for @inherits
        public class MyBaseClass : Page
        {
        }

        public class MyBaseClassWithConstuctor : Page
        {
            public MyBaseClassWithConstuctor(string s)
            {
            }
        }

        private static TestPageLoader CreateLoader()
        {
            var partManager = new ApplicationPartManager();
            partManager.ApplicationParts.Add(new AssemblyPart(typeof(DefaultPageCompilationServiceTest).GetTypeInfo().Assembly));
            partManager.FeatureProviders.Add(new MetadataReferenceFeatureProvider());

            var referenceManager = new ApplicationPartManagerReferenceManager(partManager);

            var options = Options.Create(new WebPagesOptions()
            {
                DefaultNamespace = "TestNamespace",
            });

            var compilationFactory = new DefaultCSharpCompilationFactory(referenceManager, options);

            return new TestPageLoader(options, RazorProject.Empty, compilationFactory, new PageRazorEngineHost(), new NullTagHelperDescriptorResolver());

        }

        private class TestPageLoader : DefaultPageLoader
        {
            public TestPageLoader(
                IOptions<WebPagesOptions> options,
                RazorProject project,
                CSharpCompilationFactory compilationFactory,
                PageRazorEngineHost host,
                ITagHelperDescriptorResolver tagHelperDescriptorResolver)
                : base(options, project, compilationFactory, host, tagHelperDescriptorResolver)
            {
            }

            public Type Load(string text)
            {
                var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                writer.Write(text);
                writer.Flush();
                stream.Seek(0L, SeekOrigin.Begin);

                var relativePath = "/TestPage";
                var source = RazorSourceDocument.ReadFrom(stream, relativePath);
                return Load(source, relativePath);
            }

            protected override RazorSourceDocument CreateSourceDocument(PageActionDescriptor actionDescriptor)
            {
                return base.CreateSourceDocument(actionDescriptor);
            }

            protected override Type Load(RazorSourceDocument source, string relativePath)
            {
                return base.Load(source, relativePath);
            }
        }

        private class NullTagHelperDescriptorResolver : ITagHelperDescriptorResolver
        {
            public IEnumerable<TagHelperDescriptor> Resolve(TagHelperDescriptorResolutionContext resolutionContext)
            {
                return Enumerable.Empty<TagHelperDescriptor>();
            }
        }
    }
}
