namespace Alis.Reactive.DesignSystem.Layout
{
    public static class DividerCss
    {
        public static string PlainHtml => "<hr class=\"border-t border-border my-4\" />";
        public static string DashedHtml => "<hr class=\"border-t border-dashed border-border my-4\" />";

        public static string LabeledHtml(string label)
        {
            return $"<div class=\"relative my-4\"><div class=\"absolute inset-0 flex items-center\"><div class=\"w-full border-t border-border\"></div></div><div class=\"relative flex justify-center\"><span class=\"bg-white px-3 text-sm text-text-muted\">{label}</span></div></div>";
        }
    }
}
