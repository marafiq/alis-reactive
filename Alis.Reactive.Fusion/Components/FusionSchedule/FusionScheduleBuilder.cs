using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Wraps SF ScheduleBuilder.Render() output + carries plan and elementId
    /// for .Reactive() chaining. Non-input component — no input component registration.
    /// </summary>
    public class FusionScheduleBuilder<TModel> :
        IHtmlContent
        where TModel : class
    {
        private readonly IHtmlContent _inner;

        internal ReactivePlan<TModel> Plan { get; }
        internal string ElementId { get; }

        internal FusionScheduleBuilder(ReactivePlan<TModel> plan, string elementId, IHtmlContent inner)
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
