using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

/// <summary>
/// A Syncfusion MultiColumnComboBox: the input <c>#{componentId}</c>, the input group around it with its
/// dropdown button, and the grid of rows it opens at <c>#{componentId}_options</c>.
/// </summary>
public sealed class FusionMultiColumnComboBoxLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionMultiColumnComboBoxLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The input; it shows the selected row's text.</summary>
    public ILocator Input => _page.Locator($"#{_componentId}");

    /// <summary>The input group; it carries the focus styling while the component is being edited.</summary>
    public ILocator Wrapper => Input.Locator("xpath=..");

    public ILocator DropDownButton => Wrapper.Locator(".e-multicolumn-list-icon");

    public ILocator Popup => _page.Locator($"#{_componentId}_options");

    /// <summary>The column headers of the open list.</summary>
    public ILocator ColumnHeaders => Popup.Locator(".e-headertext");

    /// <summary>The captions of the groups the open list sorts its rows into.</summary>
    public ILocator GroupCaptions => Popup.Locator(".e-groupcaption");

    /// <summary>The row of the open list that holds <paramref name="text"/>.</summary>
    public ILocator Row(string text) => Popup.Locator(".e-row").Filter(new() { HasText = text });

    /// <summary>The cells of a row, column by column.</summary>
    public ILocator Cells(string rowText) => Row(rowText).Locator("td.e-rowcell");

    /// <summary>Opens the list with its dropdown button.</summary>
    public async Task Open()
    {
        await DropDownButton.ClickWhenStableAsync(_page);
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
    }

    /// <summary>Clicks the row that holds <paramref name="text"/> in the open list and waits for the list to close.</summary>
    public async Task Pick(string text)
    {
        var row = Row(text);
        await row.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await row.ClickWhenStableAsync(_page);
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 5000 });
    }

    /// <summary>Opens the list and picks the row that holds <paramref name="text"/>.</summary>
    public async Task Choose(string text)
    {
        await Open();
        await Pick(text);
    }
}
