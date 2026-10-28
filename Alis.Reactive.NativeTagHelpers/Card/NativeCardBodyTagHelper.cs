using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Card;

/// <summary>
/// Renders a <c>native-card-body</c> element as the main content section of a card.
/// </summary>
[HtmlTargetElement("native-card-body", ParentTag = "native-card")]
public class NativeCardBodyTagHelper : TagHelper
{
    /// <summary>
    /// Controls the inner padding of the body section. Defaults to <see cref="CardPadding.Standard"/>.
    /// </summary>
    public CardPadding Padding { get; set; } = CardPadding.Standard;

    /// <summary>
    /// Caller-supplied classes appended after the generated card body classes.
    /// </summary>
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", LayoutClasses.Join("alis-card__body", PaddingClass(Padding), CssClass));
    }

    private static string PaddingClass(CardPadding padding) => padding switch
    {
        CardPadding.None => "",
        CardPadding.Compact => "alis-card__body--compact",
        CardPadding.Standard => "alis-card__body--standard",
        _ => throw new ArgumentOutOfRangeException(nameof(padding), padding, "Not a card padding.")
    };
}
