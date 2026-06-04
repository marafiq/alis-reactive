using Alis.Reactive.DesignSystem.Layout;
using Alis.Reactive.DesignSystem.Tokens;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Card;

/// <summary>
/// Renders a <c>native-card-footer</c> element as the footer section of a card.
/// </summary>
[HtmlTargetElement("native-card-footer", ParentTag = "native-card")]
public class NativeCardFooterTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets which edges of the footer show a separating border. Defaults to <see cref="CardDivider.None"/>.
    /// </summary>
    public CardDivider Divider { get; set; } = CardDivider.None;

    /// <summary>
    /// Additional HTML classes merged with the generated card footer classes.
    /// </summary>
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", CssUtils.MergeClasses(CardCss.FooterClasses(Divider), CssClass));
    }
}
