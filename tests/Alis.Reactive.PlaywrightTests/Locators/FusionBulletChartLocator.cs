using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

public sealed class FusionBulletChartLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionBulletChartLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    public ILocator Root => _page.Locator($"#{_componentId}");

    public ILocator Bar(int place) => _page.Locator($"#{_componentId}_svg_FeatureMeasure_{place}");

    public ILocator TargetLine(int place) => _page.Locator($"#{_componentId}_svg_ComparativeMeasure_{place}");

    public ILocator Tooltip => _page.Locator($"#tooltipDiv{_componentId}");
}
