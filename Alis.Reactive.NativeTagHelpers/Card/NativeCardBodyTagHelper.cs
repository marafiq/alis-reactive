using Alis.Reactive.DesignSystem.Layout;
using Alis.Reactive.DesignSystem.Tokens;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Card;

/// <summary>
/// Renders a <c>native-card-body</c> element as the main content section of a card.
/// </summary>
[HtmlTargetElement("native-card-body", ParentTag = "native-card")]
public class NativeCardBodyTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the inner padding of the body section. Defaults to <see cref="CardPadding.Standard"/>.
    /// </summary>
    public CardPadding Padding { get; set; } = CardPadding.Standard;

    /// <summary>
    /// Gets or sets extra CSS classes appended to the body's design-system classes.
    /// </summary>
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", CssUtils.MergeClasses(CardCss.BodyClasses(Padding), CssClass));
    }
}
