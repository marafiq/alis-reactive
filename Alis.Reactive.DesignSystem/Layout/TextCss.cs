using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    public static class TextCss
    {
        public static string Classes(TextSize size, TextColor color = TextColor.Primary, bool bold = false, ElementSpacing spacing = ElementSpacing.Base, string? cssClass = null)
        {
            var boldClass = bold ? " font-semibold" : "";
            var spacingClass = TokenMap.Spacing(spacing);
            var baseClasses = string.IsNullOrEmpty(spacingClass)
                ? $"{TokenMap.Size(size)} {TokenMap.Color(color)}{boldClass}"
                : $"{TokenMap.Size(size)} {TokenMap.Color(color)}{boldClass} {spacingClass}";
            return CssUtils.MergeClasses(baseClasses, cssClass);
        }
    }
}
