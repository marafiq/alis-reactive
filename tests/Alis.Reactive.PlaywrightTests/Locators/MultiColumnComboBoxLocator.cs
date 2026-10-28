using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

public sealed class MultiColumnComboBoxLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal MultiColumnComboBoxLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Input => _page.Locator($"#{_componentId}");

    public ILocator Wrapper => Input.Locator("xpath=..");

    public async Task Focus() => await Wrapper.ClickWhenStableAsync(_page);

    /// <summary>
    /// Selects by typing text and pressing Enter so row ordering does not affect the gesture.
    /// </summary>
    public async Task Select(string text)
    {
        // Clear prior focus before opening; Syncfusion keeps popup/focus state internally.
        await _page.Locator("body").ClickAsync(new() { Position = new Position { X = 0, Y = 0 } });
        await Focus();
        await _page.Keyboard.TypeAsync(text);
        await _page.Keyboard.PressAsync("Enter");
    }
}
