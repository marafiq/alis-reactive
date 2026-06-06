using System;
using System.Collections.Generic;
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
    /// Renders a native HTML <c>&lt;input type="hidden"&gt;</c> bound to a model property.
    /// </summary>
    /// <remarks>
    /// Uses the Reactive Plan-owned render target for element ID and MVC binding name.
    /// No label or validation slot is rendered.
    /// </remarks>
    /// <typeparam name="TModel">View model that owns the bound property.</typeparam>
    /// <typeparam name="TProp">Model value type rendered through the hidden input.</typeparam>
    public class NativeHiddenFieldBuilder<TModel, TProp> :
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

        internal NativeHiddenFieldBuilder(
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
            var hiddenInputAttributes = new Dictionary<string, object>
            {
                ["id"] = _elementId
            };

            var hiddenInputHtml = _html.HiddenFor(_expression, hiddenInputAttributes);
#if NET48
            writer.Write(hiddenInputHtml.ToHtmlString());
#else
            hiddenInputHtml.WriteTo(writer, HtmlEncoder.Default);
#endif
        }
    }
}
