using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

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

    public ILocator Wrapper => Root.Locator("xpath=..");

    public ILocator HamburgerIcon => Wrapper.Locator(".e-menu-header .e-menu-icon");

    public ILocator Item(string text) => Wrapper.GetByLabel(text, new() { Exact = true });

    public Task Tap(string text) => Item(text).ClickAsync(new() { Position = new() { X = 12, Y = 12 } });
}
