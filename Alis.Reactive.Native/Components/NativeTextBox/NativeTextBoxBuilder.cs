using System;
using System.IO;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Configures and renders a native HTML <c>&lt;input&gt;</c> element bound to a model property.
    /// </summary>
    /// <remarks>
    /// Supports <c>type="text"</c> (default), <c>"number"</c>, <c>"email"</c>,
    /// <c>"password"</c>, etc. Created by the <c>.NativeTextBox()</c> factory on
    /// <see cref="InputBoundField{TModel,TProp}"/>.
    /// </remarks>
    /// <typeparam name="TModel">The view model type.</typeparam>
    /// <typeparam name="TProp">The bound property type.</typeparam>
    public class NativeTextBoxBuilder<TModel, TProp> : IHtmlContent
    {
        private readonly IHtmlHelper<TModel> _html;
        private readonly Expression<Func<TModel, TProp>> _expression;
        private readonly string _elementId;
        private readonly string _bindingPath;

        private string _type = "text";
        private string? _cssClass;
        private string? _placeholder;

        // NEVER make public — devs create builders via the .NativeTextBox() factory,
        // which also registers the component in the plan's ComponentsMap.
        internal NativeTextBoxBuilder(IHtmlHelper<TModel> html, Expression<Func<TModel, TProp>> expression)
        {
            _html = html;
            _expression = expression;
            _elementId = IdGenerator.For<TModel, TProp>(expression);
            _bindingPath = html.NameFor(expression);
        }

        /// <summary>Gets the resolved element ID for this input.</summary>
        internal string ElementId => _elementId;

        /// <summary>Gets the model binding path (e.g. <c>"Address.City"</c>).</summary>
        internal string BindingPath => _bindingPath;

        /// <summary>
        /// Sets the HTML input type (e.g. <c>"email"</c>, <c>"password"</c>, <c>"number"</c>).
        /// Defaults to <c>"text"</c>.
        /// </summary>
        /// <param name="type">The HTML input type attribute value.</param>
        /// <returns>The builder for method chaining.</returns>
        public NativeTextBoxBuilder<TModel, TProp> Type(string type)
        {
            _type = type;
            return this;
        }

        /// <summary>
        /// Adds CSS classes to the input element.
        /// </summary>
        /// <param name="css">One or more CSS class names.</param>
        /// <returns>The builder for method chaining.</returns>
        public NativeTextBoxBuilder<TModel, TProp> CssClass(string css)
        {
            _cssClass = css;
            return this;
        }

        /// <summary>
        /// Sets the placeholder text shown when the input is empty.
        /// </summary>
        /// <param name="placeholder">The placeholder text.</param>
        /// <returns>The builder for method chaining.</returns>
        public NativeTextBoxBuilder<TModel, TProp> Placeholder(string placeholder)
        {
            _placeholder = placeholder;
            return this;
        }

        /// <inheritdoc />
        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var attrs = new System.Collections.Generic.Dictionary<string, object>
            {
                ["id"] = _elementId,
                ["type"] = _type
            };
            if (_cssClass != null) attrs["class"] = _cssClass;
            if (_placeholder != null) attrs["placeholder"] = _placeholder;

            var result = _html.TextBoxFor(_expression, attrs);
            result.WriteTo(writer, HtmlEncoder.Default);
        }
    }

}
