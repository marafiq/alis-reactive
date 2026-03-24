using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for Syncfusion MultiColumnComboBox.
/// Like DropDownList — click wrapper to open, type text to filter, Enter to select.
/// Does NOT provide assertions — the test decides what to verify.
///
/// Usage:
///   var mccb = plan.MultiColumnComboBox(m => m.FacilityId);
///
///   // Gesture
///   await mccb.Select("Sunrise Manor");
///
///   // Surface → test asserts
///   await Expect(mccb.Input).ToHaveValueAsync("Sunrise Manor");
/// </summary>
public sealed class MultiColumnComboBoxLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    internal MultiColumnComboBoxLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    // ─── Surfaces — What the User Sees ───

    /// <summary>The combo box input field.</summary>
    public ILocator Input => _page.Locator($"#{_componentId}");

    /// <summary>The wrapper element around the input.</summary>
    public ILocator Wrapper => Input.Locator("xpath=..");

    // ─── Gestures — What the User Does ───

    /// <summary>Click the wrapper to focus/open the dropdown.</summary>
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
