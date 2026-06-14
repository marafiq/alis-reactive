using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Locators for a rendered Syncfusion Carousel: the component root and the
/// currently active slide. The runtime stamps <c>e-active</c> on exactly one
/// <c>e-carousel-item</c>, so the active slide is the one the resident sees.
/// </summary>
public sealed class FusionCarouselLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionCarouselLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The carousel component root.</summary>
    public ILocator Root => _page.Locator($"#{_componentId}");

    /// <summary>The slide currently shown to the resident.</summary>
    public ILocator ActiveSlide => Root.Locator(".e-carousel-item.e-active");

    /// <summary>The title (section name) of the slide currently shown.</summary>
    public ILocator ActiveSectionTitle => ActiveSlide.Locator(".care-section-title");
}
