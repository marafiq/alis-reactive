using Alis.Reactive.DesignSystem.Tokens;

namespace Alis.Reactive.DesignSystem.Layout
{
    public static class TextCss
    {
        public static string Classes(TextSize size, TextColor color = TextColor.Primary, bool bold = false, string? userClass = null)
        {
            var boldClass = bold ? " font-semibold" : "";
            var baseClasses = $"{TokenMap.Size(size)} {TokenMap.Color(color)}{boldClass} mb-3";
            return CssUtils.MergeClasses(baseClasses, userClass);
        }
    }
}
