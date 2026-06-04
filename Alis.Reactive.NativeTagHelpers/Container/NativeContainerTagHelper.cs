using Alis.Reactive.DesignSystem.Layout;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Container;

/// <summary>
/// Renders a <c>native-container</c> element as a page-width container that centers
/// its content and caps its maximum width.
/// </summary>
[HtmlTargetElement("native-container")]
public class NativeContainerTagHelper : TagHelper
{
    /// <summary>
    /// Additional HTML classes merged with the generated container classes.
    /// </summary>
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", ContainerCss.Classes(CssClass));
    }
}
