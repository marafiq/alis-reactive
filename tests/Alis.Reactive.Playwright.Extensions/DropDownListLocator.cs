using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Syncfusion DropDownList uses <c>#{componentId}_popup</c> with <c>.e-list-item</c> options.
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

    public ILocator Input => _page.Locator($"#{_componentId}");
    public ILocator Wrapper => Input.Locator("xpath=..");

    public ILocator DropdownIcon => _page.Locator($"#{_componentId}").Locator("..").Locator(".e-ddl-icon");

    public ILocator Popup => _page.Locator($"#{_componentId}_popup");

    public async Task Open() => await DropdownIcon.ClickAsync();

    public async Task Focus() => await Input.ClickAsync();

    /// <summary>
    /// Selects the exact option text without depending on focus from a prior popup.
    /// </summary>
    public async Task Select(string text)
    {
        // Clear prior focus before opening; Syncfusion keeps popup/focus state internally.
        await _page.Locator("body").ClickAsync(new() { Position = new Position { X = 0, Y = 0 } });

        await DropdownIcon.ClickWhenStableAsync(_page);
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });

        var option = Popup.GetByText(text, new() { Exact = true });
        await option.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await option.ClickWhenStableAsync(_page);
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 5000 });
    }
}
