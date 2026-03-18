using Alis.Reactive.DesignSystem.Layout;
using Alis.Reactive.DesignSystem.Tokens;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Card;

[HtmlTargetElement("native-card-body", ParentTag = "native-card")]
public class NativeCardBodyTagHelper : TagHelper
{
    public CardPadding Padding { get; set; } = CardPadding.Standard;
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        var classes = CssUtils.MergeClasses(CardCss.BodyClasses(Padding), CssClass);
        output.Attributes.SetAttribute("class", classes);
    }
}
