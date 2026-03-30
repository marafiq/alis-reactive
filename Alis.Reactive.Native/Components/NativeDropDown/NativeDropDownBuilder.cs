using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Configures and renders a native HTML <c>&lt;select&gt;</c> element bound to a model property.
    /// </summary>
    /// <remarks>
    /// Created by the <c>.NativeDropDown()</c> factory on
    /// <see cref="InputBoundField{TModel,TProp}"/>.
    /// <code>
    /// Html.InputField(plan, m => m.Status, o => o.Required().Label("Status"))
    ///     .NativeDropDown(b => b
    ///         .Items(statusItems)
    ///         .Placeholder("-- Select --")
    ///         .Reactive(plan, evt => evt.Changed, (args, p) => { ... }));
    /// </code>
    /// </remarks>
    /// <typeparam name="TModel">The view model type.</typeparam>
    /// <typeparam name="TProp">The bound property type.</typeparam>
    public class NativeDropDownBuilder<TModel, TProp> : IHtmlContent
    {
        private readonly IHtmlHelper<TModel> _html;
        private readonly Expression<Func<TModel, TProp>> _expression;
        private readonly string _elementId;
        private readonly string _bindingPath;

        private IEnumerable<SelectListItem>? _items;
        private string? _placeholder;
        private bool _enabled = true;
        private string? _cssClass;

        // NEVER make public — devs create builders via the .NativeDropDown() factory,
        // which also registers the component in the plan's ComponentsMap.
        internal NativeDropDownBuilder(IHtmlHelper<TModel> html, Expression<Func<TModel, TProp>> expression)
        {
            _html = html;
            _expression = expression;
            _elementId = IdGenerator.For<TModel, TProp>(expression);
            _bindingPath = html.NameFor(expression);
        }

        /// <summary>Gets the resolved element ID for this dropdown.</summary>
        internal string ElementId => _elementId;

        /// <summary>Gets the model binding path (e.g. <c>"Address.City"</c>).</summary>
        internal string BindingPath => _bindingPath;

        /// <summary>
        /// Sets the selectable options.
        /// </summary>
        /// <param name="items">The list of options to display.</param>
        /// <returns>The builder for method chaining.</returns>
        public NativeDropDownBuilder<TModel, TProp> Items(IEnumerable<SelectListItem> items)
        {
            _items = items;
            return this;
        }

        /// <summary>
        /// Sets the empty-selection placeholder label (e.g. <c>"-- Select --"</c>).
        /// </summary>
        /// <param name="optionLabel">The placeholder text for the empty option.</param>
        /// <returns>The builder for method chaining.</returns>
        public NativeDropDownBuilder<TModel, TProp> Placeholder(string optionLabel)
        {
            _placeholder = optionLabel;
            return this;
        }

        /// <summary>
        /// Enables or disables the dropdown. Defaults to enabled.
        /// </summary>
        /// <param name="enabled"><see langword="true"/> to enable, <see langword="false"/> to disable.</param>
        /// <returns>The builder for method chaining.</returns>
        public NativeDropDownBuilder<TModel, TProp> Enabled(bool enabled)
        {
            _enabled = enabled;
            return this;
        }

        /// <summary>
        /// Adds CSS classes to the select element.
        /// </summary>
        /// <param name="css">One or more CSS class names.</param>
        /// <returns>The builder for method chaining.</returns>
        public NativeDropDownBuilder<TModel, TProp> CssClass(string css)
        {
            _cssClass = css;
            return this;
        }

        /// <inheritdoc />
        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var attrs = new Dictionary<string, object> { ["id"] = _elementId };
            if (!_enabled) attrs["disabled"] = "disabled";
            if (_cssClass != null) attrs["class"] = _cssClass;

            var result = _html.DropDownListFor(
                _expression,
                _items ?? Enumerable.Empty<SelectListItem>(),
                _placeholder,
                attrs);
            result.WriteTo(writer, HtmlEncoder.Default);
        }
    }

}
