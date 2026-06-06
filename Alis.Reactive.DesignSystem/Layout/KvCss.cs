using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    public static class KvCss
    {
        /// <summary>Builds wrapper classes for stacked key/value content.</summary>
        /// <param name="cssClass">Additional wrapper classes, if any.</param>
        /// <returns>Wrapper class string.</returns>
        public static string StackedWrapperClasses(string? cssClass = null)
        {
            return CssUtils.MergeClasses(string.Empty, cssClass);
        }

        public static string StackedDtClasses() => "text-xs font-medium text-text-muted uppercase tracking-wide";

        public static string StackedDdClasses() => "mt-1 text-sm text-text-primary";

        public static string InlineWrapperClasses(string? cssClass = null)
        {
            return CssUtils.MergeClasses("flex items-center gap-2", cssClass);
        }

        public static string InlineDtClasses() => "text-sm font-medium text-text-muted";

        public static string InlineDdClasses() => "text-sm text-text-primary";
    }
}
