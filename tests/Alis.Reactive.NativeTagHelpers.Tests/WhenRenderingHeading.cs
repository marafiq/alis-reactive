using Alis.Reactive.DesignSystem.Tokens;
using Alis.Reactive.NativeTagHelpers.Heading;
using Alis.Reactive.NativeTagHelpers.Tests.Infrastructure;
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
}
