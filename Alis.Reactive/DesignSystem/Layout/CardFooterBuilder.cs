using System.IO;

namespace Alis.Reactive.DesignSystem.Layout
{
    public class CardFooterBuilder
    {
        private CardDivider _divider = CardDivider.None;
        private string _cssClass;

        public CardFooterBuilder Divider(CardDivider divider)
        {
            _divider = divider;
            return this;
        }

        public CardFooterBuilder CssClass(string cssClass)
        {
            _cssClass = cssClass;
            return this;
        }

        public HtmlRenderScope Begin(TextWriter writer)
        {
            var classes = Tokens.CssUtils.MergeClasses(CardCss.FooterClasses(_divider), _cssClass);
            writer.Write($"<div class=\"{classes}\">");
            return new HtmlRenderScope(writer, "</div>");
        }
    }
}
