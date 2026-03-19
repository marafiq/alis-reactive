using System;
using System.IO;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Renders a native HTML &lt;textarea&gt; element bound to a model property.
    /// Uses IdGenerator for element ID and MVC NameFor for the name attribute.
    /// </summary>
    public class NativeTextAreaBuilder<TModel, TProp> : IHtmlContent
    {
        private readonly IHtmlHelper<TModel> _html;
        private readonly Expression<Func<TModel, TProp>> _expression;
        private readonly string _elementId;
        private readonly string _bindingPath;

        private int _rows = 4;
        private string? _cssClass;
        private string? _placeholder;

        internal NativeTextAreaBuilder(IHtmlHelper<TModel> html, Expression<Func<TModel, TProp>> expression)
        {
            _html = html;
            _expression = expression;
            _elementId = IdGenerator.For<TModel, TProp>(expression);
            _bindingPath = html.NameFor(expression);
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

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var attrs = new System.Collections.Generic.Dictionary<string, object>
            {
                ["id"] = _elementId,
                ["rows"] = _rows
            };
            if (_cssClass != null) attrs["class"] = _cssClass;
            if (_placeholder != null) attrs["placeholder"] = _placeholder;

            var result = _html.TextAreaFor(_expression, attrs);
            result.WriteTo(writer, HtmlEncoder.Default);
        }
    }
}
