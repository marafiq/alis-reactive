using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
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
    /// <typeparam name="TModel">The view model type.</typeparam>
    /// <typeparam name="TProp">The bound property type (typically <see cref="bool"/>).</typeparam>
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

        // NEVER make public — devs create builders via the .NativeCheckBox() factory,
        // which also registers the component in the plan's ComponentsMap.
#if NET48
        internal NativeCheckBoxBuilder(HtmlHelper<TModel> html, Expression<Func<TModel, bool>> expression)
#else
        internal NativeCheckBoxBuilder(IHtmlHelper<TModel> html, Expression<Func<TModel, bool>> expression)
#endif
        {
            _html = html;
            _expression = expression;
            _elementId = IdGenerator.For<TModel, bool>(expression);
#if NET48
            _bindingPath = ExpressionHelper.GetExpressionText(expression);
#else
            _bindingPath = html.NameFor(expression);
#endif
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
            var attrs = new Dictionary<string, object>
            {
                ["id"] = _elementId
            };
            if (_cssClass != null) attrs["class"] = _cssClass;

            var result = _html.CheckBoxFor(_expression, attrs);
#if NET48
            writer.Write(result.ToHtmlString());
#else
            result.WriteTo(writer, HtmlEncoder.Default);
#endif
        }
    }
}
