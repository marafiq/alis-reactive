using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// A Syncfusion PivotView: its table and chart, a figure by its row heading and column, the chart types its
/// toolbar offers, and the dialogs it opens.
/// </summary>
public sealed class FusionPivotViewLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionPivotViewLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The pivot table; hidden while the pivot shows its chart.</summary>
    public ILocator Table => _page.Locator($"#{_componentId}_grid");

    /// <summary>The pivot chart; hidden while the pivot shows its table.</summary>
    public ILocator Chart => _page.Locator($"#{_componentId}_chart");

    /// <summary>A column heading of the table, such as a month or a measure.</summary>
    public ILocator ColumnHeading(string text) =>
        Table.Locator(".e-gridheader").GetByText(text, new() { Exact = true }).First;

    /// <summary>A row heading of the table, such as a wing, a care level or the grand total.</summary>
    public ILocator RowHeading(string text) =>
        Table.Locator("td.e-rowsheader").GetByText(text, new() { Exact = true });

    /// <summary>
    /// The figure in the row with this heading and in this column. The pivot numbers its columns from 1,
    /// the row headings being column 1, then each column member's measures in order.
    /// </summary>
    public ILocator Figure(string rowHeading, int column) =>
        Table.Locator("tr.e-row")
            .Filter(new() { Has = _page.Locator("td.e-rowsheader").GetByText(rowHeading, new() { Exact = true }) })
            .Locator($"td.e-valuescontent[aria-colindex='{column}']");

    /// <summary>The chart button of the pivot's toolbar; pointing at it lists the chart types.</summary>
    public ILocator ChartMenu => _page.Locator($"#{_componentId}chart_menu .e-menu-item").First;

    /// <summary>A chart type in the toolbar's chart list, by the type's name (<c>Column</c>, <c>Bar</c>, <c>Line</c>).</summary>
    public ILocator ChartType(string type) => _page.Locator($"#{_componentId}_{type}");

    /// <summary>Points at the toolbar's chart button and picks a chart type; the pivot then shows its chart.</summary>
    public async Task ShowChart(string type)
    {
        await ChartMenu.HoverAsync();
        await ChartType(type).ClickAsync();
    }

    /// <summary>The conditional formatting dialog the pivot opens over itself.</summary>
    public ILocator ConditionalFormattingDialog => _page.Locator($"#{_componentId}conditionalformatting");

    /// <summary>The title of the conditional formatting dialog.</summary>
    public ILocator ConditionalFormattingDialogTitle => ConditionalFormattingDialog.Locator(".e-dlg-header");

    /// <summary>The calculated field dialog the pivot opens over itself.</summary>
    public ILocator CalculatedFieldDialog => _page.Locator($"#{_componentId}calculateddialog");

    /// <summary>The title of the calculated field dialog.</summary>
    public ILocator CalculatedFieldDialogTitle => CalculatedFieldDialog.Locator(".e-dlg-header");
}
