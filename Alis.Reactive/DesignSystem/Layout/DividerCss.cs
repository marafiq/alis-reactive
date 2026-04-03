namespace Alis.Reactive.DesignSystem.Layout
{
    /// <summary>
    /// Provides reusable divider markup snippets for the design system.
    /// </summary>
    public static class DividerCss
    {
        /// <summary>Gets the standard divider markup.</summary>
        public static string PlainHtml => "<hr class='border-t border-border my-4' />";

        /// <summary>Gets the dashed divider markup.</summary>
        public static string DashedHtml => "<hr class='border-t border-dashed border-border my-4' />";

        /// <summary>Builds labeled divider markup.</summary>
        /// <param name="label">The text shown in the divider label.</param>
        /// <returns>The labeled divider markup.</returns>
        public static string LabeledHtml(string label)
        {
            return $"<div class='relative my-4'><div class='absolute inset-0 flex items-center'><div class='w-full border-t border-border'></div></div><div class='relative flex justify-center'><span class='bg-white px-3 text-sm text-text-muted'>{label}</span></div></div>";
        }
    }
}
