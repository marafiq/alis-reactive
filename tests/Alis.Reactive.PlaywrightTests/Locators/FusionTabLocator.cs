using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class FusionTabLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionTabLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Root => _page.Locator($"#{_componentId}");

    public ILocator Header(int index) =>
        Root.Locator(".e-tab-header .e-toolbar-item").Nth(index);

    public ILocator ActiveHeader =>
        Root.Locator(".e-tab-header .e-toolbar-item.e-active");

    public ILocator ActiveContent =>
        Root.Locator(".e-content > .e-item.e-active");

    public ILocator VisibleHeaders =>
        Root.Locator(".e-tab-header .e-toolbar-item:not(.e-hidden)");

    public ILocator HeaderByText(string label) =>
        Root.Locator(".e-tab-header .e-toolbar-item").Filter(new() { HasTextString = label });

    public async Task OpenSection(int index) => await Header(index).ClickAsync();
}
