using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Card;

/// <summary>
/// Renders a <c>native-card-footer</c> element as the footer section of a card.
/// </summary>
[HtmlTargetElement("native-card-footer", ParentTag = "native-card")]
public class NativeCardFooterTagHelper : TagHelper
{
    /// <summary>
    /// Selects which footer edges show a separating border. Defaults to <see cref="CardDivider.None"/>.
    /// </summary>
    public CardDivider Divider { get; set; } = CardDivider.None;

    /// <summary>
    /// Caller-supplied classes appended after the generated card footer classes.
    /// </summary>
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var isDivided = Divider == CardDivider.Footer || Divider == CardDivider.Both;
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", LayoutClasses.Join(
            "alis-card__footer",
            isDivided ? "alis-card__footer--divided" : "",
            CssClass));
    }
}
