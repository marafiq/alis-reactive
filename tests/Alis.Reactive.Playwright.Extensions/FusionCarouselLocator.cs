using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for FusionCarousel.
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

    /// <summary>The rendered carousel root element.</summary>
    public ILocator Root => _page.Locator($"#{_componentId}");

    /// <summary>The currently active slide.</summary>
    public ILocator ActiveSlide => Root.Locator(".e-carousel-item.e-active");

    /// <summary>The current active slide text.</summary>
    public async Task<string> ActiveText() => await ActiveSlide.TextContentAsync() ?? string.Empty;
}
