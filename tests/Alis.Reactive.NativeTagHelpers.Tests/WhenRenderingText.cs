using Alis.Reactive.NativeTagHelpers.Text;
using Alis.Reactive.NativeTagHelpers.Tests.Infrastructure;
using Microsoft.AspNetCore.Razor.TagHelpers;
using NUnit.Framework;

namespace Alis.Reactive.NativeTagHelpers.Tests;

[TestFixture]
public class WhenRenderingText : TagHelperTestBase
{
    [Test]
    public void Text_renders_paragraph_by_default()
    {
        var tagHelper = new NativeTextTagHelper();
        var context = CreateContext("native-text");
        var output = CreateOutput("native-text");

        tagHelper.Process(context, output);

        Assert.That(output.TagName, Is.EqualTo("p"));
    }

    [Test]
    public void Text_as_span_renders_inline_span()
    {
        var tagHelper = new NativeTextTagHelper { AsSpan = true };
        var context = CreateContext("native-text");
        var output = CreateOutput("native-text");

        tagHelper.Process(context, output);

        Assert.That(output.TagName, Is.EqualTo("span"));
    }

    [Test]
    public void Text_bold_adds_semibold_class()
    {
        var tagHelper = new NativeTextTagHelper { Bold = true };
        var context = CreateContext("native-text");
        var output = CreateOutput("native-text");

        tagHelper.Process(context, output);

        Assert.That(output.Attributes["class"]?.Value?.ToString(), Does.Contain("font-semibold"));
    }

    [Test]
    public void Text_merges_custom_css_class()
    {
        var tagHelper = new NativeTextTagHelper { CssClass = "italic" };
        var context = CreateContext("native-text");
        var output = CreateOutput("native-text");

        tagHelper.Process(context, output);

        Assert.That(output.Attributes["class"]?.Value?.ToString(), Does.Contain("italic"));
    }

    [Test]
    public void Text_renders_with_start_and_end_tag()
    {
        var tagHelper = new NativeTextTagHelper();
        var context = CreateContext("native-text");
        var output = CreateOutput("native-text");
        output.TagMode = TagMode.SelfClosing;

        tagHelper.Process(context, output);

        Assert.That(output.TagMode, Is.EqualTo(TagMode.StartTagAndEndTag));
    }
}
