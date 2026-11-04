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

namespace Alis.Reactive.Native.Components.NativeHiddenField
{
    internal sealed class NativeHiddenFieldBuilder<TModel, TProp> :
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

        internal NativeHiddenFieldBuilder(
#if NET48
            HtmlHelper<TModel> html,
#else
            IHtmlHelper<TModel> html,
#endif
            Expression<Func<TModel, TProp>> expression,
            string elementId)
        {
            _html = html;
            _expression = expression;
            _elementId = elementId;
        }

#if NET48
        public string ToHtmlString()
        {
            var sw = new StringWriter();
            WriteTo(sw, HtmlEncoder.Default);
            return sw.ToString();
        }
#endif

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var hiddenInputAttributes = new Dictionary<string, object>
            {
                ["id"] = _elementId,
                ["data-alis-hidden-field"] = ""
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
