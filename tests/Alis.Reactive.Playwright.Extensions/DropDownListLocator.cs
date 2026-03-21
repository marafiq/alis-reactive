using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for Syncfusion DropDownList.
///
/// Verified: keyboard typing + Enter reliably selects items and fires change event.
/// Text-based, not position-based — immune to item reordering.
/// </summary>
public sealed class DropDownListLocator
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

    // ─── Gestures ───

    /// <summary>Focus the dropdown.</summary>
    public async Task Focus() => await Wrapper.ClickAsync();

    /// <summary>
    /// Select by typing text then Enter. SF highlights the match, Enter confirms.
    /// Text-based — immune to item reordering. Fires change event reliably.
    /// </summary>
    public async Task Select(string text)
    {
        await Focus();
        await _page.Keyboard.TypeAsync(text);
        await _page.Keyboard.PressAsync("Enter");
    }
}
