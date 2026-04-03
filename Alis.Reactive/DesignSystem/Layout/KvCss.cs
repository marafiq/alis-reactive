namespace Alis.Reactive.DesignSystem.Layout
{
    /// <summary>
    /// Provides reusable key-value markup snippets for the design system.
    /// </summary>
    public static class KvCss
    {
        /// <summary>Builds stacked key-value markup.</summary>
        /// <param name="label">The label text.</param>
        /// <param name="value">The value text.</param>
        /// <returns>The stacked key-value markup.</returns>
        public static string StackedHtml(string label, string value)
        {
            return $"<div><dt class='text-xs font-medium text-text-muted uppercase tracking-wide'>{label}</dt><dd class='mt-1 text-sm text-text-primary'>{value}</dd></div>";
        }

        /// <summary>Builds inline key-value markup.</summary>
        /// <param name="label">The label text.</param>
        /// <param name="value">The value text.</param>
        /// <returns>The inline key-value markup.</returns>
        public static string InlineHtml(string label, string value)
        {
            return $"<div class='flex items-center gap-2'><dt class='text-sm font-medium text-text-muted'>{label}:</dt><dd class='text-sm text-text-primary'>{value}</dd></div>";
        }
    }
}
