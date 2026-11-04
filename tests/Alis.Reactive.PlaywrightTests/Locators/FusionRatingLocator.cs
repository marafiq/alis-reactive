using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class FusionRatingLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionRatingLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator RatingList => _page.Locator($"#{_componentId}_item-list");

    public ILocator Star(int position) =>
        RatingList.Locator(".e-rating-item-container").Nth(position - 1);

    public async Task RateStars(int position) =>
        await Star(position).ClickAsync();
}
