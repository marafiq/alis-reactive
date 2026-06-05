using Alis.Reactive.DesignSystem.Layout;
using Alis.Reactive.DesignSystem.Tokens;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.HStack;

/// <summary>
/// Renders a <c>native-hstack</c> element as a horizontal flex row that lays its
/// children out side by side.
/// </summary>
[HtmlTargetElement("native-hstack")]
public class NativeHStackTagHelper : TagHelper
{
    /// <summary>
    /// Controls the spacing between children. Defaults to <see cref="SpacingScale.Base"/>.
    /// </summary>
    public SpacingScale Gap { get; set; } = SpacingScale.Base;

    /// <summary>
    /// Controls how children align on the cross axis. Defaults to <see cref="AlignItems.Center"/>.
    /// </summary>
    public AlignItems Align { get; set; } = AlignItems.Center;

    /// <summary>
    /// Controls how children are distributed along the row. Defaults to <see cref="JustifyContent.Start"/>.
    /// </summary>
    public JustifyContent Justify { get; set; } = JustifyContent.Start;

    /// <summary>
    /// Allows children to wrap onto multiple lines when they overflow the row.
    /// </summary>
    public bool Wrap { get; set; }

    /// <summary>
    /// Additional HTML classes merged with the generated row classes.
    /// </summary>
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", HStackCss.Classes(Gap, Align, Justify, Wrap, CssClass));
    }
}
