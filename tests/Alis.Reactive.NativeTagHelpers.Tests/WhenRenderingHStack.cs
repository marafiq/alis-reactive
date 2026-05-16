using Alis.Reactive.NativeTagHelpers.HStack;
using Alis.Reactive.NativeTagHelpers.Tests.Infrastructure;
using Microsoft.AspNetCore.Razor.TagHelpers;
using NUnit.Framework;

namespace Alis.Reactive.NativeTagHelpers.Tests;

[TestFixture]
public class WhenRenderingHStack : TagHelperTestBase
{
    [Test]
    public void HStack_renders_div_with_flex_row()
    {
        var tagHelper = new NativeHStackTagHelper();
        var context = CreateContext("native-hstack");
        var output = CreateOutput("native-hstack");

        tagHelper.Process(context, output);

        Assert.That(output.TagName, Is.EqualTo("div"));
        Assert.That(output.Attributes["class"]?.Value?.ToString(), Does.Contain("flex"));
    }

    [Test]
    public void HStack_merges_custom_css_class()
    {
        var tagHelper = new NativeHStackTagHelper { CssClass = "mt-4" };
        var context = CreateContext("native-hstack");
        var output = CreateOutput("native-hstack");

        tagHelper.Process(context, output);

        Assert.That(output.Attributes["class"]?.Value?.ToString(), Does.Contain("mt-4"));
    }

    [Test]
    public void HStack_renders_with_start_and_end_tag()
    {
        var tagHelper = new NativeHStackTagHelper();
        var context = CreateContext("native-hstack");
        var output = CreateOutput("native-hstack");
        output.TagMode = TagMode.SelfClosing;

        tagHelper.Process(context, output);

        Assert.That(output.TagMode, Is.EqualTo(TagMode.StartTagAndEndTag));
    }
}
