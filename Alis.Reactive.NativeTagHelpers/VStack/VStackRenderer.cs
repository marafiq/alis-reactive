using Alis.Reactive.DesignSystem.Layout;
using Alis.Reactive.DesignSystem.Tokens;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.VStack;

internal static class VStackRenderer
{
    public static void Render(TagHelperOutput output, SpacingScale gap, bool divideY, string? cssClass)
    {
        output.TagName = "div";
        var classes = VStackCss.Classes(gap, divideY, cssClass);
        output.Attributes.SetAttribute("class", classes);
    }
}
