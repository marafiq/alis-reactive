using System;
using System.IO;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Renders a native HTML &lt;input type="hidden"&gt; element bound to a model property.
    /// Uses IdGenerator for element ID and MVC NameFor for the name attribute.
    /// No label, no validation slot — hidden inputs are invisible.
    /// </summary>
    public class NativeHiddenFieldBuilder<TModel, TProp> : IHtmlContent
    {
        private readonly IHtmlHelper<TModel> _html;
        private readonly Expression<Func<TModel, TProp>> _expression;
        private readonly string _elementId;
        private readonly string _bindingPath;

        internal NativeHiddenFieldBuilder(IHtmlHelper<TModel> html, Expression<Func<TModel, TProp>> expression)
        {
            _html = html;
            _expression = expression;
            _elementId = IdGenerator.For<TModel, TProp>(expression);
            _bindingPath = html.NameFor(expression);
        }

        internal string ElementId => _elementId;
        internal string BindingPath => _bindingPath;

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var attrs = new System.Collections.Generic.Dictionary<string, object>
            {
                ["id"] = _elementId
            };

            var result = _html.HiddenFor(_expression, attrs);
            result.WriteTo(writer, HtmlEncoder.Default);
        }
    }
}
