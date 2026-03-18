using Alis.Reactive.DesignSystem.Tokens;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.HStack;

[HtmlTargetElement("native-hstack")]
public class NativeHStackTagHelper : TagHelper
{
    public SpacingScale Gap { get; set; } = SpacingScale.Base;
    public AlignItems Align { get; set; } = AlignItems.Center;
    public JustifyContent Justify { get; set; } = JustifyContent.Start;
    public bool Wrap { get; set; }
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        HStackRenderer.Render(output, Gap, Align, Justify, Wrap, CssClass);
    }
}
