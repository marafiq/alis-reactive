using Alis.Reactive.DesignSystem.Tokens;
using Alis.Reactive.NativeTagHelpers.Heading;
using Alis.Reactive.NativeTagHelpers.Tests.Infrastructure;
using Microsoft.AspNetCore.Razor.TagHelpers;
using NUnit.Framework;

namespace Alis.Reactive.NativeTagHelpers.Tests;

[TestFixture]
public class WhenRenderingHeading : TagHelperTestBase
{
    [Test]
    public void Heading_renders_correct_tag_for_level()
    {
        var tagHelper = new NativeHeadingTagHelper { Level = HeadingLevel.H3 };
        var context = CreateContext("native-heading");
        var output = CreateOutput("native-heading");

        tagHelper.Process(context, output);

        Assert.That(output.TagName, Is.EqualTo("h3"));
    }

    [Test]
    public void Heading_includes_overline_in_pre_element()
    {
        var tagHelper = new NativeHeadingTagHelper { Level = HeadingLevel.H2, Overline = "Section" };
        var context = CreateContext("native-heading");
        var output = CreateOutput("native-heading");

        tagHelper.Process(context, output);

        Assert.That(output.PreElement.GetContent(), Does.Contain("Section"));
    }

    [Test]
    public void Default_spacing_produces_mb_2()
    {
        var tagHelper = new NativeHeadingTagHelper { Level = HeadingLevel.H2 };
        var context = CreateContext("native-heading");
        var output = CreateOutput("native-heading");

        tagHelper.Process(context, output);

        var classAttr = output.Attributes["class"]?.Value?.ToString();
        Assert.That(classAttr, Does.Contain("mb-2"));
    }

    [Test]
    public void Spacing_none_omits_margin_class()
    {
        var tagHelper = new NativeHeadingTagHelper { Level = HeadingLevel.H2, Spacing = ElementSpacing.None };
        var context = CreateContext("native-heading");
        var output = CreateOutput("native-heading");

        tagHelper.Process(context, output);

        var classAttr = output.Attributes["class"]?.Value?.ToString();
        Assert.That(classAttr, Does.Not.Contain("mb-"));
    }

    [Test]
    public void Spacing_base_produces_mb_3()
    {
        var tagHelper = new NativeHeadingTagHelper { Level = HeadingLevel.H2, Spacing = ElementSpacing.Base };
        var context = CreateContext("native-heading");
        var output = CreateOutput("native-heading");

        tagHelper.Process(context, output);

        var classAttr = output.Attributes["class"]?.Value?.ToString();
        Assert.That(classAttr, Does.Contain("mb-3"));
    }

    [Test]
    public void Heading_html_encodes_the_overline()
    {
        var tagHelper = new NativeHeadingTagHelper { Overline = "<script>alert('x')</script>" };
        var context = CreateContext("native-heading");
        var output = CreateOutput("native-heading");

        tagHelper.Process(context, output);

        var preElement = output.PreElement.GetContent();
        Assert.That(preElement, Does.Contain("&lt;script&gt;"));
        Assert.That(preElement, Does.Not.Contain("<script>"));
    }

    [Test]
    public void Heading_renders_with_start_and_end_tag()
    {
        var tagHelper = new NativeHeadingTagHelper { Level = HeadingLevel.H2 };
        var context = CreateContext("native-heading");
        var output = CreateOutput("native-heading");
        output.TagMode = TagMode.SelfClosing;

        tagHelper.Process(context, output);

        Assert.That(output.TagMode, Is.EqualTo(TagMode.StartTagAndEndTag));
    }
}
