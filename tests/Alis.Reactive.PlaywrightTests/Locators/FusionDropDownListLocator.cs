using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

/// <summary>
/// A Syncfusion DropDownList: the input <c>#{componentId}</c>, the input group around it that takes focus
/// and holds its clear and dropdown buttons, and the list it opens at <c>#{componentId}_popup</c>.
/// </summary>
public sealed class FusionDropDownListLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionDropDownListLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The input; it shows the selected item's text.</summary>
    public ILocator Input => _page.Locator($"#{_componentId}");

    /// <summary>The input group, the element that takes focus when the list is entered.</summary>
    public ILocator Wrapper => Input.Locator("xpath=..");

    public ILocator DropDownButton => Wrapper.Locator(".e-ddl-icon");

    /// <summary>The cross that clears the selection; the list shows it while it is focused and holds a value.</summary>
    public ILocator ClearButton => Wrapper.Locator(".e-clear-icon");

    public ILocator Popup => _page.Locator($"#{_componentId}_popup");

    /// <summary>The items the open list offers, without its group headers.</summary>
    public ILocator Items => Popup.Locator("li.e-list-item");

    /// <summary>The headers of the groups the open list sorts its items into.</summary>
    public ILocator GroupHeaders => Popup.Locator("li.e-list-group-item");

    public ILocator Item(string text) => Popup.GetByText(text, new() { Exact = true });

    /// <summary>Opens the list with its dropdown button.</summary>
    public async Task Open()
    {
        await DropDownButton.ClickWhenStableAsync(_page);
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
    }

    /// <summary>Clicks <paramref name="text"/> in the open list and waits for the list to close.</summary>
    public async Task Pick(string text)
    {
        var item = Item(text);
        await item.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await item.ClickWhenStableAsync(_page);
        await Popup.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 5000 });
    }

    /// <summary>Opens the list and picks <paramref name="text"/>.</summary>
    public async Task Choose(string text)
    {
        await Open();
        await Pick(text);
    }
}
