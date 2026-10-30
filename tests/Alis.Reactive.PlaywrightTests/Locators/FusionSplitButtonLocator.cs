using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

/// <summary>A Syncfusion SplitButton: the primary button, the arrow beside it, and the popup menu the arrow opens.</summary>
public sealed class FusionSplitButtonLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionSplitButtonLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Primary => _page.Locator($"#{_componentId}");

    /// <summary>The arrow button that opens the menu.</summary>
    public ILocator Secondary => _page.Locator($"#{_componentId}_dropdownbtn");

    /// <summary>The popup that holds the menu while it is open.</summary>
    public ILocator Popup => _page.Locator($"#{_componentId}_dropdownbtn-popup");

    /// <summary>The menu item whose text is exactly <paramref name="text"/>.</summary>
    public ILocator Item(string text) =>
        _page.Locator($"//*[@id='{_componentId}_dropdownbtn-popup']//li[contains(concat(' ', normalize-space(@class), ' '), ' e-item ') and normalize-space(.)='{text}']");

    /// <summary>Opens the menu with the arrow and chooses the item with this text.</summary>
    public async Task Choose(string text)
    {
        await Secondary.ClickAsync();
        await Item(text).ClickAsync();
    }
}
