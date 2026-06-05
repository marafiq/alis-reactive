namespace Alis.Reactive.DesignSystem.Tokens
{
    /// <summary>
    /// Provides helpers for composing generated design-system classes with caller-supplied HTML classes.
    /// </summary>
    public static class CssUtils
    {
        /// <summary>
        /// Returns generated classes followed by trimmed caller classes so normal CSS ordering can override defaults.
        /// </summary>
        /// <param name="generated">Classes emitted by a design-system helper.</param>
        /// <param name="cssClass">Caller-supplied classes appended after <paramref name="generated"/>, or <see langword="null"/> when none are set.</param>
        /// <returns>A class string containing generated classes and any trimmed caller classes.</returns>
        public static string MergeClasses(string generated, string? cssClass)
        {
            var callerClasses = cssClass ?? string.Empty;

            if (string.IsNullOrWhiteSpace(callerClasses))
                return generated;

            if (string.IsNullOrWhiteSpace(generated))
                return callerClasses.Trim();

            return generated + " " + callerClasses.Trim();
        }
    }
}
