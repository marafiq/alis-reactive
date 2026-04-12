using Alis.Reactive.NativeTagHelpers.Kv;
using Alis.Reactive.NativeTagHelpers.Tests.Infrastructure;
using NUnit.Framework;

namespace Alis.Reactive.NativeTagHelpers.Tests;

[TestFixture]
public class WhenRenderingKv : TagHelperTestBase
{
    [Test]
    public void Stacked_kv_renders_dt_dd_with_correct_classes()
    {
        var tagHelper = new NativeKvTagHelper { Label = "Name", Value = "John" };
        var context = CreateContext("native-kv");
        var output = CreateOutput("native-kv");

        tagHelper.Process(context, output);

        Assert.That(output.TagName, Is.EqualTo("div"));
        var html = output.Content.GetContent();
        Assert.That(html, Does.Contain("Name"));
        Assert.That(html, Does.Contain("John"));
        Assert.That(html, Does.Contain("text-text-muted"));
        Assert.That(html, Does.Contain("text-text-primary"));
    }

    [Test]
    public void Inline_kv_renders_flex_wrapper_with_colon()
    {
        var tagHelper = new NativeKvTagHelper { Label = "Status", Value = "Active", Layout = KvLayout.Inline };
        var context = CreateContext("native-kv");
        var output = CreateOutput("native-kv");

        tagHelper.Process(context, output);

        var classAttr = output.Attributes["class"]?.Value?.ToString();
        Assert.That(classAttr, Does.Contain("flex"));
        Assert.That(classAttr, Does.Contain("gap-2"));
        var html = output.Content.GetContent();
        Assert.That(html, Does.Contain("Status:"));
    }

    [Test]
    public void Inline_kv_merges_custom_css_class()
    {
        var tagHelper = new NativeKvTagHelper { Label = "X", Value = "Y", Layout = KvLayout.Inline, CssClass = "my-2" };
        var context = CreateContext("native-kv");
        var output = CreateOutput("native-kv");

        tagHelper.Process(context, output);

        var classAttr = output.Attributes["class"]?.Value?.ToString();
        Assert.That(classAttr, Does.Contain("my-2"));
        Assert.That(classAttr, Does.Contain("flex"));
    }

    [Test]
    public void Stacked_kv_with_custom_class_applies_to_wrapper()
    {
        var tagHelper = new NativeKvTagHelper { Label = "X", Value = "Y", CssClass = "mb-4" };
        var context = CreateContext("native-kv");
        var output = CreateOutput("native-kv");

        tagHelper.Process(context, output);

        var classAttr = output.Attributes["class"]?.Value?.ToString();
        Assert.That(classAttr, Does.Contain("mb-4"));
    }
}
