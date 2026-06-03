using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Carries rendered Syncfusion AIAssistView markup and the Reactive Plan for event wiring.
    /// </summary>
    public sealed class FusionAIAssistViewBuilder<TModel> : IHtmlContent
        where TModel : class
    {
        private readonly IHtmlContent _inner;

        internal ReactivePlan<TModel> Plan { get; }
        internal string ElementId { get; }

        internal FusionAIAssistViewBuilder(
            ReactivePlan<TModel> plan,
            string elementId,
            IHtmlContent inner)
        {
            Plan = plan;
            ElementId = elementId;
            _inner = inner;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            _inner.WriteTo(writer, encoder);
        }
    }
}
