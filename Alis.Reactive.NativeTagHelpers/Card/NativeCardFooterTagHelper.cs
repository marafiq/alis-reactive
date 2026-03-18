using Alis.Reactive.DesignSystem.Layout;
using Alis.Reactive.DesignSystem.Tokens;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Card;

[HtmlTargetElement("native-card-footer", ParentTag = "native-card")]
public class NativeCardFooterTagHelper : TagHelper
{
    public CardDivider Divider { get; set; } = CardDivider.None;
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        var classes = CssUtils.MergeClasses(CardCss.FooterClasses(Divider), CssClass);
        output.Attributes.SetAttribute("class", classes);
    }
}
