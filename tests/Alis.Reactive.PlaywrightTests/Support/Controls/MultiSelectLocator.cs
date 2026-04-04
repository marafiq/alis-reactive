using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Support.Controls;

internal sealed class MultiSelectLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal MultiSelectLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    // ─── Surfaces — What the User Sees ───

    public ILocator Wrapper => _page.Locator($"#{_componentId}").Locator("xpath=ancestor::div[contains(@class,'e-multi-select-wrapper')]");

    public ILocator Popup => _page.Locator($"#{_componentId.Split("__").Last()}_popup");

    public ILocator PopupItems => Popup.Locator(".e-list-item");

    public ILocator PopupItem(string text) =>
        PopupItems.Filter(new() { HasText = text });

    // ─── Gestures — What the User Does ───

    public async Task Open()
    {
        await Wrapper.ClickWhenStableAsync();
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
    }

    public async Task SelectItem(string itemText)
    {
        await Open();
        var option = Popup.Locator(".e-list-item").GetByText(itemText, new() { Exact = true });
        await option.ClickWhenStableAsync();
        // Wait for popup to close (closePopupOnSelect: true in Box mode)
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 5000 });
        // Click outside the component to blur — this triggers the SF change event
        await _page.Locator("body").ClickWhenStableAsync();
    }

    public async Task SelectItems(params string[] itemTexts)
    {
        foreach (var text in itemTexts)
        {
            await Open();
            var option = Popup.Locator(".e-list-item").GetByText(text, new() { Exact = true });
            await option.ClickWhenStableAsync();
            // Wait for popup to close after selection
            await Popup.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 5000 });
        }
        // Click outside the component to blur — triggers the SF change event
        await _page.Locator("body").ClickWhenStableAsync();
    }

    public async Task Blur() => await _page.Keyboard.PressAsync("Escape");
}
