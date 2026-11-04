using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class FusionAccordionLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionAccordionLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Root => _page.Locator($"#{_componentId}");

    public ILocator Item(int index) => Root.Locator(".e-acrdn-item").Nth(index);

    public ILocator Header(int index) => Item(index).Locator(".e-acrdn-header");

    public ILocator Content(int index) => Item(index).Locator(".e-acrdn-panel .e-acrdn-content");

    public async Task OpenSection(int index) => await Header(index).ClickAsync();
}
