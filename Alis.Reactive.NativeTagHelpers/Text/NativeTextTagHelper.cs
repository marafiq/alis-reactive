using Alis.Reactive.DesignSystem.Tokens;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Text;

[HtmlTargetElement("native-text")]
public class NativeTextTagHelper : TagHelper
{
    public TextSize Size { get; set; } = TextSize.Base;
    public TextColor Color { get; set; } = TextColor.Primary;
    public bool Bold { get; set; }
    public bool AsSpan { get; set; }
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        TextRenderer.Render(output, Size, Color, Bold, AsSpan, CssClass);
    }
}
