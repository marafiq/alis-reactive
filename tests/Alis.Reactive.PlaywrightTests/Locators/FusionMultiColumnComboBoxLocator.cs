using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class FusionMultiColumnComboBoxLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionMultiColumnComboBoxLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Input => _page.Locator($"#{_componentId}");

    public ILocator Wrapper => Input.Locator("xpath=..");

    public ILocator DropDownButton => Wrapper.Locator(".e-multicolumn-list-icon");

    public ILocator Popup => _page.Locator($"#{_componentId}_options");

    public ILocator ColumnHeaders => Popup.Locator(".e-headertext");

    public ILocator GroupCaptions => Popup.Locator(".e-groupcaption");

    public ILocator Row(string text) => Popup.Locator(".e-row").Filter(new() { HasText = text });

    public ILocator Cells(string rowText) => Row(rowText).Locator("td.e-rowcell");

    public async Task Open()
    {
        await DropDownButton.ClickWhenStableAsync(_page);
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
    }

    public async Task Pick(string text)
    {
        var row = Row(text);
        await row.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await row.ClickWhenStableAsync(_page);
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 5000 });
    }

    public async Task Choose(string text)
    {
        await Open();
        await Pick(text);
    }
}
