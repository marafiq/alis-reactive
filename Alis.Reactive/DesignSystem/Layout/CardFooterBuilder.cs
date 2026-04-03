using System.IO;

namespace Alis.Reactive.DesignSystem.Layout
{
    /// <summary>
    /// Builds the opening and closing markup for a card footer section.
    /// </summary>
    public class CardFooterBuilder
    {
        private CardDivider _divider = CardDivider.None;
        private string? _cssClass;

        /// <summary>Sets the divider preset for the card footer.</summary>
        /// <param name="divider">The divider preset to apply.</param>
        /// <returns>The current builder.</returns>
        public CardFooterBuilder Divider(CardDivider divider)
        {
            _divider = divider;
            return this;
        }

        /// <summary>Adds custom CSS classes to the card footer.</summary>
        /// <param name="cssClass">The CSS classes to append.</param>
        /// <returns>The current builder.</returns>
        public CardFooterBuilder CssClass(string cssClass)
        {
            _cssClass = cssClass;
            return this;
        }

        /// <summary>Writes the opening card footer markup and returns a scope that closes it.</summary>
        /// <param name="writer">The writer that receives the markup.</param>
        /// <returns>A scope that writes the closing markup when disposed.</returns>
        public HtmlRenderScope Begin(TextWriter writer)
        {
            var classes = Tokens.CssUtils.MergeClasses(CardCss.FooterClasses(_divider), _cssClass);
            writer.Write($"<div class='{classes}'>");
            return new HtmlRenderScope(writer, "</div>");
        }
    }
}
