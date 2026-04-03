using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    /// <summary>
    /// Builds CSS utility strings for card-related surfaces.
    /// </summary>
    public static class CardCss
    {
        /// <summary>Builds the CSS classes for the card container.</summary>
        /// <param name="elevation">The elevation preset to apply.</param>
        /// <returns>The card container classes.</returns>
        public static string CardClasses(CardElevation elevation = CardElevation.Low)
        {
            var shadow = elevation switch
            {
                CardElevation.Flat => "",
                CardElevation.Low => "shadow-sm",
                CardElevation.Medium => "shadow-md",
                CardElevation.High => "shadow-lg",
                _ => "shadow-sm"
            };
            return CssUtils.MergeClasses("bg-surface-elevated rounded-2xl border border-border", shadow);
        }

        /// <summary>Builds the CSS classes for an accented card treatment.</summary>
        /// <param name="accent">The accent color to apply.</param>
        /// <returns>The accent classes.</returns>
        public static string AccentInnerClasses(AccentColor accent)
        {
            return $"border-l-4 {TokenMap.Accent(accent)}";
        }

        /// <summary>Builds the CSS classes for a card header section.</summary>
        /// <param name="divider">The divider preset to apply.</param>
        /// <returns>The header classes.</returns>
        public static string HeaderClasses(CardDivider divider)
        {
            var border = divider == CardDivider.Header || divider == CardDivider.Both
                ? "border-b border-border"
                : "";
            return CssUtils.MergeClasses("px-6 py-4", border);
        }

        /// <summary>Builds the CSS classes for a card body section.</summary>
        /// <param name="padding">The padding preset to apply.</param>
        /// <returns>The body classes.</returns>
        public static string BodyClasses(CardPadding padding = CardPadding.Standard)
        {
            return padding switch
            {
                CardPadding.None => "",
                CardPadding.Compact => "px-5 py-4",
                CardPadding.Standard => "p-6 sm:p-8",
                _ => "p-6 sm:p-8"
            };
        }

        /// <summary>Builds the CSS classes for a card footer section.</summary>
        /// <param name="divider">The divider preset to apply.</param>
        /// <returns>The footer classes.</returns>
        public static string FooterClasses(CardDivider divider)
        {
            var border = divider == CardDivider.Footer || divider == CardDivider.Both
                ? "border-t border-border"
                : "";
            return CssUtils.MergeClasses("px-6 py-4", border);
        }
    }
}
