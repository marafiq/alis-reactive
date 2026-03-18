using System.IO;
using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    public class HStackBuilder
    {
        private SpacingScale _gap;
        private AlignItems _align = AlignItems.Center;
        private JustifyContent _justify = JustifyContent.Start;
        private bool _wrap;
        private string _cssClass;
        private string _id;

        public HStackBuilder(SpacingScale gap)
        {
            _gap = gap;
        }

        public HStackBuilder Align(AlignItems align)
        {
            _align = align;
            return this;
        }

        public HStackBuilder Justify(JustifyContent justify)
        {
            _justify = justify;
            return this;
        }

        public HStackBuilder Wrap(bool wrap = true)
        {
            _wrap = wrap;
            return this;
        }

        public HStackBuilder CssClass(string cssClass)
        {
            _cssClass = cssClass;
            return this;
        }

        public HStackBuilder Id(string id)
        {
            _id = id;
            return this;
        }

        public HtmlRenderScope Begin(TextWriter writer)
        {
            var classes = HStackCss.Classes(_gap, _align, _justify, _wrap, _cssClass);
            writer.Write("<div");
            if (!string.IsNullOrEmpty(_id))
                writer.Write($" id=\"{_id}\"");
            writer.Write($" class=\"{classes}\">");
            return new HtmlRenderScope(writer, "</div>");
        }
    }
}
