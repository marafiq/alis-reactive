using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Card;

/// <summary>
/// Renders a <c>native-card-header</c> element as the header section of a card.
/// </summary>
[HtmlTargetElement("native-card-header", ParentTag = "native-card")]
public class NativeCardHeaderTagHelper : TagHelper
{
    /// <summary>
    /// Selects which header edges show a separating border. Defaults to <see cref="CardDivider.None"/>.
    /// </summary>
    public CardDivider Divider { get; set; } = CardDivider.None;

    /// <summary>
    /// Caller-supplied classes appended after the generated card header classes.
    /// </summary>
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var isDivided = Divider == CardDivider.Header || Divider == CardDivider.Both;
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", LayoutClasses.Join(
            "alis-card__header",
            isDivided ? "alis-card__header--divided" : "",
            CssClass));
    }
}
