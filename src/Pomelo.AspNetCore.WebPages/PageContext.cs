// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Pomelo.AspNetCore.WebPages.Infrastructure;
using Pomelo.AspNetCore.WebPages.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;

namespace Pomelo.AspNetCore.WebPages
{
    public class PageContext : ViewContext
    {
        private Page _page;
        private IList<IValueProviderFactory> _valueProviderFactories;

        public PageContext()
        {
        }

        public PageContext(
            ActionContext actionContext,
            ViewDataDictionary viewData,
            ITempDataDictionary tempDataDictionary,
            HtmlHelperOptions htmlHelperOptions)
            : base(actionContext, NullView.Instance, viewData, tempDataDictionary, TextWriter.Null, htmlHelperOptions)
        {
        }

        public new CompiledPageActionDescriptor ActionDescriptor
        {
            get { return (CompiledPageActionDescriptor)base.ActionDescriptor; }
            set { base.ActionDescriptor = value; }
        }

        public Page Page
        {
            get { return _page; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _page = value;
            }
        }

        public IList<IValueProviderFactory> ValueProviderFactories
        {
            get
            {
                if (_valueProviderFactories == null)
                {
                    _valueProviderFactories = new List<IValueProviderFactory>();
                }

                return _valueProviderFactories;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _valueProviderFactories = value;
            }
        }
    }
}
