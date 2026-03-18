using Alis.Reactive.DesignSystem.Tokens;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.VStack;

[HtmlTargetElement("native-vstack")]
public class NativeVStackTagHelper : TagHelper
{
    public SpacingScale Gap { get; set; } = SpacingScale.Base;
    public bool DivideY { get; set; }
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        VStackRenderer.Render(output, Gap, DivideY, CssClass);
    }
}
