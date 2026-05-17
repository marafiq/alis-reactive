using Alis.Reactive.NativeTagHelpers.Kv;
using Alis.Reactive.NativeTagHelpers.Tests.Infrastructure;
using Microsoft.AspNetCore.Razor.TagHelpers;
using NUnit.Framework;

namespace Alis.Reactive.NativeTagHelpers.Tests;

[TestFixture]
public class WhenRenderingKv : TagHelperTestBase
{
    [Test]
    public void Stacked_kv_renders_dl_with_dt_dd_and_correct_classes()
    {
        var tagHelper = new NativeKvTagHelper { Label = "Name", Value = "John" };
        var context = CreateContext("native-kv");
        var output = CreateOutput("native-kv");

        tagHelper.Process(context, output);

        Assert.That(output.TagName, Is.EqualTo("dl"));
        var html = output.Content.GetContent();
        Assert.That(html, Does.Contain("Name"));
        Assert.That(html, Does.Contain("John"));
        Assert.That(html, Does.Contain("text-text-muted"));
        Assert.That(html, Does.Contain("text-text-primary"));
    }

    [Test]
    public void Inline_kv_renders_dl_wrapper_with_colon()
    {
        var tagHelper = new NativeKvTagHelper { Label = "Status", Value = "Active", Layout = KvLayout.Inline };
        var context = CreateContext("native-kv");
        var output = CreateOutput("native-kv");

        tagHelper.Process(context, output);

        Assert.That(output.TagName, Is.EqualTo("dl"));
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

        Assert.That(output.Attributes["class"]?.Value?.ToString(), Is.EqualTo("mb-4"));
    }

    [Test]
    public void Stacked_kv_omits_class_attribute_when_no_custom_class()
    {
        var tagHelper = new NativeKvTagHelper { Label = "X", Value = "Y" };
        var context = CreateContext("native-kv");
        var output = CreateOutput("native-kv");

        tagHelper.Process(context, output);

        Assert.That(output.Attributes.ContainsName("class"), Is.False);
    }

    [Test]
    public void Inline_kv_html_encodes_label_and_value()
    {
        var tagHelper = new NativeKvTagHelper
        {
            Label = "<b>Key</b>",
            Value = "<script>alert('x')</script>",
            Layout = KvLayout.Inline
        };
        var context = CreateContext("native-kv");
        var output = CreateOutput("native-kv");

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        Assert.That(html, Does.Contain("&lt;b&gt;Key&lt;/b&gt;"));
        Assert.That(html, Does.Contain("&lt;script&gt;"));
        Assert.That(html, Does.Not.Contain("<script>"));
    }

    [Test]
    public void Stacked_kv_html_encodes_label_and_value()
    {
        var tagHelper = new NativeKvTagHelper
        {
            Label = "<b>Key</b>",
            Value = "<script>alert('x')</script>"
        };
        var context = CreateContext("native-kv");
        var output = CreateOutput("native-kv");

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        Assert.That(html, Does.Contain("&lt;b&gt;Key&lt;/b&gt;"));
        Assert.That(html, Does.Contain("&lt;script&gt;"));
        Assert.That(html, Does.Not.Contain("<script>"));
    }

    [Test]
    public void Throws_when_label_is_missing()
    {
        var tagHelper = new NativeKvTagHelper { Value = "John" };
        var context = CreateContext("native-kv");
        var output = CreateOutput("native-kv");

        Assert.That(() => tagHelper.Process(context, output),
            Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void Throws_when_value_is_missing()
    {
        var tagHelper = new NativeKvTagHelper { Label = "Name" };
        var context = CreateContext("native-kv");
        var output = CreateOutput("native-kv");

        Assert.That(() => tagHelper.Process(context, output),
            Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void Kv_renders_with_start_and_end_tag()
    {
        var tagHelper = new NativeKvTagHelper { Label = "X", Value = "Y" };
        var context = CreateContext("native-kv");
        var output = CreateOutput("native-kv");
        output.TagMode = TagMode.SelfClosing;

        tagHelper.Process(context, output);

        Assert.That(output.TagMode, Is.EqualTo(TagMode.StartTagAndEndTag));
    }
}
