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
    /// Wrapper builder for the Syncfusion Tab component.
    /// Wraps SF-rendered IHtmlContent and exposes ElementId + Plan for .Reactive() chaining.
    /// Non-input component — no InputField wrapper, no label, no validation slot.
    /// </summary>
#if NET48
    public class FusionTabBuilder<TModel> : IHtmlString where TModel : class
    {
        private readonly IHtmlString _inner;

        internal FusionTabBuilder(IReactivePlan<TModel> plan, string elementId, IHtmlString inner)
#else
    public class FusionTabBuilder<TModel> : IHtmlContent where TModel : class
    {
        private readonly IHtmlContent _inner;

        internal FusionTabBuilder(IReactivePlan<TModel> plan, string elementId, IHtmlContent inner)
#endif
        {
            Plan = plan;
            ElementId = elementId;
            _inner = inner;
        }

        /// <summary>The reactive plan — used by .Reactive() to add entries.</summary>
        internal IReactivePlan<TModel> Plan { get; }

        /// <summary>The element ID — used by .Reactive() to wire events.</summary>
        internal string ElementId { get; }

#if NET48
        public string ToHtmlString() => _inner.ToHtmlString();
#else
        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            _inner.WriteTo(writer, encoder);
        }
#endif
    }
}
