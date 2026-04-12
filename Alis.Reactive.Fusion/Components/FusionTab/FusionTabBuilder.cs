using System.IO;
using System.Text.Encodings.Web;
#if NET48
using System.Web;
#else
using Microsoft.AspNetCore.Html;
#endif

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wrapper builder for the FusionTab component.
    /// Wraps SF-rendered IHtmlContent and exposes ElementId + Plan for .Reactive() chaining.
    /// Non-input component — no InputField wrapper, no label, no validation slot.
    /// </summary>
    public class FusionTabBuilder<TModel> :
#if NET48
        IHtmlString
#else
        IHtmlContent
#endif
        where TModel : class
    {
#if NET48
        private readonly IHtmlString _inner;
#else
        private readonly IHtmlContent _inner;
#endif

#if NET48
        internal FusionTabBuilder(ReactivePlan<TModel> plan, string elementId, IHtmlString inner)
#else
        internal FusionTabBuilder(ReactivePlan<TModel> plan, string elementId, IHtmlContent inner)
#endif
        {
            Plan = plan;
            ElementId = elementId;
            _inner = inner;
        }

        /// <summary>The reactive plan — used by .Reactive() to add entries.</summary>
        internal ReactivePlan<TModel> Plan { get; }

        /// <summary>The element ID — used by .Reactive() to wire events.</summary>
        internal string ElementId { get; }

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
#if NET48
            writer.Write(_inner.ToHtmlString());
#else
            _inner.WriteTo(writer, encoder);
#endif
        }
    }
}
