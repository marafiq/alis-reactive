namespace Alis.Reactive.DesignSystem.Tokens
{
    /// <summary>
    /// Merges generated design-system classes with caller-supplied HTML classes.
    /// </summary>
    public static class CssUtils
    {
        /// <summary>
        /// Appends caller classes after generated classes so normal CSS ordering can override defaults.
        /// </summary>
        /// <param name="generated">Generated design-system classes.</param>
        /// <param name="cssClass">Caller-supplied HTML classes, or <see langword="null"/> when none are set.</param>
        /// <returns>The merged class string.</returns>
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
