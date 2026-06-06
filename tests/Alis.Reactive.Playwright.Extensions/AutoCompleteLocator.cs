using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Syncfusion AutoComplete uses <c>#{componentId}</c> input and <c>#{componentId}_popup</c> popup IDs.
/// </summary>
public sealed class AutoCompleteLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal AutoCompleteLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Input => _page.Locator($"#{_componentId}");

    public ILocator Popup => _page.Locator($"#{_componentId}_popup");

    public ILocator PopupItems => Popup.Locator(".e-list-item");

    public ILocator PopupItem(string text) => PopupItems.GetByText(text, new() { Exact = true });

    /// <summary>
    /// Uses sequential key presses because Syncfusion filtering ignores FillAsync.
    /// </summary>
    public async Task Type(string text, int delayMs = 50)
    {
        try
        {
            await TypeAndWaitForPopup(text, delayMs);
        }
        catch (TimeoutException)
        {
            await Clear();
            // TODO: Replace this fixed retry pause with a Syncfusion popup-ready signal.
            await _page.WaitForTimeoutAsync(250);
            await TypeAndWaitForPopup(text, delayMs);
        }
    }

    private async Task TypeAndWaitForPopup(string text, int delayMs)
    {
        await Input.ClickWhenStableAsync(_page);
        await Input.PressSequentiallyAsync(text, new() { Delay = delayMs });
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15000 });
    }

    public async Task Clear()
    {
        await Input.ClickWhenStableAsync(_page);
        await Input.PressAsync("Meta+a");
        await Input.PressAsync("Backspace");
    }

    public async Task SelectItem(string itemText, int timeoutMs = 15000)
    {
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        var item = PopupItem(itemText);
        await item.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        await item.ClickWhenStableAsync(_page, timeoutMs);
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = timeoutMs });
    }

    public async Task TypeAndSelect(string searchText, string itemText, int delayMs = 50)
    {
        await Type(searchText, delayMs);
        await SelectItem(itemText);
    }

    public async Task Focus() => await Input.ClickWhenStableAsync(_page);

    public async Task Blur() => await Input.PressAsync("Tab");
}
