using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    /// <summary>
    /// Builds CSS utility strings for centered content containers.
    /// </summary>
    public static class ContainerCss
    {
        /// <summary>Builds the CSS classes for a centered content container.</summary>
        /// <param name="userClass">Additional classes supplied by the caller.</param>
        /// <returns>The container classes.</returns>
        public static string Classes(string? userClass = null)
        {
            return CssUtils.MergeClasses("max-w-7xl mx-auto px-4 sm:px-6 lg:px-8", userClass);
        }
    }
}
