using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

public sealed class FusionSliderLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionSliderLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Host => _page.Locator($"#{_componentId}");

    public ILocator Handle(int index) => Host.Locator(".e-handle").Nth(index);

    /// <summary>Reads the ARIA value from the selected slider handle.</summary>
    public async Task<string?> ValueNow(int index = 0) =>
        await Handle(index).GetAttributeAsync("aria-valuenow");
}
