using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for FusionSlider.
/// </summary>
public sealed class FusionSliderLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionSliderLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The Syncfusion Slider host element.</summary>
    public ILocator Host => _page.Locator($"#{_componentId}");

    /// <summary>Gets a slider handle by zero-based index.</summary>
    public ILocator Handle(int index) => Host.Locator(".e-handle").Nth(index);

    /// <summary>Gets the current ARIA value for a slider handle.</summary>
    public async Task<string?> ValueNow(int index = 0) =>
        await Handle(index).GetAttributeAsync("aria-valuenow");
}
