using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wraps the rendered Fusion Accordion markup and carries the object identity needed for reactive workflows.
    /// </summary>
    public class FusionAccordionBuilder<TModel> : IHtmlContent where TModel : class
    {
        private readonly IHtmlContent _inner;

        internal ReactivePlan<TModel> Plan { get; }
        internal string ElementId { get; }

        internal FusionAccordionBuilder(ReactivePlan<TModel> plan, string elementId, IHtmlContent inner)
        {
            Plan = plan;
            ElementId = elementId;
            _inner = inner;
        }

        /// <summary>
        /// Writes the rendered accordion markup.
        /// </summary>
        /// <param name="writer">The writer that receives the markup.</param>
        /// <param name="encoder">The encoder used for HTML output.</param>
        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            _inner.WriteTo(writer, encoder);
        }
    }
}
