using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class FusionPivotViewLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionPivotViewLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Table => _page.Locator($"#{_componentId}_grid");

    public ILocator Chart => _page.Locator($"#{_componentId}_chart");

    public ILocator ColumnHeading(string text) =>
        Table.Locator(".e-gridheader").GetByText(text, new() { Exact = true }).First;

    public ILocator RowHeading(string text) =>
        Table.Locator("td.e-rowsheader").GetByText(text, new() { Exact = true });

    public ILocator Figure(string rowHeading, int column) =>
        Table.Locator("tr.e-row")
            .Filter(new() { Has = _page.Locator("td.e-rowsheader").GetByText(rowHeading, new() { Exact = true }) })
            .Locator($"td.e-valuescontent[aria-colindex='{column}']");

    public ILocator ChartMenu => _page.Locator($"#{_componentId}chart_menu .e-menu-item").First;

    public ILocator ChartType(string type) => _page.Locator($"#{_componentId}_{type}");

    public async Task ShowChart(string type)
    {
        await ChartMenu.HoverAsync();
        await ChartType(type).ClickAsync();
    }

    public ILocator ConditionalFormattingDialog => _page.Locator($"#{_componentId}conditionalformatting");

    public ILocator ConditionalFormattingDialogTitle => ConditionalFormattingDialog.Locator(".e-dlg-header");

    public ILocator CalculatedFieldDialog => _page.Locator($"#{_componentId}calculateddialog");

    public ILocator CalculatedFieldDialogTitle => CalculatedFieldDialog.Locator(".e-dlg-header");
}
