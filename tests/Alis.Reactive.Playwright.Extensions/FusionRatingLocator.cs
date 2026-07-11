using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

public sealed class FusionRatingLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionRatingLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The rendered rating list, the slider element that carries the current value.</summary>
    public ILocator RatingList => _page.Locator($"#{_componentId}_item-list");

    /// <summary>The clickable star at the given one-based position.</summary>
    public ILocator Star(int position) =>
        RatingList.Locator(".e-rating-item-container").Nth(position - 1);

    /// <summary>Clicks the star at the given one-based position the way a resident would.</summary>
    public async Task RateStars(int position) =>
        await Star(position).ClickAsync();
}
