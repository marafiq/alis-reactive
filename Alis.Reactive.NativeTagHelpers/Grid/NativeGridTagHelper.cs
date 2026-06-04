using Alis.Reactive.DesignSystem.Layout;
using Alis.Reactive.DesignSystem.Tokens;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Alis.Reactive.NativeTagHelpers.Grid;

/// <summary>
/// Renders a <c>native-grid</c> element as a CSS grid that arranges its children into
/// columns.
/// </summary>
[HtmlTargetElement("native-grid")]
public class NativeGridTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the number of columns. Defaults to <see cref="GridCols.C2"/>.
    /// </summary>
    public GridCols Cols { get; set; } = GridCols.C2;

    /// <summary>
    /// Gets or sets the spacing between grid cells. Defaults to <see cref="SpacingScale.Md"/>.
    /// </summary>
    public SpacingScale Gap { get; set; } = SpacingScale.Md;

    /// <summary>
    /// Gets or sets whether the column count scales down on smaller screens. Defaults to <see langword="true"/>.
    /// </summary>
    public bool Responsive { get; set; } = true;

    /// <summary>
    /// Additional HTML classes merged with the generated grid classes.
    /// </summary>
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", Responsive
            ? GridCss.ResponsiveClasses(Cols, Gap, CssClass)
            : GridCss.Classes(Cols, Gap, CssClass));
    }
}
