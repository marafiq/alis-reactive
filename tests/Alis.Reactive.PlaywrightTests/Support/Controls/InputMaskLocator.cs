using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Support.Controls;

internal sealed class InputMaskLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal InputMaskLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    // ─── Surfaces — What the User Sees ───

    public ILocator Input => _page.Locator($"#{_componentId}");

    // ─── Gestures — What the User Does ───

    public async Task Fill(string value)
    {
        await Input.FillAsync(value);
    }

    public async Task Clear()
    {
        await Input.ClickAsync();
        await Input.PressAsync("Meta+a");
        await Input.PressAsync("Backspace");
    }

    public async Task Focus() => await Input.ClickAsync();

    public async Task Blur() => await Input.PressAsync("Tab");

    public async Task FillAndBlur(string value)
    {
        await Fill(value);
        await Blur();
    }
}
