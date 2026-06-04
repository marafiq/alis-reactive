using Alis.Reactive.DesignSystem.Layout;
using Alis.Reactive.DesignSystem.Tokens;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.VStack;

/// <summary>
/// Renders a <c>native-vstack</c> element as a vertical flex column that stacks its
/// children top to bottom.
/// </summary>
[HtmlTargetElement("native-vstack")]
public class NativeVStackTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the spacing between children. Defaults to <see cref="SpacingScale.Base"/>.
    /// </summary>
    public SpacingScale Gap { get; set; } = SpacingScale.Base;

    /// <summary>
    /// Gets or sets whether a separating border is drawn between children.
    /// </summary>
    public bool DivideY { get; set; }

    /// <summary>
    /// Additional HTML classes merged with the generated column classes.
    /// </summary>
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", VStackCss.Classes(Gap, DivideY, CssClass));
    }
}
