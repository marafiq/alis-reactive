using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

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

    public ILocator Secondary => _page.Locator($"#{_componentId}_dropdownbtn");

    public ILocator Popup => _page.Locator($"#{_componentId}_dropdownbtn-popup");

    public ILocator Item(string text) =>
        _page.Locator($"//*[@id='{_componentId}_dropdownbtn-popup']//li[contains(concat(' ', normalize-space(@class), ' '), ' e-item ') and normalize-space(.)='{text}']");

    public async Task Choose(string text)
    {
        await Secondary.ClickAsync();
        await Item(text).ClickAsync();
    }
}
