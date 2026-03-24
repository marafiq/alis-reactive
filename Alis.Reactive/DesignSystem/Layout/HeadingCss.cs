using System;
using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    public static class HeadingCss
    {
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

        public static string OverlineClasses(string? userClass = null)
        {
            return CssUtils.MergeClasses("text-xs font-semibold uppercase tracking-wider text-text-muted mb-1", userClass);
        }
    }
}
