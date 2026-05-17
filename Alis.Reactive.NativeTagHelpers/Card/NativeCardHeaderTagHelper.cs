using Alis.Reactive.DesignSystem.Layout;
using Alis.Reactive.DesignSystem.Tokens;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Card;

/// <summary>
/// Renders a <c>native-card-header</c> element as the header section of a card.
/// </summary>
[HtmlTargetElement("native-card-header", ParentTag = "native-card")]
public class NativeCardHeaderTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets which edges of the header show a separating border. Defaults to <see cref="CardDivider.None"/>.
    /// </summary>
    public CardDivider Divider { get; set; } = CardDivider.None;

    /// <summary>
    /// Gets or sets extra CSS classes appended to the header's design-system classes.
    /// </summary>
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", CssUtils.MergeClasses(CardCss.HeaderClasses(Divider), CssClass));
    }
}
