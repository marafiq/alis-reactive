using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

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

    public ILocator Popup => _page.Locator($"#{_componentId}_popup");

    public ILocator PopupItems => Popup.Locator(".e-list-item");

    public ILocator PopupItem(string text) => PopupItems.GetByText(text, new() { Exact = true });

    public async Task SelectItem(string text, int timeoutMs = 15000)
    {
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        var item = PopupItem(text);
        await item.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        await item.ClickWhenStableAsync(_page, timeoutMs);
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = timeoutMs });
    }
}
