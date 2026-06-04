using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

public sealed class FusionSidebarLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionSidebarLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Root => _page.Locator($"#{_componentId}");

    public ILocator OpenRoot => _page.Locator($"#{_componentId}.e-open");

    public ILocator ClosedRoot => _page.Locator($"#{_componentId}.e-close");

    public async Task<string> Text() => await Root.TextContentAsync() ?? string.Empty;
}
