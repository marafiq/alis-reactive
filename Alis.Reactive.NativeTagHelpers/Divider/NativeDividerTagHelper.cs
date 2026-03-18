using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Divider;

[HtmlTargetElement("native-divider")]
public class NativeDividerTagHelper : TagHelper
{
    public DividerStyle Style { get; set; } = DividerStyle.Plain;
    public string? Label { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        DividerRenderer.Render(output, Style, Label);
    }
}
