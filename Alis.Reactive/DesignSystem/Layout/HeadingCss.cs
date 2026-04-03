using System;
using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    /// <summary>
    /// Builds CSS utility strings for headings and overlines.
    /// </summary>
    public static class HeadingCss
    {
        /// <summary>Builds the CSS classes for a heading element.</summary>
        /// <param name="level">The heading level to render.</param>
        /// <param name="userClass">Additional classes supplied by the caller.</param>
        /// <returns>The heading classes.</returns>
        public static string Classes(HeadingLevel level, string? userClass = null)
        {
            var sizeClass = level switch
            {
                HeadingLevel.H1 => "text-3xl font-extrabold tracking-tight mb-2",
                HeadingLevel.H2 => "text-xl font-semibold tracking-tight mb-4",
                HeadingLevel.H3 => "text-lg font-semibold tracking-tight mb-3",
                HeadingLevel.H4 => "text-lg font-medium mb-2",
                HeadingLevel.H5 => "text-base font-medium mb-2",
                HeadingLevel.H6 => "text-sm font-medium uppercase tracking-wide mb-2",
                _ => "text-base font-medium mb-2"
            };
            return CssUtils.MergeClasses($"font-display text-text-primary {sizeClass}", userClass);
        }

        /// <summary>Builds the CSS classes for an overline element.</summary>
        /// <param name="userClass">Additional classes supplied by the caller.</param>
        /// <returns>The overline classes.</returns>
        public static string OverlineClasses(string? userClass = null)
        {
            return CssUtils.MergeClasses("text-xs font-semibold uppercase tracking-wider text-text-muted mb-1", userClass);
        }
    }
}
