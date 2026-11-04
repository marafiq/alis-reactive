using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class FusionProgressButtonLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionProgressButtonLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Button => _page.Locator($"#{_componentId}");

    public ILocator Content => Button.Locator(".e-btn-content");

    public ILocator ProgressBar => Button.Locator(".e-progress");
}
