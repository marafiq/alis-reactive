using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// A Syncfusion ComboBox: the input <c>#{componentId}</c>, the input group around it with its clear and
/// dropdown buttons, and the list it opens at <c>#{componentId}_popup</c>.
/// </summary>
public sealed class FusionComboBoxLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionComboBoxLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Input => _page.Locator($"#{_componentId}");

    public ILocator Wrapper => Input.Locator("xpath=..");

    public ILocator DropDownButton => Wrapper.Locator(".e-ddl-icon");

    /// <summary>The cross that clears the input; the ComboBox shows it while the input is focused and holds text.</summary>
    public ILocator ClearButton => Wrapper.Locator(".e-clear-icon");

    public ILocator Popup => _page.Locator($"#{_componentId}_popup");

    public ILocator PopupItems => Popup.Locator(".e-list-item");

    /// <summary>The item the list highlights for what was typed.</summary>
    public ILocator HighlightedItem => Popup.Locator(".e-list-item.e-item-focus");

    /// <summary>
    /// Types into the input key by key, as the ComboBox searches its list on each key, waits for the list
    /// to highlight <paramref name="item"/>, and presses Enter to take it.
    /// </summary>
    public async Task TypeAndPressEnter(string text, string item, int timeoutMs = 15000)
    {
        await Input.ClickWhenStableAsync(_page);
        await Input.PressSequentiallyAsync(text, new() { Delay = 50 });
        await HighlightedItem.Filter(new() { HasText = item })
            .WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        await Input.PressAsync("Enter");
    }
}
