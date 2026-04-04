using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Support.Controls;

internal sealed class DropDownListLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal DropDownListLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    // ─── Surfaces ───

    public ILocator Input => _page.Locator($"#{_componentId}");
    public ILocator Wrapper => Input.Locator("xpath=..");

    public ILocator DropdownIcon => _page.Locator($"#{_componentId}").Locator("..").Locator(".e-ddl-icon");

    public ILocator Popup => _page.Locator($"#{_componentId}_popup");

    // ─── Gestures ───

    public async Task Open() => await DropdownIcon.ClickAsync();

    public async Task Focus() => await Input.ClickAsync();

    public async Task Select(string text)
    {
        await _page.Locator("body").ClickAsync(new() { Position = new Position { X = 0, Y = 0 } });

        await DropdownIcon.ClickWhenStableAsync();
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });

        var option = Popup.GetByText(text, new() { Exact = true });
        await option.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await option.ClickWhenStableAsync();
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 5000 });
    }
}
