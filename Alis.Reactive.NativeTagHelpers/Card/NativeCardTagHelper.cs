using Alis.Reactive.DesignSystem.Layout;
using Alis.Reactive.DesignSystem.Tokens;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Card;

/// <summary>
/// Renders a <c>native-card</c> element as a styled card surface that holds header,
/// body, and footer sections.
/// </summary>
[HtmlTargetElement("native-card")]
public class NativeCardTagHelper : TagHelper
{
    /// <summary>
    /// Controls how prominently the card lifts off the page. Defaults to <see cref="CardElevation.Low"/>.
    /// </summary>
    public CardElevation Elevation { get; set; } = CardElevation.Low;

    /// <summary>
    /// Optional accent color drawn as a left border on the card.
    /// </summary>
    public AccentColor? Accent { get; set; }

    /// <summary>
    /// Caller-supplied classes appended after the generated card classes.
    /// </summary>
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var cardClasses = CardCss.CardClasses(Elevation);
        if (Accent.HasValue)
            cardClasses = CssUtils.MergeClasses(cardClasses, CardCss.AccentInnerClasses(Accent.Value));
        cardClasses = CssUtils.MergeClasses(cardClasses, CssClass);

        output.Attributes.SetAttribute("class", cardClasses);
    }
}
