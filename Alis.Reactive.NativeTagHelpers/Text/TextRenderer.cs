using Alis.Reactive.DesignSystem.Layout;
using Alis.Reactive.DesignSystem.Tokens;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Text;

internal static class TextRenderer
{
    public static void Render(TagHelperOutput output, TextSize size, TextColor color, bool bold, bool asSpan, string? cssClass)
    {
        output.TagName = asSpan ? "span" : "p";
        var classes = TextCss.Classes(size, color, bold, cssClass);
        output.Attributes.SetAttribute("class", classes);
    }
}
