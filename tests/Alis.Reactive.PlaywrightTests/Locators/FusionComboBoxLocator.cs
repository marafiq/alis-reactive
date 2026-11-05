using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class FusionComboBoxLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionComboBoxLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Input => _page.Locator($"#{_componentId}");

    public ILocator Wrapper => Input.Locator("xpath=..");

    public ILocator DropDownButton => Wrapper.Locator(".e-ddl-icon");

    public ILocator ClearButton => Wrapper.Locator(".e-clear-icon");

    public ILocator Popup => _page.Locator($"#{_componentId}_popup");

    public ILocator PopupItems => Popup.Locator(".e-list-item");

    public ILocator HighlightedItem => Popup.Locator(".e-list-item.e-item-focus");

    public async Task TypeAndPressEnter(string text, string item, int timeoutMs = 15000)
    {
        await Input.ClickWhenStableAsync(_page);
        await Input.PressSequentiallyAsync(text, new() { Delay = 50 });
        await HighlightedItem.Filter(new() { HasText = item })
            .WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        await Input.PressAsync("Enter");
    }
}
