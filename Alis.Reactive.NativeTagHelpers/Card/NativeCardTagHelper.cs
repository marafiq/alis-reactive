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
    /// Gets or sets how prominently the card lifts off the page. Defaults to <see cref="CardElevation.Low"/>.
    /// </summary>
    public CardElevation Elevation { get; set; } = CardElevation.Low;

    /// <summary>
    /// Gets or sets an optional accent color drawn as a left border on the card.
    /// </summary>
    public AccentColor? Accent { get; set; }

    /// <summary>
    /// Gets or sets extra CSS classes appended to the card's design-system classes.
    /// </summary>
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        var classes = CardCss.CardClasses(Elevation);
        if (Accent.HasValue)
            classes = CssUtils.MergeClasses(classes, CardCss.AccentInnerClasses(Accent.Value));
        classes = CssUtils.MergeClasses(classes, CssClass);

        output.Attributes.SetAttribute("class", classes);
    }
}
