using Alis.Reactive.DesignSystem.Layout;
using Alis.Reactive.DesignSystem.Tokens;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Card;

[HtmlTargetElement("native-card")]
public class NativeCardTagHelper : TagHelper
{
    public CardElevation Elevation { get; set; } = CardElevation.Low;
    public AccentColor? Accent { get; set; }
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    public override void Init(TagHelperContext context)
    {
        context.Items[typeof(CardContext)] = new CardContext();
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        CardRenderer.RenderOpen(output, Elevation, Accent, CssClass);
    }
}
