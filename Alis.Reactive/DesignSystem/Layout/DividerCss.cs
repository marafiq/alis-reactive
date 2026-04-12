using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    public static class DividerCss
    {
        public static string PlainClasses(string? cssClass = null)
        {
            return CssUtils.MergeClasses("border-t border-border my-4", cssClass);
        }

        public static string DashedClasses(string? cssClass = null)
        {
            return CssUtils.MergeClasses("border-t border-dashed border-border my-4", cssClass);
        }

        public static string LabeledWrapperClasses(string? cssClass = null)
        {
            return CssUtils.MergeClasses("relative my-4", cssClass);
        }

        public static string LabeledLineOuterClasses() => "absolute inset-0 flex items-center";

        public static string LabeledLineInnerClasses() => "w-full border-t border-border";

        public static string LabeledTextWrapperClasses() => "relative flex justify-center";

        public static string LabeledTextClasses() => "bg-surface px-3 text-sm text-text-muted";
    }
}
