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
    /// Carries rendered Syncfusion Tab markup and the Reactive Plan for event wiring.
    /// Tab renders without an input field wrapper, label, or validation slot.
    /// </summary>
    public class FusionTabBuilder<TModel> :
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

        internal FusionTabBuilder(ReactivePlan<TModel> plan, string elementId,
#if NET48
            IHtmlString inner)
#else
            IHtmlContent inner)
#endif
        {
            Plan = plan;
            ElementId = elementId;
            _inner = inner;
        }

        internal ReactivePlan<TModel> Plan { get; }

        internal string ElementId { get; }

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
