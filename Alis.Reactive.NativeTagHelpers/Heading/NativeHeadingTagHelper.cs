using System.Text.Encodings.Web;
using Alis.Reactive.DesignSystem.Layout;
using Alis.Reactive.DesignSystem.Tokens;
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
    /// Gets or sets the heading level, which selects both the tag (<c>h1</c> through
    /// <c>h6</c>) and its type scale. Defaults to <see cref="HeadingLevel.H2"/>.
    /// </summary>
    public HeadingLevel Level { get; set; } = HeadingLevel.H2;

    /// <summary>
    /// Gets or sets the bottom margin applied after the heading. Defaults to <see cref="ElementSpacing.Sm"/>.
    /// </summary>
    public ElementSpacing Spacing { get; set; } = ElementSpacing.Sm;

    /// <summary>
    /// Gets or sets an optional small label shown above the heading.
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
        output.TagName = $"h{(int)Level}";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", HeadingCss.Classes(Level, Spacing, CssClass));

        var overline = Overline;
        if (!string.IsNullOrEmpty(overline))
        {
            output.PreElement.SetHtmlContent(
                $"<p class=\"{HeadingCss.OverlineClasses()}\">{HtmlEncoder.Default.Encode(overline)}</p>");
        }
    }
}
