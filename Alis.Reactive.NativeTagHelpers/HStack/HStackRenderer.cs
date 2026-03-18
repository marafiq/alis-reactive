using Alis.Reactive.DesignSystem.Layout;
using Alis.Reactive.DesignSystem.Tokens;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.HStack;

internal static class HStackRenderer
{
    public static void Render(TagHelperOutput output, SpacingScale gap, AlignItems align, JustifyContent justify, bool wrap, string? cssClass)
    {
        output.TagName = "div";
        var classes = HStackCss.Classes(gap, align, justify, wrap, cssClass);
        output.Attributes.SetAttribute("class", classes);
    }
}
