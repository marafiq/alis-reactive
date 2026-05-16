using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    public static class KvCss
    {
        /// <summary>
        /// Builds the wrapper class string for a stacked key/value pair, merging any
        /// developer-supplied override classes.
        /// </summary>
        /// <param name="cssClass">Optional developer-supplied class string.</param>
        /// <returns>The wrapper class string, or an empty string when no override is supplied.</returns>
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
