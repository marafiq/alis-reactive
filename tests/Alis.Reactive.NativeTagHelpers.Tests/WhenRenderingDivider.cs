using Alis.Reactive.NativeTagHelpers.Divider;
using Alis.Reactive.NativeTagHelpers.Tests.Infrastructure;
using NUnit.Framework;

namespace Alis.Reactive.NativeTagHelpers.Tests;

[TestFixture]
public class WhenRenderingDivider : TagHelperTestBase
{
    [Test]
    public void Plain_divider_renders_hr_with_border_classes()
    {
        var tagHelper = new NativeDividerTagHelper();
        var context = CreateContext("native-divider");
        var output = CreateOutput("native-divider");

        tagHelper.Process(context, output);

        Assert.That(output.TagName, Is.EqualTo("hr"));
        var classAttr = output.Attributes["class"]?.Value?.ToString();
        Assert.That(classAttr, Does.Contain("border-t"));
        Assert.That(classAttr, Does.Contain("border-border"));
    }

    [Test]
    public void Dashed_divider_includes_dashed_class()
    {
        var tagHelper = new NativeDividerTagHelper { Style = DividerStyle.Dashed };
        var context = CreateContext("native-divider");
        var output = CreateOutput("native-divider");

        tagHelper.Process(context, output);

        var classAttr = output.Attributes["class"]?.Value?.ToString();
        Assert.That(classAttr, Does.Contain("border-dashed"));
    }

    [Test]
    public void Labeled_divider_renders_wrapper_div_with_label_text()
    {
        var tagHelper = new NativeDividerTagHelper { Label = "Section" };
        var context = CreateContext("native-divider");
        var output = CreateOutput("native-divider");

        tagHelper.Process(context, output);

        Assert.That(output.TagName, Is.EqualTo("div"));
        var html = output.Content.GetContent();
        Assert.That(html, Does.Contain("Section"));
        Assert.That(html, Does.Contain("bg-surface"));
        Assert.That(html, Does.Not.Contain("bg-white"));
    }

    [Test]
    public void Plain_divider_merges_custom_css_class()
    {
        var tagHelper = new NativeDividerTagHelper { CssClass = "mt-8" };
        var context = CreateContext("native-divider");
        var output = CreateOutput("native-divider");

        tagHelper.Process(context, output);

        var classAttr = output.Attributes["class"]?.Value?.ToString();
        Assert.That(classAttr, Does.Contain("mt-8"));
        Assert.That(classAttr, Does.Contain("border-t"));
    }

    [Test]
    public void Labeled_divider_merges_custom_css_class_on_wrapper()
    {
        var tagHelper = new NativeDividerTagHelper { Label = "Section", CssClass = "my-8" };
        var context = CreateContext("native-divider");
        var output = CreateOutput("native-divider");

        tagHelper.Process(context, output);

        var classAttr = output.Attributes["class"]?.Value?.ToString();
        Assert.That(classAttr, Does.Contain("my-8"));
        Assert.That(classAttr, Does.Contain("relative"));
    }

    [Test]
    public void Labeled_divider_html_encodes_the_label()
    {
        var tagHelper = new NativeDividerTagHelper { Label = "<script>alert('x')</script>" };
        var context = CreateContext("native-divider");
        var output = CreateOutput("native-divider");

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        Assert.That(html, Does.Contain("&lt;script&gt;"));
        Assert.That(html, Does.Not.Contain("<script>"));
    }
}
