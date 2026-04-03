using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    /// <summary>
    /// Builds CSS utility strings for grid layouts.
    /// </summary>
    public static class GridCss
    {
        /// <summary>Builds the CSS classes for a fixed grid layout.</summary>
        /// <param name="cols">The column token to render.</param>
        /// <param name="gap">The spacing token used between grid items.</param>
        /// <param name="userClass">Additional classes supplied by the caller.</param>
        /// <returns>The grid classes.</returns>
        public static string Classes(GridCols cols, SpacingScale gap = SpacingScale.Md, string? userClass = null)
        {
            var baseClasses = $"grid {TokenMap.Cols(cols)} {TokenMap.Gap(gap)}";
            return CssUtils.MergeClasses(baseClasses, userClass);
        }

        /// <summary>Builds the CSS classes for a responsive grid layout.</summary>
        /// <param name="cols">The column token to render at larger breakpoints.</param>
        /// <param name="gap">The spacing token used between grid items.</param>
        /// <param name="userClass">Additional classes supplied by the caller.</param>
        /// <returns>The responsive grid classes.</returns>
        public static string ResponsiveClasses(GridCols cols, SpacingScale gap = SpacingScale.Md, string? userClass = null)
        {
            var colCount = (int)cols;
            var responsive = colCount <= 2
                ? $"grid grid-cols-1 sm:{TokenMap.Cols(cols)} {TokenMap.Gap(gap)}"
                : $"grid grid-cols-1 sm:grid-cols-2 lg:{TokenMap.Cols(cols)} {TokenMap.Gap(gap)}";
            return CssUtils.MergeClasses(responsive, userClass);
        }
    }
}
