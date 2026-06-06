using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

public sealed class RichTextEditorLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal RichTextEditorLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>Outer <c>.e-richtexteditor</c> wrapper, parent of the hidden textarea.</summary>
    public ILocator Container => _page.Locator($"#{_componentId}").Locator("xpath=..");

    /// <summary>Contenteditable editing area inside the wrapper.</summary>
    public ILocator Editor => Container.Locator("[contenteditable='true']");

    public async Task Focus() => await Editor.ClickWhenStableAsync(_page);

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

    /// <summary>Fills text and blurs so Syncfusion raises <c>change</c>.</summary>
    public async Task FillAndBlur(string text)
    {
        await Fill(text);
        await Blur();
    }
}
