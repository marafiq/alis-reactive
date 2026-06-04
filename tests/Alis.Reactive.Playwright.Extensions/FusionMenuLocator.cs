using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

public sealed class FusionMenuLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionMenuLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Root => _page.Locator($"#{_componentId}");

    public ILocator Item(string text) => Root.GetByText(text, new() { Exact = true });

    public ILocator OpenButton => _page.Locator("#open-menu-btn");

    public ILocator CloseButton => _page.Locator("#close-menu-btn");
}
