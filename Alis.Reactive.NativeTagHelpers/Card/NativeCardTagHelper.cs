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
        output.Attributes.SetAttribute("class", LayoutClasses.Join(
            "alis-card",
            ElevationClass(Elevation),
            Accent.HasValue ? "alis-card--accent " + AccentClass(Accent.Value) : "",
            CssClass));
    }

    private static string ElevationClass(CardElevation elevation) => elevation switch
    {
        CardElevation.Flat => "alis-card--flat",
        CardElevation.Low => "alis-card--low",
        CardElevation.Medium => "alis-card--medium",
        CardElevation.High => "alis-card--high",
        _ => throw new ArgumentOutOfRangeException(nameof(elevation), elevation, "Not a card elevation.")
    };

    private static string AccentClass(AccentColor accent) => accent switch
    {
        AccentColor.Primary => "alis-card--accent-primary",
        AccentColor.Secondary => "alis-card--accent-secondary",
        AccentColor.Success => "alis-card--accent-success",
        AccentColor.Warning => "alis-card--accent-warning",
        AccentColor.Error => "alis-card--accent-error",
        AccentColor.Info => "alis-card--accent-info",
        AccentColor.Muted => "alis-card--accent-muted",
        _ => throw new ArgumentOutOfRangeException(nameof(accent), accent, "Not an accent color.")
    };
}
