using System;
using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    public static class HeadingCss
    {
        public static string Classes(HeadingLevel level, ElementSpacing spacing = ElementSpacing.Sm, string? cssClass = null)
        {
            var sizeClass = level switch
            {
                HeadingLevel.H1 => "text-3xl font-extrabold tracking-tight",
                HeadingLevel.H2 => "text-xl font-semibold tracking-tight",
                HeadingLevel.H3 => "text-lg font-semibold tracking-tight",
                HeadingLevel.H4 => "text-lg font-medium",
                HeadingLevel.H5 => "text-base font-medium",
                HeadingLevel.H6 => "text-sm font-medium uppercase tracking-wide",
                _ => "text-base font-medium"
            };
            var spacingClass = TokenMap.Spacing(spacing);
            var baseClasses = string.IsNullOrEmpty(spacingClass)
                ? $"font-display text-text-primary {sizeClass}"
                : $"font-display text-text-primary {sizeClass} {spacingClass}";
            return CssUtils.MergeClasses(baseClasses, cssClass);
        }

        public static string OverlineClasses(string? cssClass = null)
        {
            return CssUtils.MergeClasses("text-xs font-semibold uppercase tracking-wider text-text-muted mb-1", cssClass);
        }
    }
}
