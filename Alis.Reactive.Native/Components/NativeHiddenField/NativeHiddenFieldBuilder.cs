using System;
using System.IO;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Builds a native hidden input bound to a model property.
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

        /// <summary>Gets the rendered element id used for event wiring.</summary>
        internal string ElementId => _elementId;

        /// <summary>Gets the model binding path used for reads and gathers.</summary>
        internal string BindingPath => _bindingPath;

        /// <summary>Writes the hidden input markup.</summary>
        /// <param name="writer">The writer that receives the markup.</param>
        /// <param name="encoder">The encoder used for HTML output.</param>
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
