using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Carries rendered Syncfusion Tab markup and the Reactive Plan for event wiring.
    /// The tab renders without an input field wrapper, label, or validation slot.
    /// </summary>
    public class FusionTabBuilder<TModel> :
        IHtmlContent
        where TModel : class
    {
        private readonly IHtmlContent _inner;

        internal FusionTabBuilder(ReactivePlan<TModel> plan, string elementId, IHtmlContent inner)
        {
            Plan = plan;
            ElementId = elementId;
            _inner = inner;
        }

        internal ReactivePlan<TModel> Plan { get; }

        internal string ElementId { get; }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            _inner.WriteTo(writer, encoder);
        }
    }
}
