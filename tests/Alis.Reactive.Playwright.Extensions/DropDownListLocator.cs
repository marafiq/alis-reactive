using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for FusionDropDownList.
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
    /// Open the popup and click the exact list item text.
    /// This keeps the interaction purely user-driven without depending on keyboard
    /// focus state from a prior open popup.
    /// </summary>
    public async Task Select(string text)
    {
        await _page.Locator("body").ClickAsync(new() { Position = new Position { X = 0, Y = 0 } });

        await DropdownIcon.ClickWhenStableAsync(_page);
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });

        var option = Popup.GetByText(text, new() { Exact = true });
        await option.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await option.ClickWhenStableAsync(_page);
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 5000 });
    }
}
