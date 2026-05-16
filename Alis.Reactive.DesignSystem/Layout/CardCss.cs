using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    public static class CardCss
    {
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

        public static string AccentInnerClasses(AccentColor accent)
        {
            return $"border-l-4 {TokenMap.Accent(accent)}";
        }

        public static string HeaderClasses(CardDivider divider)
        {
            var border = divider == CardDivider.Header || divider == CardDivider.Both
                ? "border-b border-border"
                : "";
            return CssUtils.MergeClasses("px-6 py-4", border);
        }

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

        public static string FooterClasses(CardDivider divider)
        {
            var border = divider == CardDivider.Footer || divider == CardDivider.Both
                ? "border-t border-border"
                : "";
            return CssUtils.MergeClasses("px-6 py-4", border);
        }
    }
}
