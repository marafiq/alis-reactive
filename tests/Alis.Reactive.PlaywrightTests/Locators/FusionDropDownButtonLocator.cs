using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

/// <summary>A Syncfusion DropDownButton: the button and the popup menu it opens.</summary>
public sealed class FusionDropDownButtonLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionDropDownButtonLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Button => _page.Locator($"#{_componentId}");

    /// <summary>The popup that holds the menu while it is open.</summary>
    public ILocator Popup => _page.Locator($"#{_componentId}-popup");

    /// <summary>The menu item whose text is exactly <paramref name="text"/>.</summary>
    public ILocator Item(string text) =>
        _page.Locator($"//*[@id='{_componentId}-popup']//li[contains(concat(' ', normalize-space(@class), ' '), ' e-item ') and normalize-space(.)='{text}']");

    /// <summary>Opens the menu and chooses the item with this text.</summary>
    public async Task Choose(string text)
    {
        await Button.ClickAsync();
        await Item(text).ClickAsync();
    }
}
