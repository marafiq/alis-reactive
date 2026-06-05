using Alis.Reactive.DesignSystem.Layout;
using Alis.Reactive.DesignSystem.Tokens;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Text;

/// <summary>
/// Renders a <c>native-text</c> element as a styled paragraph or inline span.
/// </summary>
[HtmlTargetElement("native-text")]
public class NativeTextTagHelper : TagHelper
{
    /// <summary>
    /// Controls the rendered text size. Defaults to <see cref="TextSize.Base"/>.
    /// </summary>
    public TextSize Size { get; set; } = TextSize.Base;

    /// <summary>
    /// Controls the text color. Defaults to <see cref="TextColor.Primary"/>.
    /// </summary>
    public TextColor Color { get; set; } = TextColor.Primary;

    /// <summary>
    /// Renders the text with bold weight.
    /// </summary>
    public bool Bold { get; set; }

    /// <summary>
    /// Controls the bottom margin after the text. Defaults to <see cref="ElementSpacing.Base"/>.
    /// </summary>
    public ElementSpacing Spacing { get; set; } = ElementSpacing.Base;

    /// <summary>
    /// Renders an inline <c>span</c> instead of a block <c>p</c>.
    /// </summary>
    public bool AsSpan { get; set; }

    /// <summary>
    /// Additional HTML classes merged with the generated text classes.
    /// </summary>
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = AsSpan ? "span" : "p";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", TextCss.Classes(Size, Color, Bold, Spacing, CssClass));
    }
}
