namespace Alis.Reactive.DesignSystem.Tokens
{
    /// <summary>Composes generated design-system classes with caller-supplied classes.</summary>
    public static class CssUtils
    {
        /// <summary>Appends caller classes after generated classes so normal CSS ordering can override defaults.</summary>
        /// <param name="generated">Generated classes.</param>
        /// <param name="cssClass">Caller-supplied classes, if any.</param>
        /// <returns>Merged class string.</returns>
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
