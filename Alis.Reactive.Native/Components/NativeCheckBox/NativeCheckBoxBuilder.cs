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
    /// Configures and renders a native HTML <c>&lt;input type="checkbox"&gt;</c> bound to a model property.
    /// </summary>
    /// <remarks>
    /// Created by the <c>.NativeCheckBox()</c> factory on
    /// <see cref="InputBoundField{TModel,TProp}"/>.
    /// </remarks>
    /// <typeparam name="TModel">The view model type.</typeparam>
    /// <typeparam name="TProp">The bound property type (typically <see cref="bool"/>).</typeparam>
    public class NativeCheckBoxBuilder<TModel, TProp> : IHtmlContent
        where TModel : class
    {
        private readonly IHtmlHelper<TModel> _html;
        private readonly Expression<Func<TModel, bool>> _expression;
        private readonly string _elementId;
        private readonly string _bindingPath;

        private string? _cssClass;

        // NEVER make public — devs create builders via the .NativeCheckBox() factory,
        // which also registers the component in the plan's ComponentsMap.
        internal NativeCheckBoxBuilder(IHtmlHelper<TModel> html, Expression<Func<TModel, bool>> expression)
        {
            _html = html;
            _expression = expression;
            _elementId = IdGenerator.For<TModel, bool>(expression);
            _bindingPath = html.NameFor(expression);
        }

        /// <summary>Gets the resolved element ID for this checkbox.</summary>
        internal string ElementId => _elementId;

        /// <summary>Gets the model binding path (e.g. <c>"IsActive"</c>).</summary>
        internal string BindingPath => _bindingPath;

        /// <summary>
        /// Adds CSS classes to the checkbox element.
        /// </summary>
        /// <param name="css">One or more CSS class names.</param>
        /// <returns>The builder for method chaining.</returns>
        public NativeCheckBoxBuilder<TModel, TProp> CssClass(string css)
        {
            _cssClass = css;
            return this;
        }

        /// <inheritdoc />
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
