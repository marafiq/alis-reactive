using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wraps Syncfusion ButtonBuilder output while carrying the component id.
    /// </summary>
    public sealed class FusionButtonBuilder<TModel> : IHtmlContent
        where TModel : class
    {
        private readonly IHtmlContent _inner;

        internal ReactivePlan<TModel> Plan { get; }
        internal string ElementId { get; }

        internal FusionButtonBuilder(ReactivePlan<TModel> plan, string elementId, IHtmlContent inner)
        {
            Plan = plan;
            ElementId = elementId;
            _inner = inner;
        }

        /// <inheritdoc />
        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            _inner.WriteTo(writer, encoder);
        }
    }
}
