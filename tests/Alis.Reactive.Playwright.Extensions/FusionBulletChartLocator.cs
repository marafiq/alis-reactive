using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// User interaction primitives for FusionBulletChart.
/// </summary>
public sealed class FusionBulletChartLocator
{
    private readonly IPage _page;
    private readonly string _componentId;

    public FusionBulletChartLocator(IPage page, string componentId)
    {
        _page = page;
        _componentId = componentId;
    }

    /// <summary>The rendered bullet chart root element.</summary>
    public ILocator Root => _page.Locator($"#{_componentId}");

    /// <summary>The first feature measure bar.</summary>
    public ILocator FeatureMeasure => _page.Locator($"#{_componentId}_svg_FeatureMeasure_0");

    /// <summary>The first comparative measure marker.</summary>
    public ILocator ComparativeMeasure => _page.Locator($"#{_componentId}_svg_ComparativeMeasure_0");

    /// <summary>The rendered tooltip container.</summary>
    public ILocator Tooltip => _page.Locator($"#tooltipDiv{_componentId}");
}
