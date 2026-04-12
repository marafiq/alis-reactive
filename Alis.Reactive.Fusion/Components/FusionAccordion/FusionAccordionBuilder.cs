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
    public class FusionAccordionBuilder<TModel> :
#if NET48
        IHtmlString
#else
        IHtmlContent
#endif
        where TModel : class
    {
#if NET48
        private readonly IHtmlString _inner;
#else
        private readonly IHtmlContent _inner;
#endif

        internal ReactivePlan<TModel> Plan { get; }
        internal string ElementId { get; }

#if NET48
        internal FusionAccordionBuilder(ReactivePlan<TModel> plan, string elementId, IHtmlString inner)
#else
        internal FusionAccordionBuilder(ReactivePlan<TModel> plan, string elementId, IHtmlContent inner)
#endif
        {
            Plan = plan;
            ElementId = elementId;
            _inner = inner;
        }

#if NET48
        public string ToHtmlString()
        {
            var sw = new StringWriter();
            WriteTo(sw, HtmlEncoder.Default);
            return sw.ToString();
        }
#endif

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
#if NET48
            writer.Write(_inner.ToHtmlString());
#else
            _inner.WriteTo(writer, encoder);
#endif
        }
    }
}
