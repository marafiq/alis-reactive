using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using Alis.Reactive;
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
    /// Configures and renders a native HTML <c>&lt;select&gt;</c> element bound to a model property.
    /// </summary>
    /// <remarks>
    /// Created by the <c>.NativeDropDown()</c> factory on
    /// <see cref="InputBoundField{TModel,TProp}"/>.
    /// </remarks>
    /// <typeparam name="TModel">The view model that owns the bound property.</typeparam>
    /// <typeparam name="TProp">The model value type represented by the selected option.</typeparam>
    public class NativeDropDownBuilder<TModel, TProp> :
#if NET48
        IHtmlString
    {
        private readonly HtmlHelper<TModel> _html;
#else
        IHtmlContent
    {
        private readonly IHtmlHelper<TModel> _html;
#endif
        private readonly Expression<Func<TModel, TProp>> _expression;
        private readonly string _elementId;
        private readonly string _bindingPath;

        private IEnumerable<SelectListItem>? _items;
        private string? _placeholder;
        private bool _enabled = true;
        private string? _cssClass;

        // Internal because the factory also registers this input component in the Reactive Plan.
        internal NativeDropDownBuilder(
#if NET48
            HtmlHelper<TModel> html,
#else
            IHtmlHelper<TModel> html,
#endif
            Expression<Func<TModel, TProp>> expression,
            InputComponentRenderTarget target)
        {
            _html = html;
            _expression = expression;
            if (target == null) throw new ArgumentNullException(nameof(target));
            _elementId = target.ElementId;
            _bindingPath = target.BindingName;
        }

        internal string ElementId => _elementId;

        internal string BindingPath => _bindingPath;

        /// <summary>
        /// Replaces the selectable options.
        /// </summary>
        /// <param name="items">The list of options to display.</param>
        public NativeDropDownBuilder<TModel, TProp> Items(IEnumerable<SelectListItem> items)
        {
            _items = items;
            return this;
        }

        /// <summary>
        /// Replaces the empty-selection placeholder label, such as <c>"-- Select --"</c>.
        /// </summary>
        /// <param name="optionLabel">The placeholder text for the empty option.</param>
        public NativeDropDownBuilder<TModel, TProp> Placeholder(string optionLabel)
        {
            _placeholder = optionLabel;
            return this;
        }

        /// <summary>
        /// Enables or disables the dropdown. Defaults to enabled.
        /// </summary>
        /// <param name="enabled"><see langword="true"/> to enable, <see langword="false"/> to disable.</param>
        public NativeDropDownBuilder<TModel, TProp> Enabled(bool enabled)
        {
            _enabled = enabled;
            return this;
        }

        /// <summary>
        /// Replaces the CSS classes on the select element.
        /// </summary>
        /// <param name="css">One or more CSS class names.</param>
        public NativeDropDownBuilder<TModel, TProp> CssClass(string css)
        {
            _cssClass = css;
            return this;
        }

#if NET48
        /// <inheritdoc />
        public string ToHtmlString()
        {
            var sw = new StringWriter();
            WriteTo(sw, HtmlEncoder.Default);
            return sw.ToString();
        }
#endif

        /// <inheritdoc />
        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var selectAttributes = new Dictionary<string, object> { ["id"] = _elementId };
            if (!_enabled) selectAttributes["disabled"] = "disabled";
            if (_cssClass != null) selectAttributes["class"] = _cssClass;

            var dropdownHtml = _html.DropDownListFor(
                _expression,
                _items ?? Enumerable.Empty<SelectListItem>(),
                _placeholder,
                selectAttributes);
#if NET48
            writer.Write(dropdownHtml.ToHtmlString());
#else
            dropdownHtml.WriteTo(writer, HtmlEncoder.Default);
#endif
        }
    }

}
