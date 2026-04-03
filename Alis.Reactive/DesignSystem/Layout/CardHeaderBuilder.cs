using System.IO;

namespace Alis.Reactive.DesignSystem.Layout
{
    /// <summary>
    /// Builds the opening and closing markup for a card header section.
    /// </summary>
    public class CardHeaderBuilder
    {
        private CardDivider _divider = CardDivider.None;
        private string? _cssClass;

        /// <summary>Sets the divider preset for the card header.</summary>
        /// <param name="divider">The divider preset to apply.</param>
        /// <returns>The current builder.</returns>
        public CardHeaderBuilder Divider(CardDivider divider)
        {
            _divider = divider;
            return this;
        }

        /// <summary>Adds custom CSS classes to the card header.</summary>
        /// <param name="cssClass">The CSS classes to append.</param>
        /// <returns>The current builder.</returns>
        public CardHeaderBuilder CssClass(string cssClass)
        {
            _cssClass = cssClass;
            return this;
        }

        /// <summary>Writes the opening card header markup and returns a scope that closes it.</summary>
        /// <param name="writer">The writer that receives the markup.</param>
        /// <returns>A scope that writes the closing markup when disposed.</returns>
        public HtmlRenderScope Begin(TextWriter writer)
        {
            var classes = Tokens.CssUtils.MergeClasses(CardCss.HeaderClasses(_divider), _cssClass);
            writer.Write($"<div class='{classes}'>");
            return new HtmlRenderScope(writer, "</div>");
        }
    }
}
