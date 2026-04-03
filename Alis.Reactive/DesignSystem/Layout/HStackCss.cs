using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    /// <summary>
    /// Builds CSS utility strings for horizontal stack layouts.
    /// </summary>
    public static class HStackCss
    {
        /// <summary>Builds the CSS classes for a horizontal stack layout.</summary>
        /// <param name="gap">The spacing token used between items.</param>
        /// <param name="align">The cross-axis alignment preset.</param>
        /// <param name="justify">The main-axis justification preset.</param>
        /// <param name="wrap"><see langword="true"/> to allow wrapping; otherwise, <see langword="false"/>.</param>
        /// <param name="userClass">Additional classes supplied by the caller.</param>
        /// <returns>The horizontal stack classes.</returns>
        public static string Classes(
            SpacingScale gap,
            AlignItems align = AlignItems.Center,
            JustifyContent justify = JustifyContent.Start,
            bool wrap = false,
            string? userClass = null)
        {
            var wrapClass = wrap ? "flex-wrap" : "";
            var baseClasses = $"flex {TokenMap.Gap(gap)} {TokenMap.Items(align)} {TokenMap.Justify(justify)} {wrapClass}".Trim();
            return CssUtils.MergeClasses(baseClasses, userClass);
        }
    }
}
