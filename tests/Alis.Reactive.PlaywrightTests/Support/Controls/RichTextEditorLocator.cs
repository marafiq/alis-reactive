using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Support.Controls;

internal sealed class RichTextEditorLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal RichTextEditorLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    // ─── Surfaces — What the User Sees ───

    public ILocator Container => _page.Locator($"#{_componentId}").Locator("xpath=..");

    public ILocator Editor => Container.Locator("[contenteditable='true']");

    // ─── Gestures — What the User Does ───

    public async Task Focus() => await Editor.ClickWhenStableAsync();

    public async Task Fill(string text)
    {
        await Focus();
        await _page.Keyboard.PressAsync("Meta+a");
        await _page.Keyboard.TypeAsync(text);
    }

    public async Task Clear()
    {
        await Focus();
        await _page.Keyboard.PressAsync("Meta+a");
        await _page.Keyboard.PressAsync("Backspace");
    }

    public async Task Blur() => await _page.Keyboard.PressAsync("Tab");

    public async Task FillAndBlur(string text)
    {
        await Fill(text);
        await Blur();
    }
}
