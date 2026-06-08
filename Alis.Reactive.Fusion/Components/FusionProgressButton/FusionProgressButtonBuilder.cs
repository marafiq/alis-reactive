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
    /// Wraps Syncfusion ProgressButtonBuilder output while carrying the component id.
    /// </summary>
    public sealed class FusionProgressButtonBuilder<TModel> :
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

        internal FusionProgressButtonBuilder(
            ReactivePlan<TModel> plan,
            string elementId,
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

#if NET48
        public string ToHtmlString() => _inner.ToHtmlString();
#else
        /// <inheritdoc />
        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            _inner.WriteTo(writer, encoder);
        }
#endif
    }
}
