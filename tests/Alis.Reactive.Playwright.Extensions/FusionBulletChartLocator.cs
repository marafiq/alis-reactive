using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

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

    public ILocator FeatureMeasure => _page.Locator($"#{_componentId}_svg_FeatureMeasure_0");

    public ILocator ComparativeMeasure => _page.Locator($"#{_componentId}_svg_ComparativeMeasure_0");

    public ILocator Tooltip => _page.Locator($"#tooltipDiv{_componentId}");
}
