// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Pomelo.AspNetCore.WebPages.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pomelo.AspNetCore.WebPages.Data;

namespace Pomelo.AspNetCore.WebPages
{
    public abstract class Page : IRazorPage, IDisposable
    {
        private PageArgumentBinder _binder;
        private readonly HashSet<string> _renderedSections = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Stack<TagHelperScopeInfo> _tagHelperScopes = new Stack<TagHelperScopeInfo>();
        private IUrlHelper _urlHelper;
        private ITagHelperFactory _tagHelperFactory;
        private bool _renderedBody;
        private AttributeInfo _attributeInfo;
        private TagHelperAttributeInfo _tagHelperAttributeInfo;
        private StringWriter _valueBuffer;
        private IViewBufferScope _bufferScope;
        private bool _ignoreBody;
        private HashSet<string> _ignoredSections;
        private TextWriter _pageWriter;

        public Page()
        {
            SectionWriters = new Dictionary<string, RenderAsyncDelegate>(StringComparer.OrdinalIgnoreCase);
        }

        [BindNever]
        public Database Database { get; set; }

        [BindNever]
        public PageArgumentBinder Binder
        {
            get
            {
                if (_binder == null)
                {
                    _binder = PageContext.HttpContext.RequestServices.GetRequiredService<PageArgumentBinder>();
                }

                return _binder;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _binder = value;
            }
        }

        [BindNever]
        public bool IsGet { get { return Context.Request.Method.ToUpper() == "GET"; } }
        [BindNever]
        public bool IsPost { get { return Context.Request.Method.ToUpper() == "POST"; } }
        [BindNever]
        public bool IsPut { get { return Context.Request.Method.ToUpper() == "PUT"; } }
        [BindNever]
        public bool IsDelete { get { return Context.Request.Method.ToUpper() == "DELETE"; } }

        [BindNever]
        public PageContext PageContext { get; set; }

        [BindNever]
        public ModelStateDictionary ModelState => PageContext.ModelState;

        [BindNever]
        public ViewDataDictionary ViewData => PageContext?.ViewData;

        protected Task<T> BindAsync<T>(string name)
        {
            return Binder.BindModelAsync<T>(PageContext, name);
        }

        protected Task<T> BindAsync<T>(T @default, string name)
        {
            return Binder.BindModelAsync<T>(PageContext, @default, name);
        }

        protected Task<bool> TryUpdateModelAsync<T>(T value)
        {
            return Binder.TryUpdateModelAsync<T>(PageContext, value);
        }

        protected Task<bool> TryUpdateModelAsync<T>(T value, string name)
        {
            return Binder.TryUpdateModelAsync<T>(PageContext, value, name);
        }

        protected IActionResult Redirect(string url)
        {
            return new RedirectResult(url);
        }

        protected IActionResult View()
        {
            return new PageViewResult(this);
        }

        protected IActionResult View<T>(T model)
        {
            return new PageViewResult(this, model);
        }

        protected ContentResult Content(string content, string contentType = "text/plain", int statusCode = 200)
        {
            return new ContentResult() { Content = content, ContentType = contentType, StatusCode = statusCode };
        }
        
        public virtual JsonResult Json(object data)
        {
            return new JsonResult(data);
        }
        
        public virtual JsonResult Json(object data, JsonSerializerSettings serializerSettings)
        {
            if (serializerSettings == null)
            {
                throw new ArgumentNullException(nameof(serializerSettings));
            }

            return new JsonResult(data, serializerSettings);
        }

        public virtual FileContentResult File(byte[] fileContents, string contentType)
        {
            return File(fileContents, contentType, fileDownloadName: null);
        }

        public virtual FileContentResult File(byte[] fileContents, string contentType, string fileDownloadName)
        {
            return new FileContentResult(fileContents, contentType) { FileDownloadName = fileDownloadName };
        }

        public virtual FileStreamResult File(Stream fileStream, string contentType)
        {
            return File(fileStream, contentType, fileDownloadName: null);
        }

        public virtual FileStreamResult File(Stream fileStream, string contentType, string fileDownloadName)
        {
            return new FileStreamResult(fileStream, contentType) { FileDownloadName = fileDownloadName };
        }

        public virtual VirtualFileResult File(string virtualPath, string contentType)
        {
            return File(virtualPath, contentType, fileDownloadName: null);
        }

        public virtual VirtualFileResult File(string virtualPath, string contentType, string fileDownloadName)
        {
            return new VirtualFileResult(virtualPath, contentType) { FileDownloadName = fileDownloadName };
        }

        public virtual PhysicalFileResult PhysicalFile(string physicalPath, string contentType)
        {
            return PhysicalFile(physicalPath, contentType, fileDownloadName: null);
        }

        public virtual PhysicalFileResult PhysicalFile(
            string physicalPath,
            string contentType,
            string fileDownloadName)
        {
            return new PhysicalFileResult(physicalPath, contentType) { FileDownloadName = fileDownloadName };
        }

        public virtual RedirectToActionResult RedirectToAction(string actionName)
        {
            return RedirectToAction(actionName, routeValues: null);
        }

        public virtual RedirectToActionResult RedirectToAction(string actionName, object routeValues)
        {
            return RedirectToAction(actionName, controllerName: null, routeValues: routeValues);
        }

        public virtual RedirectToActionResult RedirectToAction(string actionName, string controllerName)
        {
            return RedirectToAction(actionName, controllerName, routeValues: null);
        }

        public virtual RedirectToActionResult RedirectToAction(
            string actionName,
            string controllerName,
            object routeValues)
        {
            return new RedirectToActionResult(actionName, controllerName, routeValues)
            {
                UrlHelper = _urlHelper,
            };
        }

        public virtual RedirectToActionResult RedirectToActionPermanent(string actionName)
        {
            return RedirectToActionPermanent(actionName, routeValues: null);
        }

        public virtual RedirectToActionResult RedirectToActionPermanent(string actionName, object routeValues)
        {
            return RedirectToActionPermanent(actionName, controllerName: null, routeValues: routeValues);
        }

        public virtual RedirectToActionResult RedirectToActionPermanent(string actionName, string controllerName)
        {
            return RedirectToActionPermanent(actionName, controllerName, routeValues: null);
        }

        public virtual RedirectToActionResult RedirectToActionPermanent(
            string actionName,
            string controllerName,
            object routeValues)
        {
            return new RedirectToActionResult(
                actionName,
                controllerName,
                routeValues,
                permanent: true)
            {
                UrlHelper = _urlHelper,
            };
        }

        public virtual RedirectToRouteResult RedirectToRoute(string routeName)
        {
            return RedirectToRoute(routeName, routeValues: null);
        }

        public virtual RedirectToRouteResult RedirectToRoute(object routeValues)
        {
            return RedirectToRoute(routeName: null, routeValues: routeValues);
        }

        public virtual RedirectToRouteResult RedirectToRoute(string routeName, object routeValues)
        {
            return new RedirectToRouteResult(routeName, routeValues)
            {
                UrlHelper = _urlHelper,
            };
        }

        public virtual RedirectToRouteResult RedirectToRoutePermanent(string routeName)
        {
            return RedirectToRoutePermanent(routeName, routeValues: null);
        }

        public virtual RedirectToRouteResult RedirectToRoutePermanent(object routeValues)
        {
            return RedirectToRoutePermanent(routeName: null, routeValues: routeValues);
        }

        public virtual RedirectToRouteResult RedirectToRoutePermanent(string routeName, object routeValues)
        {
            return new RedirectToRouteResult(routeName, routeValues, permanent: true)
            {
                UrlHelper = _urlHelper,
            };
        }

        public virtual UnauthorizedResult Unauthorized()
        {
            return new UnauthorizedResult();
        }

        public virtual NotFoundResult NotFound()
        {
            return new NotFoundResult();
        }

        public virtual NotFoundObjectResult NotFound(object value)
        {
            return new NotFoundObjectResult(value);
        }

        public virtual BadRequestResult BadRequest()
        {
            return new BadRequestResult();
        }

        public virtual BadRequestObjectResult BadRequest(object error)
        {
            return new BadRequestObjectResult(error);
        }

        public virtual BadRequestObjectResult BadRequest(ModelStateDictionary modelState)
        {
            if (modelState == null)
            {
                throw new ArgumentNullException(nameof(modelState));
            }

            return new BadRequestObjectResult(modelState);
        }

        Task IRazorPage.ExecuteAsync()
        {
            return RenderAsync();
        }

        public virtual Task RenderAsync()
        {
            return TaskCache.CompletedTask;
        }

        /// <summary>
        /// An <see cref="HttpContext"/> representing the current request execution.
        /// </summary>
        [BindNever]
        public HttpContext Context => PageContext?.HttpContext;

        /// <inheritdoc />
        [BindNever]
        public string Path { get; set; }

        /// <inheritdoc />
        [BindNever]
        public ViewContext ViewContext
        {
            get { return PageContext; }
            set
            {
                if (!object.ReferenceEquals(PageContext, value))
                {
                    throw new InvalidOperationException();
                }
            }
        }

        /// <inheritdoc />
        [BindNever]
        public string Layout { get; set; }

        /// <summary>
        /// Gets the <see cref="System.Text.Encodings.Web.HtmlEncoder"/> to use when this <see cref="RazorPage"/>
        /// handles non-<see cref="IHtmlContent"/> C# expressions.
        /// </summary>
        [RazorInject]
        [BindNever]
        public HtmlEncoder HtmlEncoder { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="DiagnosticSource.DiagnosticSource"/> instance used to instrument the page execution.
        /// </summary>
        [RazorInject]
        [BindNever]
        public DiagnosticSource DiagnosticSource { get; set; }

        [RazorInject]
        [BindNever]
        public IModelExpressionProvider ModelExpressionProvider { get; set; }

        /// <summary>
        /// Gets the <see cref="TextWriter"/> that the page is writing output to.
        /// </summary>
        [BindNever]
        public virtual TextWriter Output
        {
            get
            {
                if (ViewContext == null)
                {
                    throw new InvalidOperationException("nahhhh");
                }

                return ViewContext.Writer;
            }
        }

        /// <summary>
        /// Gets the <see cref="ClaimsPrincipal"/> of the current logged in user.
        /// </summary>
        [BindNever]
        public virtual ClaimsPrincipal User => Context?.User;

        /// <summary>
        /// Gets the dynamic view data dictionary.
        /// </summary>
        [BindNever]
        public dynamic ViewBag => ViewContext?.ViewBag;

        /// <summary>
        /// Gets the <see cref="ITempDataDictionary"/> from the <see cref="ViewContext"/>.
        /// </summary>
        /// <remarks>Returns null if <see cref="ViewContext"/> is null.</remarks>
        [BindNever]
        public ITempDataDictionary TempData => ViewContext?.TempData;

        /// <inheritdoc />
        [BindNever]
        public IHtmlContent BodyContent { get; set; }

        [BindNever]
        public HttpRequest Request { get { return Context.Request; } }

        [BindNever]
        public HttpResponse Response { get { return Context.Response; } }

        [BindNever]
        public IServiceProvider RequestServices { get { return Context.RequestServices; } }

        [BindNever]
        public WebMail WebMail { get { return RequestServices.GetRequiredService<WebMail>(); } }

        [BindNever]
        public ISession Session { get { return Context.Session; } }

        /// <inheritdoc />
        [BindNever]
        public bool IsLayoutBeingRendered { get; set; }

        /// <inheritdoc />
        [BindNever]
        public IDictionary<string, RenderAsyncDelegate> PreviousSectionWriters { get; set; }

        /// <inheritdoc />
        [BindNever]
        public IDictionary<string, RenderAsyncDelegate> SectionWriters { get; }

        [BindNever]
        private ITagHelperFactory TagHelperFactory
        {
            get
            {
                if (_tagHelperFactory == null)
                {
                    var services = ViewContext.HttpContext.RequestServices;
                    _tagHelperFactory = services.GetRequiredService<ITagHelperFactory>();
                }

                return _tagHelperFactory;
            }
        }

        [BindNever]
        private IViewBufferScope BufferScope
        {
            get
            {
                if (_bufferScope == null)
                {
                    var services = ViewContext.HttpContext.RequestServices;
                    _bufferScope = services.GetRequiredService<IViewBufferScope>();
                }

                return _bufferScope;
            }
        }

        /// <summary>
        /// Format an error message about using an indexer when the tag helper property is <c>null</c>.
        /// </summary>
        /// <param name="attributeName">Name of the HTML attribute associated with the indexer.</param>
        /// <param name="tagHelperTypeName">Full name of the tag helper <see cref="Type"/>.</param>
        /// <param name="propertyName">Dictionary property in the tag helper.</param>
        /// <returns>An error message about using an indexer when the tag helper property is <c>null</c>.</returns>
        public static string InvalidTagHelperIndexerAssignment(
            string attributeName,
            string tagHelperTypeName,
            string propertyName)
        {
            return "No way bro!";
        }

        /// <summary>
        /// Creates and activates a <see cref="ITagHelper"/>.
        /// </summary>
        /// <typeparam name="TTagHelper">A <see cref="ITagHelper"/> type.</typeparam>
        /// <returns>The activated <see cref="ITagHelper"/>.</returns>
        /// <remarks>
        /// <typeparamref name="TTagHelper"/> must have a parameterless constructor.
        /// </remarks>
        public TTagHelper CreateTagHelper<TTagHelper>() where TTagHelper : ITagHelper
        {
            return TagHelperFactory.CreateTagHelper<TTagHelper>(ViewContext);
        }

        /// <summary>
        /// Starts a new writing scope and optionally overrides <see cref="HtmlEncoder"/> within that scope.
        /// </summary>
        /// <param name="encoder">
        /// The <see cref="System.Text.Encodings.Web.HtmlEncoder"/> to use when this <see cref="RazorPage"/> handles
        /// non-<see cref="IHtmlContent"/> C# expressions. If <c>null</c>, does not change <see cref="HtmlEncoder"/>.
        /// </param>
        /// <remarks>
        /// All writes to the <see cref="Output"/> or <see cref="ViewContext.Writer"/> after calling this method will
        /// be buffered until <see cref="EndTagHelperWritingScope"/> is called.
        /// </remarks>
        public void StartTagHelperWritingScope(HtmlEncoder encoder)
        {
            var buffer = new ViewBuffer(BufferScope, Path, ViewBuffer.TagHelperPageSize);
            _tagHelperScopes.Push(new TagHelperScopeInfo(buffer, HtmlEncoder, ViewContext.Writer));

            // If passed an HtmlEncoder, override the property.
            if (encoder != null)
            {
                HtmlEncoder = encoder;
            }

            // We need to replace the ViewContext's Writer to ensure that all content (including content written
            // from HTML helpers) is redirected.
            ViewContext.Writer = new ViewBufferTextWriter(buffer, ViewContext.Writer.Encoding);
        }

        /// <summary>
        /// Ends the current writing scope that was started by calling <see cref="StartTagHelperWritingScope"/>.
        /// </summary>
        /// <returns>The buffered <see cref="TagHelperContent"/>.</returns>
        public TagHelperContent EndTagHelperWritingScope()
        {
            if (_tagHelperScopes.Count == 0)
            {
                throw new InvalidOperationException("derp.");
            }

            var scopeInfo = _tagHelperScopes.Pop();

            // Get the content written during the current scope.
            var tagHelperContent = new DefaultTagHelperContent();
            tagHelperContent.AppendHtml(scopeInfo.Buffer);

            // Restore previous scope.
            HtmlEncoder = scopeInfo.Encoder;
            ViewContext.Writer = scopeInfo.Writer;

            return tagHelperContent;
        }

        /// <summary>
        /// Starts a new scope for writing <see cref="ITagHelper"/> attribute values.
        /// </summary>
        /// <remarks>
        /// All writes to the <see cref="Output"/> or <see cref="ViewContext.Writer"/> after calling this method will
        /// be buffered until <see cref="EndWriteTagHelperAttribute"/> is called.
        /// The content will be buffered using a shared <see cref="StringWriter"/> within this <see cref="RazorPage"/>
        /// Nesting of <see cref="BeginWriteTagHelperAttribute"/> and <see cref="EndWriteTagHelperAttribute"/> method calls
        /// is not supported.
        /// </remarks>
        public void BeginWriteTagHelperAttribute()
        {
            if (_pageWriter != null)
            {
                throw new InvalidOperationException("derp");
            }

            _pageWriter = ViewContext.Writer;

            if (_valueBuffer == null)
            {
                _valueBuffer = new StringWriter();
            }

            // We need to replace the ViewContext's Writer to ensure that all content (including content written
            // from HTML helpers) is redirected.
            ViewContext.Writer = _valueBuffer;

        }

        /// <summary>
        /// Ends the current writing scope that was started by calling <see cref="BeginWriteTagHelperAttribute"/>.
        /// </summary>
        /// <returns>The content buffered by the shared <see cref="StringWriter"/> of this <see cref="RazorPage"/>.</returns>
        /// <remarks>
        /// This method assumes that there will be no nesting of <see cref="BeginWriteTagHelperAttribute"/>
        /// and <see cref="EndWriteTagHelperAttribute"/> method calls.
        /// </remarks>
        public string EndWriteTagHelperAttribute()
        {
            if (_pageWriter == null)
            {
                throw new InvalidOperationException("derp");
            }

            var content = _valueBuffer.ToString();
            _valueBuffer.GetStringBuilder().Clear();

            // Restore previous writer.
            ViewContext.Writer = _pageWriter;
            _pageWriter = null;

            return content;
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> with HTML encoding to <see cref="Output"/>.
        /// </summary>
        /// <param name="value">The <see cref="object"/> to write.</param>
        public virtual void Write(object value)
        {
            WriteTo(Output, value);
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> with HTML encoding to <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="value">The <see cref="object"/> to write.</param>
        /// <remarks>
        /// <paramref name="value"/>s of type <see cref="IHtmlContent"/> are written using
        /// <see cref="IHtmlContent.WriteTo(TextWriter, HtmlEncoder)"/>.
        /// For all other types, the encoded result of <see cref="object.ToString"/> is written to the
        /// <paramref name="writer"/>.
        /// </remarks>
        public virtual void WriteTo(TextWriter writer, object value)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            WriteTo(writer, HtmlEncoder, value);
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> with HTML encoding to given <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="encoder">
        /// The <see cref="System.Text.Encodings.Web.HtmlEncoder"/> to use when encoding <paramref name="value"/>.
        /// </param>
        /// <param name="value">The <see cref="object"/> to write.</param>
        /// <remarks>
        /// <paramref name="value"/>s of type <see cref="IHtmlContent"/> are written using
        /// <see cref="IHtmlContent.WriteTo(TextWriter, HtmlEncoder)"/>.
        /// For all other types, the encoded result of <see cref="object.ToString"/> is written to the
        /// <paramref name="writer"/>.
        /// </remarks>
        public static void WriteTo(TextWriter writer, HtmlEncoder encoder, object value)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (encoder == null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }

            if (value == null || value == HtmlString.Empty)
            {
                return;
            }

            var htmlContent = value as IHtmlContent;
            if (htmlContent != null)
            {
                var bufferedWriter = writer as ViewBufferTextWriter;
                if (bufferedWriter == null || !bufferedWriter.IsBuffering)
                {
                    htmlContent.WriteTo(writer, encoder);
                }
                else
                {
                    var htmlContentContainer = value as IHtmlContentContainer;
                    if (htmlContentContainer != null)
                    {
                        // This is likely another ViewBuffer.
                        htmlContentContainer.MoveTo(bufferedWriter.Buffer);
                    }
                    else
                    {
                        // Perf: This is the common case for IHtmlContent, ViewBufferTextWriter is inefficient
                        // for writing character by character.
                        bufferedWriter.Buffer.AppendHtml(htmlContent);
                    }
                }

                return;
            }

            WriteTo(writer, encoder, value.ToString());
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> with HTML encoding to <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="value">The <see cref="string"/> to write.</param>
        public virtual void WriteTo(TextWriter writer, string value)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            WriteTo(writer, HtmlEncoder, value);
        }

        private static void WriteTo(TextWriter writer, HtmlEncoder encoder, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                // Perf: Encode right away instead of writing it character-by-character.
                // character-by-character isn't efficient when using a writer backed by a ViewBuffer.
                var encoded = encoder.Encode(value);
                writer.Write(encoded);
            }
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> without HTML encoding to <see cref="Output"/>.
        /// </summary>
        /// <param name="value">The <see cref="object"/> to write.</param>
        public virtual void WriteLiteral(object value)
        {
            WriteLiteralTo(Output, value);
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> without HTML encoding to the <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="value">The <see cref="object"/> to write.</param>
        public virtual void WriteLiteralTo(TextWriter writer, object value)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (value != null)
            {
                WriteLiteralTo(writer, value.ToString());
            }
        }

        /// <summary>
        /// Writes the specified <paramref name="value"/> without HTML encoding to <see cref="Output"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> instance to write to.</param>
        /// <param name="value">The <see cref="string"/> to write.</param>
        public virtual void WriteLiteralTo(TextWriter writer, string value)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (!string.IsNullOrEmpty(value))
            {
                writer.Write(value);
            }
        }

        public virtual void BeginWriteAttribute(
            string name,
            string prefix,
            int prefixOffset,
            string suffix,
            int suffixOffset,
            int attributeValuesCount)
        {
            BeginWriteAttributeTo(Output, name, prefix, prefixOffset, suffix, suffixOffset, attributeValuesCount);
        }

        public virtual void BeginWriteAttributeTo(
            TextWriter writer,
            string name,
            string prefix,
            int prefixOffset,
            string suffix,
            int suffixOffset,
            int attributeValuesCount)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (prefix == null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            if (suffix == null)
            {
                throw new ArgumentNullException(nameof(suffix));
            }

            _attributeInfo = new AttributeInfo(name, prefix, prefixOffset, suffix, suffixOffset, attributeValuesCount);

            // Single valued attributes might be omitted in entirety if it the attribute value strictly evaluates to
            // null  or false. Consequently defer the prefix generation until we encounter the attribute value.
            if (attributeValuesCount != 1)
            {
                WritePositionTaggedLiteral(writer, prefix, prefixOffset);
            }
        }

        public void WriteAttributeValue(
            string prefix,
            int prefixOffset,
            object value,
            int valueOffset,
            int valueLength,
            bool isLiteral)
        {
            WriteAttributeValueTo(Output, prefix, prefixOffset, value, valueOffset, valueLength, isLiteral);
        }

        public void WriteAttributeValueTo(
            TextWriter writer,
            string prefix,
            int prefixOffset,
            object value,
            int valueOffset,
            int valueLength,
            bool isLiteral)
        {
            if (_attributeInfo.AttributeValuesCount == 1)
            {
                if (IsBoolFalseOrNullValue(prefix, value))
                {
                    // Value is either null or the bool 'false' with no prefix; don't render the attribute.
                    _attributeInfo.Suppressed = true;
                    return;
                }

                // We are not omitting the attribute. Write the prefix.
                WritePositionTaggedLiteral(writer, _attributeInfo.Prefix, _attributeInfo.PrefixOffset);

                if (IsBoolTrueWithEmptyPrefixValue(prefix, value))
                {
                    // The value is just the bool 'true', write the attribute name instead of the string 'True'.
                    value = _attributeInfo.Name;
                }
            }

            // This block handles two cases.
            // 1. Single value with prefix.
            // 2. Multiple values with or without prefix.
            if (value != null)
            {
                if (!string.IsNullOrEmpty(prefix))
                {
                    WritePositionTaggedLiteral(writer, prefix, prefixOffset);
                }

                BeginContext(valueOffset, valueLength, isLiteral);

                WriteUnprefixedAttributeValueTo(writer, value, isLiteral);

                EndContext();
            }
        }

        public virtual void EndWriteAttribute()
        {
            EndWriteAttributeTo(Output);
        }

        public virtual void EndWriteAttributeTo(TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (!_attributeInfo.Suppressed)
            {
                WritePositionTaggedLiteral(writer, _attributeInfo.Suffix, _attributeInfo.SuffixOffset);
            }
        }

        public void BeginAddHtmlAttributeValues(
            TagHelperExecutionContext executionContext,
            string attributeName,
            int attributeValuesCount,
            HtmlAttributeValueStyle attributeValueStyle)
        {
            _tagHelperAttributeInfo = new TagHelperAttributeInfo(
                executionContext,
                attributeName,
                attributeValuesCount,
                attributeValueStyle);
        }

        public void AddHtmlAttributeValue(
            string prefix,
            int prefixOffset,
            object value,
            int valueOffset,
            int valueLength,
            bool isLiteral)
        {
            Debug.Assert(_tagHelperAttributeInfo.ExecutionContext != null);
            if (_tagHelperAttributeInfo.AttributeValuesCount == 1)
            {
                if (IsBoolFalseOrNullValue(prefix, value))
                {
                    // The first value was 'null' or 'false' indicating that we shouldn't render the attribute. The
                    // attribute is treated as a TagHelper attribute so it's only available in
                    // TagHelperContext.AllAttributes for TagHelper authors to see (if they want to see why the
                    // attribute was removed from TagHelperOutput.Attributes).
                    _tagHelperAttributeInfo.ExecutionContext.AddTagHelperAttribute(
                        _tagHelperAttributeInfo.Name,
                        value?.ToString() ?? string.Empty,
                        _tagHelperAttributeInfo.AttributeValueStyle);
                    _tagHelperAttributeInfo.Suppressed = true;
                    return;
                }
                else if (IsBoolTrueWithEmptyPrefixValue(prefix, value))
                {
                    _tagHelperAttributeInfo.ExecutionContext.AddHtmlAttribute(
                        _tagHelperAttributeInfo.Name,
                        _tagHelperAttributeInfo.Name,
                        _tagHelperAttributeInfo.AttributeValueStyle);
                    _tagHelperAttributeInfo.Suppressed = true;
                    return;
                }
            }

            if (value != null)
            {
                // Perf: We'll use this buffer for all of the attribute values and then clear it to
                // reduce allocations.
                if (_valueBuffer == null)
                {
                    _valueBuffer = new StringWriter();
                }

                if (!string.IsNullOrEmpty(prefix))
                {
                    WriteLiteralTo(_valueBuffer, prefix);
                }

                WriteUnprefixedAttributeValueTo(_valueBuffer, value, isLiteral);
            }
        }

        public void EndAddHtmlAttributeValues(TagHelperExecutionContext executionContext)
        {
            if (!_tagHelperAttributeInfo.Suppressed)
            {
                // Perf: _valueBuffer might be null if nothing was written. If it is set, clear it so
                // it is reset for the next value.
                var content = _valueBuffer == null ? HtmlString.Empty : new HtmlString(_valueBuffer.ToString());
                _valueBuffer?.GetStringBuilder().Clear();

                executionContext.AddHtmlAttribute(_tagHelperAttributeInfo.Name, content, _tagHelperAttributeInfo.AttributeValueStyle);
            }
        }

        public virtual string Href(string contentPath)
        {
            if (contentPath == null)
            {
                throw new ArgumentNullException(nameof(contentPath));
            }

            if (_urlHelper == null)
            {
                var services = Context.RequestServices;
                var factory = services.GetRequiredService<IUrlHelperFactory>();
                _urlHelper = factory.GetUrlHelper(ViewContext);
            }

            return _urlHelper.Content(contentPath);
        }

        private void WriteUnprefixedAttributeValueTo(TextWriter writer, object value, bool isLiteral)
        {
            var stringValue = value as string;

            // The extra branching here is to ensure that we call the Write*To(string) overload where possible.
            if (isLiteral && stringValue != null)
            {
                WriteLiteralTo(writer, stringValue);
            }
            else if (isLiteral)
            {
                WriteLiteralTo(writer, value);
            }
            else if (stringValue != null)
            {
                WriteTo(writer, stringValue);
            }
            else
            {
                WriteTo(writer, value);
            }
        }

        private void WritePositionTaggedLiteral(TextWriter writer, string value, int position)
        {
            BeginContext(position, value.Length, isLiteral: true);
            WriteLiteralTo(writer, value);
            EndContext();
        }

        /// <summary>
        /// In a Razor layout page, renders the portion of a content page that is not within a named section.
        /// </summary>
        /// <returns>The HTML content to render.</returns>
        protected virtual IHtmlContent RenderBody()
        {
            if (BodyContent == null)
            {
                throw new InvalidOperationException("nah.");
            }

            _renderedBody = true;
            return BodyContent;
        }

        /// <summary>
        /// In a Razor layout page, ignores rendering the portion of a content page that is not within a named section.
        /// </summary>
        public void IgnoreBody()
        {
            _ignoreBody = true;
        }

        /// <summary>
        /// Creates a named content section in the page that can be invoked in a Layout page using
        /// <see cref="RenderSection(string)"/> or <see cref="RenderSectionAsync(string, bool)"/>.
        /// </summary>
        /// <param name="name">The name of the section to create.</param>
        /// <param name="section">The <see cref="RenderAsyncDelegate"/> to execute when rendering the section.</param>
        public void DefineSection(string name, RenderAsyncDelegate section)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (section == null)
            {
                throw new ArgumentNullException(nameof(section));
            }

            if (SectionWriters.ContainsKey(name))
            {
                throw new InvalidOperationException("hurr durr");
            }
            SectionWriters[name] = section;
        }

        /// <summary>
        /// Returns a value that indicates whether the specified section is defined in the content page.
        /// </summary>
        /// <param name="name">The section name to search for.</param>
        /// <returns><c>true</c> if the specified section is defined in the content page; otherwise, <c>false</c>.</returns>
        public bool IsSectionDefined(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            EnsureMethodCanBeInvoked(nameof(IsSectionDefined));
            return PreviousSectionWriters.ContainsKey(name);
        }

        /// <summary>
        /// In layout pages, renders the content of the section named <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the section to render.</param>
        /// <returns>Returns <see cref="HtmlString.Empty"/> to allow the <see cref="Write(object)"/> call to
        /// succeed.</returns>
        /// <remarks>The method writes to the <see cref="Output"/> and the value returned is a token
        /// value that allows the Write (produced due to @RenderSection(..)) to succeed. However the
        /// value does not represent the rendered content.</remarks>
        public HtmlString RenderSection(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return RenderSection(name, required: true);
        }

        /// <summary>
        /// In layout pages, renders the content of the section named <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The section to render.</param>
        /// <param name="required">Indicates if this section must be rendered.</param>
        /// <returns>Returns <see cref="HtmlString.Empty"/> to allow the <see cref="Write(object)"/> call to
        /// succeed.</returns>
        /// <remarks>The method writes to the <see cref="Output"/> and the value returned is a token
        /// value that allows the Write (produced due to @RenderSection(..)) to succeed. However the
        /// value does not represent the rendered content.</remarks>
        public HtmlString RenderSection(string name, bool required)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            EnsureMethodCanBeInvoked(nameof(RenderSection));

            var task = RenderSectionAsyncCore(name, required);
            return task.GetAwaiter().GetResult();
        }

        /// <summary>
        /// In layout pages, asynchronously renders the content of the section named <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The section to render.</param>
        /// <returns>A <see cref="Task{HtmlString}"/> that on completion returns <see cref="HtmlString.Empty"/> that
        /// allows the <see cref="Write(object)"/> call to succeed.</returns>
        /// <remarks>The method writes to the <see cref="Output"/> and the value returned is a token
        /// value that allows the Write (produced due to @RenderSection(..)) to succeed. However the
        /// value does not represent the rendered content.</remarks>
        public Task<HtmlString> RenderSectionAsync(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return RenderSectionAsync(name, required: true);
        }

        /// <summary>
        /// In layout pages, asynchronously renders the content of the section named <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The section to render.</param>
        /// <param name="required">Indicates the <paramref name="name"/> section must be registered
        /// (using <c>@section</c>) in the page.</param>
        /// <returns>A <see cref="Task{HtmlString}"/> that on completion returns <see cref="HtmlString.Empty"/> that
        /// allows the <see cref="Write(object)"/> call to succeed.</returns>
        /// <remarks>The method writes to the <see cref="Output"/> and the value returned is a token
        /// value that allows the Write (produced due to @RenderSection(..)) to succeed. However the
        /// value does not represent the rendered content.</remarks>
        /// <exception cref="InvalidOperationException">if <paramref name="required"/> is <c>true</c> and the section
        /// was not registered using the <c>@section</c> in the Razor page.</exception>
        public Task<HtmlString> RenderSectionAsync(string name, bool required)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            EnsureMethodCanBeInvoked(nameof(RenderSectionAsync));
            return RenderSectionAsyncCore(name, required);
        }

        private async Task<HtmlString> RenderSectionAsyncCore(string sectionName, bool required)
        {
            if (_renderedSections.Contains(sectionName))
            {
                var message = "bleh";
                throw new InvalidOperationException(message);
            }

            RenderAsyncDelegate renderDelegate;
            if (PreviousSectionWriters.TryGetValue(sectionName, out renderDelegate))
            {
                _renderedSections.Add(sectionName);
                await renderDelegate(Output);

                // Return a token value that allows the Write call that wraps the RenderSection \ RenderSectionAsync
                // to succeed.
                return HtmlString.Empty;
            }
            else if (required)
            {
                // If the section is not found, and it is not optional, throw an error.
                var message = "bleh";
                throw new InvalidOperationException(message);
            }
            else
            {
                // If the section is optional and not found, then don't do anything.
                return null;
            }
        }

        /// <summary>
        /// In layout pages, ignores rendering the content of the section named <paramref name="sectionName"/>.
        /// </summary>
        /// <param name="sectionName">The section to ignore.</param>
        public void IgnoreSection(string sectionName)
        {
            if (sectionName == null)
            {
                throw new ArgumentNullException(nameof(sectionName));
            }

            if (!PreviousSectionWriters.ContainsKey(sectionName))
            {
                // If the section is not defined, throw an error.
                throw new InvalidOperationException("bleh");
            }

            if (_ignoredSections == null)
            {
                _ignoredSections = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            _ignoredSections.Add(sectionName);
        }

        /// <summary>
        /// Invokes <see cref="TextWriter.FlushAsync"/> on <see cref="Output"/> and <see cref="M:Stream.FlushAsync"/>
        /// on the response stream, writing out any buffered content to the <see cref="HttpResponse.Body"/>.
        /// </summary>
        /// <returns>A <see cref="Task{HtmlString}"/> that represents the asynchronous flush operation and on
        /// completion returns <see cref="HtmlString.Empty"/>.</returns>
        /// <remarks>The value returned is a token value that allows FlushAsync to work directly in an HTML
        /// section. However the value does not represent the rendered content.
        /// This method also writes out headers, so any modifications to headers must be done before
        /// <see cref="FlushAsync"/> is called. For example, call <see cref="SetAntiforgeryCookieAndHeader"/> to send
        /// antiforgery cookie token and X-Frame-Options header to client before this method flushes headers out.
        /// </remarks>
        public async Task<HtmlString> FlushAsync()
        {
            // If there are active scopes, then we should throw. Cannot flush content that has the potential to change.
            if (_tagHelperScopes.Count > 0)
            {
                throw new InvalidOperationException("nowai");
            }

            // Calls to Flush are allowed if the page does not specify a Layout or if it is executing a section in the
            // Layout.
            if (!IsLayoutBeingRendered && !string.IsNullOrEmpty(Layout))
            {
                var message = "huh?";
                throw new InvalidOperationException(message);
            }

            await Output.FlushAsync();
            await Context.Response.Body.FlushAsync();
            return HtmlString.Empty;
        }

        /// <inheritdoc />
        public void EnsureRenderedBodyOrSections()
        {
            // a) all sections defined for this page are rendered.
            // b) if no sections are defined, then the body is rendered if it's available.
            if (PreviousSectionWriters != null && PreviousSectionWriters.Count > 0)
            {
                var sectionsNotRendered = PreviousSectionWriters.Keys.Except(
                    _renderedSections,
                    StringComparer.OrdinalIgnoreCase);

                string[] sectionsNotIgnored;
                if (_ignoredSections != null)
                {
                    sectionsNotIgnored = sectionsNotRendered.Except(_ignoredSections, StringComparer.OrdinalIgnoreCase).ToArray();
                }
                else
                {
                    sectionsNotIgnored = sectionsNotRendered.ToArray();
                }

                if (sectionsNotIgnored.Length > 0)
                {
                    var sectionNames = string.Join(", ", sectionsNotIgnored);
                    throw new InvalidOperationException("herp derp");
                }
            }
            else if (BodyContent != null && !_renderedBody && !_ignoreBody)
            {
                // There are no sections defined, but RenderBody was NOT called.
                // If a body was defined and the body not ignored, then RenderBody should have been called.
                var message = "gah!";
                throw new InvalidOperationException(message);
            }
        }

        public void BeginContext(int position, int length, bool isLiteral)
        {
            const string BeginContextEvent = "Microsoft.AspNetCore.Mvc.Razor.BeginInstrumentationContext";

            if (DiagnosticSource?.IsEnabled(BeginContextEvent) == true)
            {
                DiagnosticSource.Write(
                    BeginContextEvent,
                    new
                    {
                        httpContext = Context,
                        path = Path,
                        position = position,
                        length = length,
                        isLiteral = isLiteral,
                    });
            }
        }

        public void EndContext()
        {
            const string EndContextEvent = "Microsoft.AspNetCore.Mvc.Razor.EndInstrumentationContext";

            if (DiagnosticSource?.IsEnabled(EndContextEvent) == true)
            {
                DiagnosticSource.Write(
                    EndContextEvent,
                    new
                    {
                        httpContext = Context,
                        path = Path,
                    });
            }
        }

        /// <summary>
        /// Sets antiforgery cookie and X-Frame-Options header on the response.
        /// </summary>
        /// <returns><see cref="HtmlString.Empty"/>.</returns>
        /// <remarks> Call this method to send antiforgery cookie token and X-Frame-Options header to client
        /// before <see cref="FlushAsync"/> flushes the headers. </remarks>
        public virtual HtmlString SetAntiforgeryCookieAndHeader()
        {
            var antiforgery = Context.RequestServices.GetRequiredService<IAntiforgery>();
            antiforgery.SetCookieTokenAndHeader(Context);

            return HtmlString.Empty;
        }

        private bool IsBoolFalseOrNullValue(string prefix, object value)
        {
            return string.IsNullOrEmpty(prefix) &&
                (value == null ||
                (value is bool && !(bool)value));
        }

        private bool IsBoolTrueWithEmptyPrefixValue(string prefix, object value)
        {
            // If the value is just the bool 'true', use the attribute name as the value.
            return string.IsNullOrEmpty(prefix) &&
                (value is bool && (bool)value);
        }

        private void EnsureMethodCanBeInvoked(string methodName)
        {
            if (PreviousSectionWriters == null)
            {
                throw new InvalidOperationException("hhhhhhhhh");
            }
        }

        public void Dispose()
        {
            if (Database != null)
                Database.Dispose();
        }

        private struct AttributeInfo
        {
            public AttributeInfo(
                string name,
                string prefix,
                int prefixOffset,
                string suffix,
                int suffixOffset,
                int attributeValuesCount)
            {
                Name = name;
                Prefix = prefix;
                PrefixOffset = prefixOffset;
                Suffix = suffix;
                SuffixOffset = suffixOffset;
                AttributeValuesCount = attributeValuesCount;

                Suppressed = false;
            }

            public int AttributeValuesCount { get; }

            public string Name { get; }

            public string Prefix { get; }

            public int PrefixOffset { get; }

            public string Suffix { get; }

            public int SuffixOffset { get; }

            public bool Suppressed { get; set; }
        }

        private struct TagHelperAttributeInfo
        {
            public TagHelperAttributeInfo(
                TagHelperExecutionContext tagHelperExecutionContext,
                string name,
                int attributeValuesCount,
                HtmlAttributeValueStyle attributeValueStyle)
            {
                ExecutionContext = tagHelperExecutionContext;
                Name = name;
                AttributeValuesCount = attributeValuesCount;
                AttributeValueStyle = attributeValueStyle;

                Suppressed = false;
            }

            public string Name { get; }

            public TagHelperExecutionContext ExecutionContext { get; }

            public int AttributeValuesCount { get; }

            public HtmlAttributeValueStyle AttributeValueStyle { get; }

            public bool Suppressed { get; set; }
        }

        private struct TagHelperScopeInfo
        {
            public TagHelperScopeInfo(ViewBuffer buffer, HtmlEncoder encoder, TextWriter writer)
            {
                Buffer = buffer;
                Encoder = encoder;
                Writer = writer;
            }

            public ViewBuffer Buffer { get; }

            public HtmlEncoder Encoder { get; }

            public TextWriter Writer { get; }
        }
    }
}
