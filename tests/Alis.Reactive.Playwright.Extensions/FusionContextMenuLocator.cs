using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

public sealed class FusionContextMenuLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionContextMenuLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Root => _page.Locator($"#{_componentId}");

    public ILocator Target => _page.Locator("#resident-context-target");

    public ILocator OpenButton => _page.Locator("#open-context-menu-btn");

    public ILocator CloseButton => _page.Locator("#close-context-menu-btn");

    public ILocator Item(string text) => _page.GetByRole(AriaRole.Menuitem, new() { Name = text });
}
