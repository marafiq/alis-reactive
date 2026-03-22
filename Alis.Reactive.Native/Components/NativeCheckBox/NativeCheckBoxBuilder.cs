using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
#if NET48
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Renders a native HTML &lt;input type="checkbox"&gt; bound to a model property.
    /// Uses IdGenerator for element ID and MVC NameFor for the name attribute.
    /// </summary>
#if NET48
    public class NativeCheckBoxBuilder<TModel, TProp> : IHtmlString
        where TModel : class
    {
        private readonly HtmlHelper<TModel> _html;
#else
    public class NativeCheckBoxBuilder<TModel, TProp> : IHtmlContent
        where TModel : class
    {
        private readonly IHtmlHelper<TModel> _html;
#endif
        private readonly Expression<Func<TModel, bool>> _expression;
        private readonly string _elementId;
        private readonly string _bindingPath;

        private string? _cssClass;

#if NET48
        internal NativeCheckBoxBuilder(HtmlHelper<TModel> html, Expression<Func<TModel, bool>> expression)
#else
        internal NativeCheckBoxBuilder(IHtmlHelper<TModel> html, Expression<Func<TModel, bool>> expression)
#endif
        {
            _html = html;
            _expression = expression;
            _elementId = IdGenerator.For<TModel, bool>(expression);
#if NET48
            _bindingPath = ExpressionHelper.GetExpressionText(expression);
#else
            _bindingPath = html.NameFor(expression);
#endif
        }

        internal string ElementId => _elementId;
        internal string BindingPath => _bindingPath;

        public NativeCheckBoxBuilder<TModel, TProp> CssClass(string css)
        {
            _cssClass = css;
            return this;
        }

#if NET48
        public string ToHtmlString()
        {
            using (var sw = new StringWriter())
            {
                WriteTo(sw, HtmlEncoder.Default);
                return sw.ToString();
            }
        }
#endif

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var attrs = new Dictionary<string, object>
            {
                ["id"] = _elementId
            };
            if (_cssClass != null) attrs["class"] = _cssClass;

#if NET48
            var result = System.Web.Mvc.Html.InputExtensions.CheckBoxFor(_html, _expression, attrs);
            writer.Write(result.ToHtmlString());
#else
            var result = _html.CheckBoxFor(_expression, attrs);
            result.WriteTo(writer, HtmlEncoder.Default);
#endif
        }
    }
}
