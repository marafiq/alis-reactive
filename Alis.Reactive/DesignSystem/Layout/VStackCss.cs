using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    /// <summary>
    /// Builds CSS utility strings for vertical stack layouts.
    /// </summary>
    public static class VStackCss
    {
        /// <summary>Builds the CSS classes for a vertical stack layout.</summary>
        /// <param name="gap">The spacing token used between items.</param>
        /// <param name="divideY"><see langword="true"/> to render dividers; otherwise, <see langword="false"/>.</param>
        /// <param name="userClass">Additional classes supplied by the caller.</param>
        /// <returns>The vertical stack classes.</returns>
        public static string Classes(SpacingScale gap, bool divideY = false, string? userClass = null)
        {
            var divideClass = divideY ? " divide-y divide-border" : "";
            var baseClasses = $"flex flex-col {TokenMap.Gap(gap)}{divideClass}";
            return CssUtils.MergeClasses(baseClasses, userClass);
        }
    }
}
