using System;
using System.IO;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using Alis.Reactive;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Renders a native HTML &lt;input type="hidden"&gt; element bound to a model property.
    /// Uses the plan-owned render target for element ID and MVC binding name.
    /// No label, no validation slot — hidden inputs are invisible.
    /// </summary>
    public class NativeHiddenFieldBuilder<TModel, TProp> :
        IHtmlContent
    {
        private readonly IHtmlHelper<TModel> _html;
        private readonly Expression<Func<TModel, TProp>> _expression;
        private readonly string _elementId;
        private readonly string _bindingPath;

        internal NativeHiddenFieldBuilder(
            IHtmlHelper<TModel> html,
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


        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var attrs = new System.Collections.Generic.Dictionary<string, object>
            {
                ["id"] = _elementId
            };

            var result = _html.HiddenFor(_expression, attrs);
            result.WriteTo(writer, HtmlEncoder.Default);
        }
    }
}
