using Alis.Reactive.DesignSystem.Tokens;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Heading;

[HtmlTargetElement("native-heading")]
public class NativeHeadingTagHelper : TagHelper
{
    public HeadingLevel Level { get; set; } = HeadingLevel.H2;
    public string? Overline { get; set; }
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        HeadingRenderer.Render(output, Level, Overline, CssClass);
    }
}
