using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class FusionGridLocator
{
    private readonly IPage _page;
    private readonly string _gridId;

    public FusionGridLocator(IPage page, string gridId)
    {
        _page = page;
        _gridId = gridId;
    }

    public ILocator Root => _page.Locator($"#{_gridId}");

    public ILocator Rows => Root.Locator(".e-row");

    public ILocator Row(string text) => Rows.Filter(new() { HasText = text });

    public ILocator Cell(string rowText, int column) => Row(rowText).Locator($"td[aria-colindex='{column}']");

    public ILocator SelectedRows => Root.Locator(".e-row[aria-selected='true']");

    public ILocator VisibleHeaders => Root.Locator(".e-headercell:visible .e-headertext");

    public ILocator Header(string text) =>
        Root.Locator(".e-headercell .e-headertext").Filter(new() { HasTextRegex = new Regex("^" + Regex.Escape(text) + "$") });

    public ILocator PagerPage(int number) =>
        Root.Locator(".e-pager .e-numericitem").Filter(new() { HasTextRegex = new Regex($"^{number}$") });

    public ILocator PageSizeChoice => Root.Locator(".e-pager .e-pagerdropdown");

    public async Task ChoosePageSize(int size)
    {
        await PageSizeChoice.ClickWhenStableAsync(_page);
        var option = _page.Locator(".e-ddl.e-popup.e-popup-open li").Filter(new() { HasTextRegex = new Regex($"^{size}$") });
        await option.ClickWhenStableAsync(_page);
    }
}
