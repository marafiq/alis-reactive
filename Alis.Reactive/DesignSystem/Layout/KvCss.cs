namespace Alis.Reactive.DesignSystem.Layout
{
    public static class KvCss
    {
        public static string StackedHtml(string label, string value)
        {
            return $"<div><dt class=\"text-xs font-medium text-text-muted uppercase tracking-wide\">{label}</dt><dd class=\"mt-1 text-sm text-text-primary\">{value}</dd></div>";
        }

        public static string InlineHtml(string label, string value)
        {
            return $"<div class=\"flex items-center gap-2\"><dt class=\"text-sm font-medium text-text-muted\">{label}:</dt><dd class=\"text-sm text-text-primary\">{value}</dd></div>";
        }
    }
}
