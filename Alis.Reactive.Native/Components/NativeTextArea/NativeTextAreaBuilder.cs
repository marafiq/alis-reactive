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
    /// Configures and renders a native HTML <c>&lt;textarea&gt;</c> element bound to a model property.
    /// </summary>
    /// <remarks>
    /// Created by the <c>.NativeTextArea()</c> factory on
    /// <see cref="InputBoundField{TModel,TProp}"/>.
    /// </remarks>
    /// <typeparam name="TModel">The view model that owns the bound property.</typeparam>
    /// <typeparam name="TProp">The model value type rendered through the textarea.</typeparam>
    public class NativeTextAreaBuilder<TModel, TProp> :
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

        private int _rows = 4;
        private string? _cssClass;
        private string? _placeholder;

        // Internal because the factory also registers this input component in the Reactive Plan.
#if NET48
        internal NativeTextAreaBuilder(
            HtmlHelper<TModel> html,
            Expression<Func<TModel, TProp>> expression,
            InputComponentRenderTarget target)
#else
        internal NativeTextAreaBuilder(
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
        /// Controls the visible row count. Defaults to 4.
        /// </summary>
        /// <param name="rows">Number of visible text rows.</param>
        public NativeTextAreaBuilder<TModel, TProp> Rows(int rows)
        {
            _rows = rows;
            return this;
        }

        /// <summary>
        /// Replaces the CSS classes on the textarea element.
        /// </summary>
        /// <param name="css">One or more CSS class names.</param>
        public NativeTextAreaBuilder<TModel, TProp> CssClass(string css)
        {
            _cssClass = css;
            return this;
        }

        /// <summary>
        /// Replaces the placeholder text shown when the textarea is empty.
        /// </summary>
        /// <param name="placeholder">The placeholder text.</param>
        public NativeTextAreaBuilder<TModel, TProp> Placeholder(string placeholder)
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
            var textAreaAttributes = new System.Collections.Generic.Dictionary<string, object>
            {
                ["id"] = _elementId,
                ["rows"] = _rows
            };
            if (_cssClass != null) textAreaAttributes["class"] = _cssClass;
            if (_placeholder != null) textAreaAttributes["placeholder"] = _placeholder;

            var textAreaHtml = _html.TextAreaFor(_expression, textAreaAttributes);
#if NET48
            writer.Write(textAreaHtml.ToHtmlString());
#else
            textAreaHtml.WriteTo(writer, HtmlEncoder.Default);
#endif
        }
    }
}
