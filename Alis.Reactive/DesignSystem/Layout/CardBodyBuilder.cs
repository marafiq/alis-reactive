using System.IO;

namespace Alis.Reactive.DesignSystem.Layout
{
    /// <summary>
    /// Builds the opening and closing markup for a card body section.
    /// </summary>
    public class CardBodyBuilder
    {
        private CardPadding _padding = CardPadding.Standard;
        private string? _cssClass;

        /// <summary>Sets the padding preset for the card body.</summary>
        /// <param name="padding">The padding preset to apply.</param>
        /// <returns>The current builder.</returns>
        public CardBodyBuilder Padding(CardPadding padding)
        {
            _padding = padding;
            return this;
        }

        /// <summary>Adds custom CSS classes to the card body.</summary>
        /// <param name="cssClass">The CSS classes to append.</param>
        /// <returns>The current builder.</returns>
        public CardBodyBuilder CssClass(string cssClass)
        {
            _cssClass = cssClass;
            return this;
        }

        /// <summary>Writes the opening card body markup and returns a scope that closes it.</summary>
        /// <param name="writer">The writer that receives the markup.</param>
        /// <returns>A scope that writes the closing markup when disposed.</returns>
        public HtmlRenderScope Begin(TextWriter writer)
        {
            var classes = Tokens.CssUtils.MergeClasses(CardCss.BodyClasses(_padding), _cssClass);
            writer.Write($"<div class='{classes}'>");
            return new HtmlRenderScope(writer, "</div>");
        }
    }
}
