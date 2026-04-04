using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Support.Controls;

internal sealed class MultiColumnComboBoxLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal MultiColumnComboBoxLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    // ─── Surfaces — What the User Sees ───

    public ILocator Input => _page.Locator($"#{_componentId}");

    public ILocator Wrapper => Input.Locator("xpath=..");

    // ─── Gestures — What the User Does ───

    public async Task Focus() => await Wrapper.ClickWhenStableAsync();

    public async Task Select(string text)
    {
        await _page.Locator("body").ClickAsync(new() { Position = new Position { X = 0, Y = 0 } });
        await Focus();
        await _page.Keyboard.TypeAsync(text);
        await _page.Keyboard.PressAsync("Enter");
    }
}
