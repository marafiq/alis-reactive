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
    /// Controls the number of grid columns. Defaults to <see cref="GridCols.C2"/>.
    /// </summary>
    public GridCols Cols { get; set; } = GridCols.C2;

    /// <summary>
    /// Controls the spacing between grid cells. Defaults to <see cref="SpacingScale.Md"/>.
    /// </summary>
    public SpacingScale Gap { get; set; } = SpacingScale.Md;

    /// <summary>
    /// Scales the column count down on smaller screens: one column on a phone, then up to two, then
    /// <see cref="Cols"/> on a wide screen. Defaults to <see langword="true"/>.
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
        var columns = Responsive ? "alis-grid--responsive-" : "alis-grid--cols-";
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", LayoutClasses.Join(
            "alis-grid",
            columns + ColumnCount(Cols),
            LayoutClasses.Gap(Gap),
            CssClass));
    }

    private static int ColumnCount(GridCols cols) => cols switch
    {
        GridCols.C1 or GridCols.C2 or GridCols.C3 or GridCols.C4 or GridCols.C5 or GridCols.C6 => (int)cols,
        _ => throw new ArgumentOutOfRangeException(nameof(cols), cols, "Not a grid column count.")
    };
}
