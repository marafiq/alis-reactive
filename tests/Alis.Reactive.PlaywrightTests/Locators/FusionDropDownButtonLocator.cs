using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

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

    public ILocator Popup => _page.Locator($"#{_componentId}-popup");

    public ILocator Item(string text) =>
        _page.Locator($"//*[@id='{_componentId}-popup']//li[contains(concat(' ', normalize-space(@class), ' '), ' e-item ') and normalize-space(.)='{text}']");

    public async Task Choose(string text)
    {
        await Button.ClickAsync();
        await Item(text).ClickAsync();
    }
}
