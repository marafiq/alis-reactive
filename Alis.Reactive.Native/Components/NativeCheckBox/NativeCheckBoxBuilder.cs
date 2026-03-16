using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Renders a native HTML &lt;input type="checkbox"&gt; bound to a model property.
    /// Uses IdGenerator for element ID and MVC NameFor for the name attribute.
    /// </summary>
    public class NativeCheckBoxBuilder<TModel, TProp> : IHtmlContent
        where TModel : class
    {
        private readonly IHtmlHelper<TModel> _html;
        private readonly Expression<Func<TModel, bool>> _expression;
        private readonly string _elementId;
        private readonly string _bindingPath;

        private string? _cssClass;

        internal NativeCheckBoxBuilder(IHtmlHelper<TModel> html, Expression<Func<TModel, bool>> expression)
        {
            _html = html;
            _expression = expression;
            _elementId = IdGenerator.For<TModel, bool>(expression);
            _bindingPath = html.NameFor(expression);
        }

        internal string ElementId => _elementId;
        internal string BindingPath => _bindingPath;

        public NativeCheckBoxBuilder<TModel, TProp> CssClass(string css)
        {
            _cssClass = css;
            return this;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var attrs = new Dictionary<string, object>
            {
                ["id"] = _elementId
            };
            if (_cssClass != null) attrs["class"] = _cssClass;

            var result = _html.CheckBoxFor(_expression, attrs);
            result.WriteTo(writer, HtmlEncoder.Default);
        }
    }
}
