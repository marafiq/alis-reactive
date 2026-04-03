using System.IO;
using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    /// <summary>
    /// Builds the opening and closing markup for a card container.
    /// </summary>
    public class CardBuilder
    {
        private CardElevation _elevation = CardElevation.Low;
        private AccentColor? _accent;
        private string? _id;
        private string? _cssClass;

        /// <summary>Sets the elevation preset for the card.</summary>
        /// <param name="elevation">The elevation preset to apply.</param>
        /// <returns>The current builder.</returns>
        public CardBuilder Elevation(CardElevation elevation)
        {
            _elevation = elevation;
            return this;
        }

        /// <summary>Adds an accent treatment to the card.</summary>
        /// <param name="accent">The accent color to apply.</param>
        /// <returns>The current builder.</returns>
        public CardBuilder Accent(AccentColor accent)
        {
            _accent = accent;
            return this;
        }

        /// <summary>Sets the HTML id attribute for the card element.</summary>
        /// <param name="id">The element id to assign.</param>
        /// <returns>The current builder.</returns>
        public CardBuilder Id(string id)
        {
            _id = id;
            return this;
        }

        /// <summary>Adds custom CSS classes to the card element.</summary>
        /// <param name="cssClass">The CSS classes to append.</param>
        /// <returns>The current builder.</returns>
        public CardBuilder CssClass(string cssClass)
        {
            _cssClass = cssClass;
            return this;
        }

        /// <summary>Writes the opening card markup and returns a scope that closes it.</summary>
        /// <param name="writer">The writer that receives the markup.</param>
        /// <returns>A scope that writes the closing markup when disposed.</returns>
        public HtmlRenderScope Begin(TextWriter writer)
        {
            var classes = CardCss.CardClasses(_elevation);
            if (_accent.HasValue)
                classes = CssUtils.MergeClasses(classes, CardCss.AccentInnerClasses(_accent.Value));
            classes = CssUtils.MergeClasses(classes, _cssClass);

            writer.Write("<div");
            if (!string.IsNullOrEmpty(_id))
                writer.Write($" id='{_id}'");
            writer.Write($" class='{classes}'>");
            return new HtmlRenderScope(writer, "</div>");
        }
    }
}
