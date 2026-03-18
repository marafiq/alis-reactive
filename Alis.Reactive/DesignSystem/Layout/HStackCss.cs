using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    public static class HStackCss
    {
        public static string Classes(
            SpacingScale gap,
            AlignItems align = AlignItems.Center,
            JustifyContent justify = JustifyContent.Start,
            bool wrap = false,
            string userClass = null)
        {
            var wrapClass = wrap ? "flex-wrap" : "";
            var baseClasses = $"flex {TokenMap.Gap(gap)} {TokenMap.Items(align)} {TokenMap.Justify(justify)} {wrapClass}".Trim();
            return CssUtils.MergeClasses(baseClasses, userClass);
        }
    }
}
