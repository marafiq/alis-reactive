using System;
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
    /// Renders a native HTML &lt;input type="hidden"&gt; element bound to a model property.
    /// Uses IdGenerator for element ID and MVC NameFor for the name attribute.
    /// No label, no validation slot — hidden inputs are invisible.
    /// </summary>
#if NET48
    public class NativeHiddenFieldBuilder<TModel, TProp> : IHtmlString
    {
        private readonly HtmlHelper<TModel> _html;
#else
    public class NativeHiddenFieldBuilder<TModel, TProp> : IHtmlContent
    {
        private readonly IHtmlHelper<TModel> _html;
#endif
        private readonly Expression<Func<TModel, TProp>> _expression;
        private readonly string _elementId;
        private readonly string _bindingPath;

#if NET48
        internal NativeHiddenFieldBuilder(HtmlHelper<TModel> html, Expression<Func<TModel, TProp>> expression)
#else
        internal NativeHiddenFieldBuilder(IHtmlHelper<TModel> html, Expression<Func<TModel, TProp>> expression)
#endif
        {
            _html = html;
            _expression = expression;
            _elementId = IdGenerator.For<TModel, TProp>(expression);
#if NET48
            _bindingPath = ExpressionHelper.GetExpressionText(expression);
#else
            _bindingPath = html.NameFor(expression);
#endif
        }

        internal string ElementId => _elementId;
        internal string BindingPath => _bindingPath;

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
            var attrs = new System.Collections.Generic.Dictionary<string, object>
            {
                ["id"] = _elementId
            };

#if NET48
            var result = System.Web.Mvc.Html.InputExtensions.HiddenFor(_html, _expression, attrs);
            writer.Write(result.ToHtmlString());
#else
            var result = _html.HiddenFor(_expression, attrs);
            result.WriteTo(writer, HtmlEncoder.Default);
#endif
        }
    }
}
