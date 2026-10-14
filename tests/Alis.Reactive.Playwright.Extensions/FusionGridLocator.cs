using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// A Syncfusion Grid: its rows, a row by the text it carries, a cell of a row, the rows it shows selected, the
/// headers of the columns it shows, a header to click, its pager and its choice of page size.
/// </summary>
/// <remarks>
/// The grid numbers a row's cells from 1 in <c>aria-colindex</c>, hidden columns included; a cell keeps its
/// number when columns are hidden or moved.
/// </remarks>
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

    /// <summary>Every row of the page the grid shows, in order.</summary>
    public ILocator Rows => Root.Locator(".e-row");

    /// <summary>The row that carries <paramref name="text"/>, such as a resident's name.</summary>
    public ILocator Row(string text) => Rows.Filter(new() { HasText = text });

    /// <summary>The cell of the row that carries <paramref name="rowText"/> in the column numbered <paramref name="column"/>.</summary>
    public ILocator Cell(string rowText, int column) => Row(rowText).Locator($"td[aria-colindex='{column}']");

    /// <summary>The rows the grid shows selected, in order.</summary>
    public ILocator SelectedRows => Root.Locator(".e-row[aria-selected='true']");

    /// <summary>The header text of every column the grid shows, in order.</summary>
    public ILocator VisibleHeaders => Root.Locator(".e-headercell:visible .e-headertext");

    /// <summary>The header text <paramref name="text"/>, exactly; clicking it sorts by that column.</summary>
    /// <remarks>The header cell also carries text for screen readers ("Press Enter to sort"), so its text is not the header's.</remarks>
    public ILocator Header(string text) =>
        Root.Locator(".e-headercell .e-headertext").Filter(new() { HasTextRegex = new Regex("^" + Regex.Escape(text) + "$") });

    /// <summary>The pager's link to page <paramref name="number"/>.</summary>
    public ILocator PagerPage(int number) =>
        Root.Locator(".e-pager .e-numericitem").Filter(new() { HasTextRegex = new Regex($"^{number}$") });

    /// <summary>The pager's choice of how many rows a page shows.</summary>
    public ILocator PageSizeChoice => Root.Locator(".e-pager .e-pagerdropdown");

    /// <summary>Opens the pager's choice of page size and picks <paramref name="size"/> rows a page.</summary>
    /// <remarks>The choice's list opens in a popup on the page body, not inside the grid.</remarks>
    public async Task ChoosePageSize(int size)
    {
        await PageSizeChoice.ClickWhenStableAsync(_page);
        var option = _page.Locator(".e-ddl.e-popup.e-popup-open li").Filter(new() { HasTextRegex = new Regex($"^{size}$") });
        await option.ClickWhenStableAsync(_page);
    }
}
