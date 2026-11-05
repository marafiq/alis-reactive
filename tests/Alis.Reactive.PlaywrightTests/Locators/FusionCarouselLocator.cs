using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class FusionCarouselLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionCarouselLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Root => _page.Locator($"#{_componentId}");

    public ILocator ActiveSlide => Root.Locator(".e-carousel-item.e-active");

    public ILocator ActiveSectionTitle => ActiveSlide.Locator(".care-section-title");
}
