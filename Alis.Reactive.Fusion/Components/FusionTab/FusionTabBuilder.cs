using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wraps the rendered Fusion Tab markup and carries the object identity needed for reactive workflows.
    /// </summary>
    public class FusionTabBuilder<TModel> : IHtmlContent where TModel : class
    {
        private readonly IHtmlContent _inner;

        internal FusionTabBuilder(ReactivePlan<TModel> plan, string elementId, IHtmlContent inner)
        {
            Plan = plan;
            ElementId = elementId;
            _inner = inner;
        }

        /// <summary>The reactive plan — used by .Reactive() to add workflows.</summary>
        internal ReactivePlan<TModel> Plan { get; }

        /// <summary>The element ID — used by .Reactive() to wire events.</summary>
        internal string ElementId { get; }

        /// <summary>
        /// Writes the rendered tab markup.
        /// </summary>
        /// <param name="writer">The writer that receives the markup.</param>
        /// <param name="encoder">The encoder used for HTML output.</param>
        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            _inner.WriteTo(writer, encoder);
        }
    }
}
