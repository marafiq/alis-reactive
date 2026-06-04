using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

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

    public ILocator SaveItem => _page.Locator("#toolbar-save");

    public ILocator DeleteItem => _page.Locator("#toolbar-delete");

    public ILocator DisabledRoot => _page.Locator($"#{_componentId}.e-disabled");
}
