using Alis.Reactive.DesignSystem;

namespace Alis.Reactive.DesignSystem
{
    public static class VStackCss
    {
        public static string Classes(SpacingScale gap, bool divideY = false, string? cssClass = null)
        {
            var divideClass = divideY ? " divide-y divide-border" : "";
            var baseClasses = $"flex flex-col {TokenMap.Gap(gap)}{divideClass}";
            return CssUtils.MergeClasses(baseClasses, cssClass);
        }
    }
}
