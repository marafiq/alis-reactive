using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    /// <summary>
    /// Builds CSS utility strings for body text.
    /// </summary>
    public static class TextCss
    {
        /// <summary>Builds the CSS classes for a text element.</summary>
        /// <param name="size">The text size to render.</param>
        /// <param name="color">The text color to render.</param>
        /// <param name="bold"><see langword="true"/> to render bold text; otherwise, <see langword="false"/>.</param>
        /// <param name="userClass">Additional classes supplied by the caller.</param>
        /// <returns>The text classes.</returns>
        public static string Classes(TextSize size, TextColor color = TextColor.Primary, bool bold = false, string? userClass = null)
        {
            var boldClass = bold ? " font-semibold" : "";
            var baseClasses = $"{TokenMap.Size(size)} {TokenMap.Color(color)}{boldClass} mb-3";
            return CssUtils.MergeClasses(baseClasses, userClass);
        }
    }
}
