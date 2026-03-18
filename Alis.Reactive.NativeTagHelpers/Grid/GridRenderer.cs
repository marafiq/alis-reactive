using Alis.Reactive.DesignSystem.Layout;
using Alis.Reactive.DesignSystem.Tokens;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Grid;

internal static class GridRenderer
{
    public static void Render(TagHelperOutput output, GridCols cols, SpacingScale gap, bool responsive, string? cssClass)
    {
        output.TagName = "div";
        var classes = responsive
            ? GridCss.ResponsiveClasses(cols, gap, cssClass)
            : GridCss.Classes(cols, gap, cssClass);
        output.Attributes.SetAttribute("class", classes);
    }
}
