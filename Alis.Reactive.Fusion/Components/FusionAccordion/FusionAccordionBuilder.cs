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
    /// Wraps SF AccordionBuilder.Render() output + carries plan and elementId
    /// for .Reactive() chaining. Non-input component — no ComponentsMap registration.
    /// </summary>
#if NET48
    public class FusionAccordionBuilder<TModel> : IHtmlString where TModel : class
    {
        private readonly IHtmlString _inner;
#else
    public class FusionAccordionBuilder<TModel> : IHtmlContent where TModel : class
    {
        private readonly IHtmlContent _inner;
#endif

        internal IReactivePlan<TModel> Plan { get; }
        internal string ElementId { get; }

#if NET48
        internal FusionAccordionBuilder(IReactivePlan<TModel> plan, string elementId, IHtmlString inner)
#else
        internal FusionAccordionBuilder(IReactivePlan<TModel> plan, string elementId, IHtmlContent inner)
#endif
        {
            Plan = plan;
            ElementId = elementId;
            _inner = inner;
        }

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
