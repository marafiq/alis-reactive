using System.IO;

namespace Alis.Reactive.DesignSystem.Layout
{
    public class CardBodyBuilder
    {
        private CardPadding _padding = CardPadding.Standard;
        private string _cssClass;

        public CardBodyBuilder Padding(CardPadding padding)
        {
            _padding = padding;
            return this;
        }

        public CardBodyBuilder CssClass(string cssClass)
        {
            _cssClass = cssClass;
            return this;
        }

        public HtmlRenderScope Begin(TextWriter writer)
        {
            var classes = Tokens.CssUtils.MergeClasses(CardCss.BodyClasses(_padding), _cssClass);
            writer.Write($"<div class=\"{classes}\">");
            return new HtmlRenderScope(writer, "</div>");
        }
    }
}
