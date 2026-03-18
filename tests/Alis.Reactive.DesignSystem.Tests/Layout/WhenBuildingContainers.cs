using Alis.Reactive.DesignSystem.Layout;
using NUnit.Framework;

namespace Alis.Reactive.DesignSystem.Tests.Layout;

[TestFixture]
public class WhenBuildingContainers
{
    [Test]
    public void Container_includes_max_width_and_centering()
    {
        var classes = ContainerCss.Classes();
        Assert.That(classes, Does.Contain("max-w-7xl"));
        Assert.That(classes, Does.Contain("mx-auto"));
    }

    [Test]
    public void Container_includes_responsive_padding()
    {
        var classes = ContainerCss.Classes();
        Assert.That(classes, Does.Contain("px-4"));
        Assert.That(classes, Does.Contain("sm:px-6"));
        Assert.That(classes, Does.Contain("lg:px-8"));
    }

    [Test]
    public void Container_merges_user_class()
    {
        var classes = ContainerCss.Classes("py-8");
        Assert.That(classes, Does.Contain("max-w-7xl"));
        Assert.That(classes, Does.Contain("py-8"));
    }
}
