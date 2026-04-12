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
    /// Wraps SF GridBuilder.Render() output and carries plan and elementId
    /// for <c>.Reactive()</c> chaining. Non-input component.
    /// </summary>
    public class FusionGridBuilder<TModel> :
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
        internal FusionGridBuilder(ReactivePlan<TModel> plan, string elementId, IHtmlString inner)
#else
        internal FusionGridBuilder(ReactivePlan<TModel> plan, string elementId, IHtmlContent inner)
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
