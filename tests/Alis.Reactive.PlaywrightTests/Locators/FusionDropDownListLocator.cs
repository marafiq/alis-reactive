using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class FusionDropDownListLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionDropDownListLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Input => _page.Locator($"#{_componentId}");

    public ILocator Wrapper => Input.Locator("xpath=..");

    public ILocator DropDownButton => Wrapper.Locator(".e-ddl-icon");

    public ILocator ClearButton => Wrapper.Locator(".e-clear-icon");

    public ILocator Popup => _page.Locator($"#{_componentId}_popup");

    public ILocator Items => Popup.Locator("li.e-list-item");

    public ILocator GroupHeaders => Popup.Locator("li.e-list-group-item");

    public ILocator Item(string text) => Popup.GetByText(text, new() { Exact = true });

    public async Task Open()
    {
        await DropDownButton.ClickWhenStableAsync(_page);
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
    }

    public async Task Pick(string text)
    {
        var item = Item(text);
        await item.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await item.ClickWhenStableAsync(_page);
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 5000 });
    }

    public async Task Choose(string text)
    {
        await Open();
        await Pick(text);
    }
}
