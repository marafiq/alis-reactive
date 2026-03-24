using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for Syncfusion DropDownList.
///
/// Verified: click input to open popup, click list item to select.
/// SF DDL popup: #{componentId}_popup contains .e-list-item elements.
/// </summary>
public sealed class DropDownListLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public DropDownListLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    // ─── Surfaces ───

    public ILocator Input => _page.Locator($"#{_componentId}");
    public ILocator Wrapper => Input.Locator("xpath=..");

    /// <summary>The dropdown arrow icon that opens the popup.</summary>
    public ILocator DropdownIcon => _page.Locator($"#{_componentId}").Locator("..").Locator(".e-ddl-icon");

    /// <summary>The popup container (visible after opening the dropdown).</summary>
    public ILocator Popup => _page.Locator($"#{_componentId}_popup");

    // ─── Gestures ───

    /// <summary>Click the dropdown icon to open the popup.</summary>
    public async Task Open() => await DropdownIcon.ClickAsync();

    /// <summary>Click the input to focus it.</summary>
    public async Task Focus() => await Input.ClickAsync();

    /// <summary>
    /// Open the popup via the dropdown icon, navigate with ArrowDown to the item, press Enter.
    /// SF DDL responds to keyboard navigation natively — ArrowDown highlights items,
    /// Enter confirms the selection and fires the change event.
    /// </summary>
    public async Task Select(string text)
    {
        // Clear stale focus from any prior DDL — prevents keyboard events going to wrong popup
        await _page.Locator("body").ClickAsync(new() { Position = new Position { X = 0, Y = 0 } });

        await DropdownIcon.ClickAsync();
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });

        // Navigate with ArrowDown until the highlighted item matches the target text
        var items = Popup.Locator(".e-list-item");
        var count = await items.CountAsync();
        for (var i = 0; i < count; i++)
        {
            await _page.Keyboard.PressAsync("ArrowDown");
            var active = Popup.Locator(".e-list-item.e-active, .e-list-item.e-item-focus");
            var activeText = await active.TextContentAsync();
            if (activeText?.Trim() == text)
            {
                await _page.Keyboard.PressAsync("Enter");
                return;
            }
        }

        // Fallback: if exact match not found, just press Enter on whatever is highlighted
        await _page.Keyboard.PressAsync("Enter");
    }
}
