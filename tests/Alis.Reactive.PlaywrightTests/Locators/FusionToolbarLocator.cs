using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class FusionToolbarLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionToolbarLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Root => _page.Locator($"#{_componentId}");

    public ILocator DisabledRoot => _page.Locator($"#{_componentId}.e-overlay");

    public ILocator Command(string itemId) => _page.Locator($"#{itemId}");

    public async Task ClickCommand(string itemId) => await Command(itemId).ClickAsync();
}
