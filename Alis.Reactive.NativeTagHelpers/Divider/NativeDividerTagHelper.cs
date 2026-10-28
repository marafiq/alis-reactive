using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Divider;

/// <summary>
/// Renders a <c>native-divider</c> element as a horizontal rule, optionally with a
/// centered label.
/// </summary>
[HtmlTargetElement("native-divider")]
public class NativeDividerTagHelper : TagHelper
{
    /// <summary>
    /// Controls the visual treatment of the rule. Defaults to <see cref="DividerStyle.Plain"/>.
    /// </summary>
    public DividerStyle Style { get; set; } = DividerStyle.Plain;

    /// <summary>
    /// Optional centered label. When set, the divider renders as a labeled section
    /// break instead of a plain rule.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Additional HTML classes merged with the generated divider classes.
    /// </summary>
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var label = Label;
        if (!string.IsNullOrEmpty(label))
        {
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("class", LayoutClasses.Join("alis-divider-labeled", CssClass));
            output.Content.SetHtmlContent(
                "<div class=\"alis-divider-labeled__line\"><div class=\"alis-divider-labeled__rule\"></div></div>" +
                "<div class=\"alis-divider-labeled__text-row\">" +
                $"<span class=\"alis-divider-labeled__text\">{HtmlEncoder.Default.Encode(label)}</span></div>");
            return;
        }

        output.TagName = "hr";
        output.TagMode = TagMode.SelfClosing;
        output.Attributes.SetAttribute("class", LayoutClasses.Join(
            "alis-divider",
            Style == DividerStyle.Dashed ? "alis-divider--dashed" : "",
            CssClass));
    }
}
