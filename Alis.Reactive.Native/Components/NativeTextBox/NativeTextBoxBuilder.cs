using System;
using System.IO;
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
    /// Configures and renders a native HTML <c>&lt;input&gt;</c> element bound to a model property.
    /// </summary>
    /// <remarks>
    /// Supports <c>type="text"</c> (default), <c>"number"</c>, <c>"email"</c>,
    /// <c>"password"</c>, etc. Created by the <c>.NativeTextBox()</c> factory on
    /// <see cref="InputBoundField{TModel,TProp}"/>.
    /// </remarks>
    /// <typeparam name="TModel">The view model that owns the bound property.</typeparam>
    /// <typeparam name="TProp">The model value type rendered through the text input.</typeparam>
    public class NativeTextBoxBuilder<TModel, TProp> :
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

        private string _type = "text";
        private string? _cssClass;
        private string? _placeholder;

        // Internal because the factory also registers this input component in the Reactive Plan.
#if NET48
        internal NativeTextBoxBuilder(
            HtmlHelper<TModel> html,
            Expression<Func<TModel, TProp>> expression,
            InputComponentRenderTarget target)
#else
        internal NativeTextBoxBuilder(
            IHtmlHelper<TModel> html,
            Expression<Func<TModel, TProp>> expression,
            InputComponentRenderTarget target)
#endif
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
        /// Sets the HTML input type (e.g. <c>"email"</c>, <c>"password"</c>, <c>"number"</c>).
        /// Defaults to <c>"text"</c>.
        /// </summary>
        /// <param name="type">The HTML <c>type</c> attribute value.</param>
        public NativeTextBoxBuilder<TModel, TProp> Type(string type)
        {
            _type = type;
            return this;
        }

        /// <summary>
        /// Adds CSS classes to the input element.
        /// </summary>
        /// <param name="css">One or more CSS class names.</param>
        public NativeTextBoxBuilder<TModel, TProp> CssClass(string css)
        {
            _cssClass = css;
            return this;
        }

        /// <summary>
        /// Sets the placeholder text shown when the input is empty.
        /// </summary>
        /// <param name="placeholder">The placeholder text.</param>
        public NativeTextBoxBuilder<TModel, TProp> Placeholder(string placeholder)
        {
            _placeholder = placeholder;
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
            var inputAttributes = new System.Collections.Generic.Dictionary<string, object>
            {
                ["id"] = _elementId,
                ["type"] = _type
            };
            if (_cssClass != null) inputAttributes["class"] = _cssClass;
            if (_placeholder != null) inputAttributes["placeholder"] = _placeholder;

            var textBoxHtml = _html.TextBoxFor(_expression, inputAttributes);
#if NET48
            writer.Write(textBoxHtml.ToHtmlString());
#else
            textBoxHtml.WriteTo(writer, HtmlEncoder.Default);
#endif
        }
    }

}
