using Alis.Reactive.NativeTagHelpers.Container;
using Alis.Reactive.NativeTagHelpers.Tests.Infrastructure;
using NUnit.Framework;

namespace Alis.Reactive.NativeTagHelpers.Tests;

[TestFixture]
public class WhenRenderingContainer : TagHelperTestBase
{
    [Test]
    public void Container_renders_div_with_max_width_classes()
    {
        var tagHelper = new NativeContainerTagHelper();
        var context = CreateContext("native-container");
        var output = CreateOutput("native-container");

        tagHelper.Process(context, output);

        Assert.That(output.TagName, Is.EqualTo("div"));
        var classAttr = output.Attributes["class"]?.Value?.ToString();
        Assert.That(classAttr, Does.Contain("max-w-7xl"));
        Assert.That(classAttr, Does.Contain("mx-auto"));
    }

    [Test]
    public void Container_merges_custom_css_class()
    {
        var tagHelper = new NativeContainerTagHelper { CssClass = "py-8" };
        var context = CreateContext("native-container");
        var output = CreateOutput("native-container");

        tagHelper.Process(context, output);

        var classAttr = output.Attributes["class"]?.Value?.ToString();
        Assert.That(classAttr, Does.Contain("py-8"));
    }
}
