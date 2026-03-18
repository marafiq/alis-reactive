using Alis.Reactive.DesignSystem.Layout;
using Alis.Reactive.DesignSystem.Tokens;
using NUnit.Framework;

namespace Alis.Reactive.DesignSystem.Tests.Layout;

[TestFixture]
public class WhenBuildingGrids
{
    [Test]
    public void Grid_classes_include_columns_and_gap()
    {
        var classes = GridCss.Classes(GridCols.C3);
        Assert.That(classes, Does.Contain("grid"));
        Assert.That(classes, Does.Contain("grid-cols-3"));
        Assert.That(classes, Does.Contain("gap-6"));
    }

    [Test]
    public void Responsive_grid_adds_breakpoint_prefixes()
    {
        var classes = GridCss.ResponsiveClasses(GridCols.C3);
        Assert.That(classes, Does.Contain("grid-cols-1"));
        Assert.That(classes, Does.Contain("sm:grid-cols-2"));
        Assert.That(classes, Does.Contain("lg:grid-cols-3"));
    }

    [Test]
    public void Two_column_responsive_skips_lg_breakpoint()
    {
        var classes = GridCss.ResponsiveClasses(GridCols.C2);
        Assert.That(classes, Does.Contain("grid-cols-1"));
        Assert.That(classes, Does.Contain("sm:grid-cols-2"));
        Assert.That(classes, Does.Not.Contain("lg:"));
    }
}
