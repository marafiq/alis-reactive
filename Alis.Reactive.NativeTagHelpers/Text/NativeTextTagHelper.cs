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
        output.Attributes.SetAttribute("class", LayoutClasses.Join(
            "alis-text",
            SizeClass(Size),
            ColorClass(Color),
            Bold ? "alis-text--bold" : "",
            LayoutClasses.SpaceBelow(Spacing),
            CssClass));
    }

    private static string SizeClass(TextSize size) => size switch
    {
        TextSize.Xs => "alis-text--xs",
        TextSize.Sm => "alis-text--sm",
        TextSize.Base => "alis-text--base",
        TextSize.Lg => "alis-text--lg",
        TextSize.Xl => "alis-text--xl",
        _ => throw new ArgumentOutOfRangeException(nameof(size), size, "Not a text size.")
    };

    private static string ColorClass(TextColor color) => color switch
    {
        TextColor.Primary => "alis-text--primary",
        TextColor.Secondary => "alis-text--secondary",
        TextColor.Muted => "alis-text--muted",
        TextColor.Inverse => "alis-text--inverse",
        TextColor.Accent => "alis-text--accent",
        TextColor.Success => "alis-text--success",
        TextColor.Warning => "alis-text--warning",
        TextColor.Error => "alis-text--error",
        TextColor.Inherit => "alis-text--inherit",
        _ => throw new ArgumentOutOfRangeException(nameof(color), color, "Not a text color.")
    };
}
