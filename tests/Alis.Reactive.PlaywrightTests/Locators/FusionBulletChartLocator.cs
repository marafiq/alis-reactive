using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Locators;

/// <summary>A Syncfusion BulletChart by its component ID: its bars, their target lines and its tooltip.</summary>
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

    /// <summary>The bar of the row at this zero-based place in the chart.</summary>
    public ILocator Bar(int place) => _page.Locator($"#{_componentId}_svg_FeatureMeasure_{place}");

    /// <summary>The target line of the row at this zero-based place in the chart.</summary>
    public ILocator TargetLine(int place) => _page.Locator($"#{_componentId}_svg_ComparativeMeasure_{place}");

    /// <summary>The tooltip the chart shows for the bar or line under the pointer.</summary>
    public ILocator Tooltip => _page.Locator($"#tooltipDiv{_componentId}");
}
