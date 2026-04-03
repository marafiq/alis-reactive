namespace Alis.Reactive.DesignSystem.Tokens
{
    /// <summary>
    /// Provides helper methods for composing CSS class strings.
    /// </summary>
    public static class CssUtils
    {
        /// <summary>
        /// Merges framework-generated classes with optional user-supplied classes.
        /// </summary>
        /// <param name="generated">The framework-generated class list.</param>
        /// <param name="userClass">Additional classes supplied by the caller.</param>
        /// <returns>A merged class string.</returns>
        public static string MergeClasses(string generated, string? userClass)
        {
            if (string.IsNullOrWhiteSpace(userClass))
                return generated;

            return generated + " " + userClass.Trim();
        }
    }
}
