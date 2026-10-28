using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Heading;

/// <summary>
/// Renders a <c>native-heading</c> element as a styled heading, optionally preceded by
/// a small overline label.
/// </summary>
[HtmlTargetElement("native-heading")]
public class NativeHeadingTagHelper : TagHelper
{
    /// <summary>
    /// Selects both the heading tag (<c>h1</c> through <c>h6</c>) and its type
    /// scale. Defaults to <see cref="HeadingLevel.H2"/>.
    /// </summary>
    public HeadingLevel Level { get; set; } = HeadingLevel.H2;

    /// <summary>
    /// Controls the bottom margin after the heading. Defaults to <see cref="ElementSpacing.Sm"/>.
    /// </summary>
    public ElementSpacing Spacing { get; set; } = ElementSpacing.Sm;

    /// <summary>
    /// Optional small label shown above the heading.
    /// </summary>
    public string? Overline { get; set; }

    /// <summary>
    /// Additional HTML classes merged with the generated heading classes.
    /// </summary>
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var level = LevelNumber(Level);
        output.TagName = $"h{level}";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", LayoutClasses.Join(
            "alis-heading",
            $"alis-heading--h{level}",
            LayoutClasses.SpaceBelow(Spacing),
            CssClass));

        var overline = Overline;
        if (!string.IsNullOrEmpty(overline))
        {
            output.PreElement.SetHtmlContent(
                $"<p class=\"alis-heading__overline\">{HtmlEncoder.Default.Encode(overline)}</p>");
        }
    }

    private static int LevelNumber(HeadingLevel level) => level switch
    {
        HeadingLevel.H1 or HeadingLevel.H2 or HeadingLevel.H3 or HeadingLevel.H4 or HeadingLevel.H5 or HeadingLevel.H6 => (int)level,
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Not a heading level.")
    };
}
