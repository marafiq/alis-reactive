using Alis.Reactive.DesignSystem.Tokens;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Grid;

[HtmlTargetElement("native-grid")]
public class NativeGridTagHelper : TagHelper
{
    public GridCols Cols { get; set; } = GridCols.C2;
    public SpacingScale Gap { get; set; } = SpacingScale.Md;
    public bool Responsive { get; set; } = true;
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        GridRenderer.Render(output, Cols, Gap, Responsive, CssClass);
    }
}
