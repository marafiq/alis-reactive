using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    public static class GridCss
    {
        public static string Classes(GridCols cols, SpacingScale gap = SpacingScale.Md, string? userClass = null)
        {
            var baseClasses = $"grid {TokenMap.Cols(cols)} {TokenMap.Gap(gap)}";
            return CssUtils.MergeClasses(baseClasses, userClass);
        }

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
