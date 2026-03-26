using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wrapper builder for the Syncfusion Grid component.
    /// Wraps SF-rendered IHtmlContent and exposes ElementId + Plan for .Reactive() chaining.
    /// Non-input component — no InputField wrapper, no label, no validation slot.
    /// </summary>
    public class FusionGridBuilder<TModel> : IHtmlContent where TModel : class
    {
        private readonly IHtmlContent _inner;

        internal FusionGridBuilder(IReactivePlan<TModel> plan, string elementId, IHtmlContent inner)
        {
            Plan = plan;
            ElementId = elementId;
            _inner = inner;
        }

        /// <summary>The reactive plan — used by .Reactive() to add entries.</summary>
        internal IReactivePlan<TModel> Plan { get; }

        /// <summary>The element ID — used by .Reactive() to wire events.</summary>
        internal string ElementId { get; }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            _inner.WriteTo(writer, encoder);
        }
    }
}
