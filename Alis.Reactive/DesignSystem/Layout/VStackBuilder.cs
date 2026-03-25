using System.IO;
using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    public class VStackBuilder
    {
        private readonly SpacingScale _gap;
        private bool _divideY;
        private string? _cssClass;
        private string? _id;

        public VStackBuilder(SpacingScale gap)
        {
            _gap = gap;
        }

        public VStackBuilder DivideY(bool divideY = true)
        {
            _divideY = divideY;
            return this;
        }

        public VStackBuilder CssClass(string cssClass)
        {
            _cssClass = cssClass;
            return this;
        }

        public VStackBuilder Id(string id)
        {
            _id = id;
            return this;
        }

        public HtmlRenderScope Begin(TextWriter writer)
        {
            var classes = VStackCss.Classes(_gap, _divideY, _cssClass);
            writer.Write("<div");
            if (!string.IsNullOrEmpty(_id))
                writer.Write($" id=\"{_id}\"");
            writer.Write($" class=\"{classes}\">");
            return new HtmlRenderScope(writer, "</div>");
        }
    }
}
