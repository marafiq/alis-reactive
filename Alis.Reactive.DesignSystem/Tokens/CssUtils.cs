namespace Alis.Reactive.DesignSystem.Tokens
{
    /// <summary>
    /// Composes CSS class strings from design-system defaults and developer-supplied overrides.
    /// </summary>
    public static class CssUtils
    {
        /// <summary>
        /// Appends a developer-supplied class string to the design-system generated classes,
        /// letting callers extend or override the default styling.
        /// </summary>
        /// <param name="generated">The design-system generated class string.</param>
        /// <param name="cssClass">The developer-supplied class string, or <see langword="null"/> when none is set.</param>
        /// <returns>
        /// The combined class string. Returns <paramref name="generated"/> when
        /// <paramref name="cssClass"/> is empty, and the trimmed <paramref name="cssClass"/>
        /// when <paramref name="generated"/> is empty.
        /// </returns>
        public static string MergeClasses(string generated, string? cssClass)
        {
            var overrideClasses = cssClass ?? string.Empty;

            if (string.IsNullOrWhiteSpace(overrideClasses))
                return generated;

            if (string.IsNullOrWhiteSpace(generated))
                return overrideClasses.Trim();

            return generated + " " + overrideClasses.Trim();
        }
    }
}
