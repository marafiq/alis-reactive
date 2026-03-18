using Alis.Reactive.DesignSystem.Tokens;
using Alis.Reactive.NativeTagHelpers.Grid;
using Alis.Reactive.NativeTagHelpers.Tests.Infrastructure;
using NUnit.Framework;

namespace Alis.Reactive.NativeTagHelpers.Tests;

[TestFixture]
public class WhenRenderingGrid : TagHelperTestBase
{
    [Test]
    public void Grid_renders_responsive_classes_by_default()
    {
        var tagHelper = new NativeGridTagHelper { Cols = GridCols.C3 };
        var context = CreateContext("native-grid");
        var output = CreateOutput("native-grid");

        tagHelper.Process(context, output);

        Assert.That(output.TagName, Is.EqualTo("div"));
        var classAttr = output.Attributes["class"]?.Value?.ToString();
        Assert.That(classAttr, Does.Contain("grid-cols-1"));
        Assert.That(classAttr, Does.Contain("sm:grid-cols-2"));
        Assert.That(classAttr, Does.Contain("lg:grid-cols-3"));
    }

    [Test]
    public void Grid_non_responsive_uses_direct_columns()
    {
        var tagHelper = new NativeGridTagHelper { Cols = GridCols.C3, Responsive = false };
        var context = CreateContext("native-grid");
        var output = CreateOutput("native-grid");

        tagHelper.Process(context, output);

        var classAttr = output.Attributes["class"]?.Value?.ToString();
        Assert.That(classAttr, Does.Contain("grid-cols-3"));
        Assert.That(classAttr, Does.Not.Contain("sm:"));
    }
}
