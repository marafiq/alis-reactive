using System;
using System.IO;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Configures and renders a native HTML <c>&lt;textarea&gt;</c> element bound to a model property.
    /// </summary>
    /// <remarks>
    /// Created by the <c>.NativeTextArea()</c> factory on
    /// <see cref="InputBoundField{TModel,TProp}"/>.
    /// </remarks>
    /// <typeparam name="TModel">The view model type.</typeparam>
    /// <typeparam name="TProp">The bound property type.</typeparam>
    public class NativeTextAreaBuilder<TModel, TProp> : IHtmlContent
    {
        private readonly IHtmlHelper<TModel> _html;
        private readonly Expression<Func<TModel, TProp>> _expression;
        private readonly string _elementId;
        private readonly string _bindingPath;

        private int _rows = 4;
        private string? _cssClass;
        private string? _placeholder;

        // NEVER make public — devs create builders via the .NativeTextArea() factory,
        // which also registers the component in the plan's ComponentsMap.
        internal NativeTextAreaBuilder(IHtmlHelper<TModel> html, Expression<Func<TModel, TProp>> expression)
        {
            _html = html;
            _expression = expression;
            _elementId = IdGenerator.For<TModel, TProp>(expression);
            _bindingPath = html.NameFor(expression);
        }

        /// <summary>Gets the resolved element ID for this textarea.</summary>
        internal string ElementId => _elementId;

        /// <summary>Gets the model binding path (e.g. <c>"CareNotes"</c>).</summary>
        internal string BindingPath => _bindingPath;

        /// <summary>
        /// Sets the visible row count. Defaults to 4.
        /// </summary>
        /// <param name="rows">Number of visible text rows.</param>
        /// <returns>The builder for method chaining.</returns>
        public NativeTextAreaBuilder<TModel, TProp> Rows(int rows)
        {
            _rows = rows;
            return this;
        }

        /// <summary>
        /// Adds CSS classes to the textarea element.
        /// </summary>
        /// <param name="css">One or more CSS class names.</param>
        /// <returns>The builder for method chaining.</returns>
        public NativeTextAreaBuilder<TModel, TProp> CssClass(string css)
        {
            _cssClass = css;
            return this;
        }

        /// <summary>
        /// Sets the placeholder text shown when the textarea is empty.
        /// </summary>
        /// <param name="placeholder">The placeholder text.</param>
        /// <returns>The builder for method chaining.</returns>
        public NativeTextAreaBuilder<TModel, TProp> Placeholder(string placeholder)
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
                ["rows"] = _rows
            };
            if (_cssClass != null) attrs["class"] = _cssClass;
            if (_placeholder != null) attrs["placeholder"] = _placeholder;

            var result = _html.TextAreaFor(_expression, attrs);
            result.WriteTo(writer, HtmlEncoder.Default);
        }
    }
}
