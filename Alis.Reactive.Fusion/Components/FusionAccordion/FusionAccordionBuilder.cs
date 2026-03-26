using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wraps SF AccordionBuilder.Render() output + carries plan and elementId
    /// for .Reactive() chaining. Non-input component — no ComponentsMap registration.
    /// </summary>
    public class FusionAccordionBuilder<TModel> : IHtmlContent where TModel : class
    {
        private readonly IHtmlContent _inner;

        internal IReactivePlan<TModel> Plan { get; }
        internal string ElementId { get; }

        internal FusionAccordionBuilder(IReactivePlan<TModel> plan, string elementId, IHtmlContent inner)
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
