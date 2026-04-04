using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Support.Controls;

internal sealed class AutoCompleteLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal AutoCompleteLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    // ─── Surfaces — What the User Sees ───

    public ILocator Input => _page.Locator($"#{_componentId}");

    public ILocator Popup => _page.Locator($"#{_componentId}_popup");

    public ILocator PopupItems => Popup.Locator(".e-list-item");

    public ILocator PopupItem(string text) => PopupItems.GetByText(text, new() { Exact = true });

    // ─── Gestures — What the User Does ───

    public async Task Type(string text, int delayMs = 50)
    {
        await _page.Locator("body").ClickWhenStableAsync();
        await Input.ClickWhenStableAsync();
        await Input.PressAsync("Meta+a");
        await Input.PressAsync("Backspace");
        await _page.Keyboard.TypeAsync(text, new() { Delay = delayMs });
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15000 });
    }

    public async Task Clear()
    {
        await Input.ClickWhenStableAsync();
        await Input.PressAsync("Meta+a");
        await Input.PressAsync("Backspace");
    }

    public async Task SelectItem(string itemText, int timeoutMs = 15000)
    {
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        var item = PopupItem(itemText);
        await item.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        await item.ClickWhenStableAsync();
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = timeoutMs });
    }

    public async Task TypeAndSelect(string searchText, string itemText, int delayMs = 50)
    {
        await Type(searchText, delayMs);
        await SelectItem(itemText);
    }

    public async Task Focus() => await Input.ClickWhenStableAsync();

    public async Task Blur() => await Input.PressAsync("Tab");
}
