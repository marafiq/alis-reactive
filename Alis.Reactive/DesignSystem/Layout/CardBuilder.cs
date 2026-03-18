using System.IO;
using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    public class CardBuilder
    {
        private CardElevation _elevation = CardElevation.Low;
        private AccentColor? _accent;
        private string _id;
        private string _cssClass;

        public CardBuilder Elevation(CardElevation elevation)
        {
            _elevation = elevation;
            return this;
        }

        public CardBuilder Accent(AccentColor accent)
        {
            _accent = accent;
            return this;
        }

        public CardBuilder Id(string id)
        {
            _id = id;
            return this;
        }

        public CardBuilder CssClass(string cssClass)
        {
            _cssClass = cssClass;
            return this;
        }

        public HtmlRenderScope Begin(TextWriter writer)
        {
            var classes = CardCss.CardClasses(_elevation);
            if (_accent.HasValue)
                classes = CssUtils.MergeClasses(classes, CardCss.AccentInnerClasses(_accent.Value));
            classes = CssUtils.MergeClasses(classes, _cssClass);

            writer.Write("<div");
            if (!string.IsNullOrEmpty(_id))
                writer.Write($" id=\"{_id}\"");
            writer.Write($" class=\"{classes}\">");
            return new HtmlRenderScope(writer, "</div>");
        }
    }
}
