using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using Alis.Reactive;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Renders a native HTML &lt;select&gt; element bound to a model property.
    /// Follows the same CRTP-style pattern as DesignSystem native input builders.
    ///
    /// Usage:
    ///   Html.Field("Status", true, m => m.Status, expr =>
    ///       Html.NativeDropDownFor(expr)
    ///           .Items(statusItems)
    ///           .Placeholder("-- Select --")
    ///           .Reactive(plan, evt => evt.Changed, (args, p) => { ... })
    ///   );
    ///
    /// .Reactive() is always the last call (native has no .Render()).
    /// </summary>
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

        public NativeDropDownBuilder(IHtmlHelper<TModel> html, Expression<Func<TModel, TProp>> expression)
        {
            _html = html;
            _expression = expression;
            _elementId = IdGenerator.For<TModel, TProp>(expression);
            _bindingPath = html.NameFor(expression).ToString();
        }

        /// <summary>The resolved element ID — used by .Reactive() to wire events.</summary>
        internal string ElementId => _elementId;

        /// <summary>The dot-notation model binding path (e.g. "Address.City") for future HTTP gather.</summary>
        internal string BindingPath => _bindingPath;

        /// <summary>Sets the selectable options.</summary>
        public NativeDropDownBuilder<TModel, TProp> Items(IEnumerable<SelectListItem> items)
        {
            _items = items;
            return this;
        }

        /// <summary>Sets the empty-selection option label (e.g. "-- Select --").</summary>
        public NativeDropDownBuilder<TModel, TProp> Placeholder(string optionLabel)
        {
            _placeholder = optionLabel;
            return this;
        }

        /// <summary>Sets the disabled state (default: enabled).</summary>
        public NativeDropDownBuilder<TModel, TProp> Enabled(bool enabled)
        {
            _enabled = enabled;
            return this;
        }

        /// <summary>Appends additional CSS classes on the select element.</summary>
        public NativeDropDownBuilder<TModel, TProp> CssClass(string css)
        {
            _cssClass = css;
            return this;
        }

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

    /// <summary>
    /// Factory extension for creating NativeDropDownBuilder bound to a model property.
    /// </summary>
    public static class NativeDropDownHtmlExtensions
    {
        private static readonly NativeDropDown _component = new NativeDropDown();

        /// <summary>
        /// Creates a native &lt;select&gt; builder bound to a model property.
        /// </summary>
        public static NativeDropDownBuilder<TModel, TProp> NativeDropDownFor<TModel, TProp>(
            this IHtmlHelper<TModel> html,
            IReactivePlan<TModel> plan,
            Expression<Func<TModel, TProp>> expression)
            where TModel : class
        {
            var uniqueId = IdGenerator.For<TModel, TProp>(expression);
            var name = html.NameFor(expression).ToString();

            plan.AddToComponentsMap(name, new ComponentRegistration(
                uniqueId,
                _component.Vendor,
                name,
                _component.ReadExpr));

            return new NativeDropDownBuilder<TModel, TProp>(html, expression);
        }
    }
}
