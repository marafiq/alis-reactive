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
    /// Configures and renders a native HTML <c>&lt;input type="checkbox"&gt;</c> bound to a model property.
    /// </summary>
    /// <remarks>
    /// Created by the <c>.NativeCheckBox()</c> factory on
    /// <see cref="InputBoundField{TModel,TProp}"/>.
    /// </remarks>
    /// <typeparam name="TModel">The view model that owns the bound property.</typeparam>
    /// <typeparam name="TProp">The model value type represented by the checked state, typically <see cref="bool"/>.</typeparam>
    public class NativeCheckBoxBuilder<TModel, TProp> :
#if NET48
        IHtmlString
#else
        IHtmlContent
#endif
        where TModel : class
    {
#if NET48
        private readonly HtmlHelper<TModel> _html;
#else
        private readonly IHtmlHelper<TModel> _html;
#endif
        private readonly Expression<Func<TModel, bool>> _expression;
        private readonly string _elementId;
        private readonly string _bindingPath;

        private string? _cssClass;

        // Internal because the factory also registers this input component in the Reactive Plan.
        internal NativeCheckBoxBuilder(
#if NET48
            HtmlHelper<TModel> html,
#else
            IHtmlHelper<TModel> html,
#endif
            Expression<Func<TModel, bool>> expression,
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
        /// Replaces the CSS classes on the checkbox element.
        /// </summary>
        /// <param name="css">One or more CSS class names.</param>
        public NativeCheckBoxBuilder<TModel, TProp> CssClass(string css)
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
            var checkboxAttributes = new Dictionary<string, object>
            {
                ["id"] = _elementId
            };
            if (_cssClass != null) checkboxAttributes["class"] = _cssClass;

            var checkboxHtml = _html.CheckBoxFor(_expression, checkboxAttributes);
#if NET48
            writer.Write(checkboxHtml.ToHtmlString());
#else
            checkboxHtml.WriteTo(writer, HtmlEncoder.Default);
#endif
        }
    }
}
