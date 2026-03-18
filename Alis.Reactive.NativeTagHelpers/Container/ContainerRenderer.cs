using Alis.Reactive.DesignSystem.Layout;
using Alis.Reactive.DesignSystem.Tokens;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Container;

internal static class ContainerRenderer
{
    public static void Render(TagHelperOutput output, string? cssClass)
    {
        output.TagName = "div";
        output.Attributes.SetAttribute("class", ContainerCss.Classes(cssClass));
    }
}
