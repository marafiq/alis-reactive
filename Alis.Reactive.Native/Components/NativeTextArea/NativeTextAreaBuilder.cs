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
    /// Renders a native HTML &lt;textarea&gt; element bound to a model property.
    /// Uses IdGenerator for element ID and MVC NameFor for the name attribute.
    /// </summary>
#if NET48
    public class NativeTextAreaBuilder<TModel, TProp> : IHtmlString
    {
        private readonly HtmlHelper<TModel> _html;
#else
    public class NativeTextAreaBuilder<TModel, TProp> : IHtmlContent
    {
        private readonly IHtmlHelper<TModel> _html;
#endif
        private readonly Expression<Func<TModel, TProp>> _expression;
        private readonly string _elementId;
        private readonly string _bindingPath;

        private int _rows = 4;
        private string? _cssClass;
        private string? _placeholder;

#if NET48
        internal NativeTextAreaBuilder(HtmlHelper<TModel> html, Expression<Func<TModel, TProp>> expression)
#else
        internal NativeTextAreaBuilder(IHtmlHelper<TModel> html, Expression<Func<TModel, TProp>> expression)
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

        public NativeTextAreaBuilder<TModel, TProp> Rows(int rows)
        {
            _rows = rows;
            return this;
        }

        public NativeTextAreaBuilder<TModel, TProp> CssClass(string css)
        {
            _cssClass = css;
            return this;
        }

        public NativeTextAreaBuilder<TModel, TProp> Placeholder(string placeholder)
        {
            _placeholder = placeholder;
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
            var attrs = new System.Collections.Generic.Dictionary<string, object>
            {
                ["id"] = _elementId,
                ["rows"] = _rows
            };
            if (_cssClass != null) attrs["class"] = _cssClass;
            if (_placeholder != null) attrs["placeholder"] = _placeholder;

#if NET48
            var result = System.Web.Mvc.Html.TextAreaExtensions.TextAreaFor(_html, _expression, attrs);
            writer.Write(result.ToHtmlString());
#else
            var result = _html.TextAreaFor(_expression, attrs);
            result.WriteTo(writer, HtmlEncoder.Default);
#endif
        }
    }
}
