using Alis.Reactive.DesignSystem.Tokens;
using Alis.Reactive.NativeTagHelpers.VStack;
using Alis.Reactive.NativeTagHelpers.Tests.Infrastructure;
using NUnit.Framework;

namespace Alis.Reactive.NativeTagHelpers.Tests;

[TestFixture]
public class WhenRenderingVStack : TagHelperTestBase
{
    [Test]
    public void VStack_renders_flex_col_with_gap()
    {
        var tagHelper = new NativeVStackTagHelper { Gap = SpacingScale.Lg };
        var context = CreateContext("native-vstack");
        var output = CreateOutput("native-vstack");

        tagHelper.Process(context, output);

        Assert.That(output.TagName, Is.EqualTo("div"));
        var classAttr = output.Attributes["class"]?.Value?.ToString();
        Assert.That(classAttr, Does.Contain("flex-col"));
        Assert.That(classAttr, Does.Contain("gap-8"));
    }

    [Test]
    public void VStack_with_divideY_adds_divide_classes()
    {
        var tagHelper = new NativeVStackTagHelper { Gap = SpacingScale.Base, DivideY = true };
        var context = CreateContext("native-vstack");
        var output = CreateOutput("native-vstack");

        tagHelper.Process(context, output);

        var classAttr = output.Attributes["class"]?.Value?.ToString();
        Assert.That(classAttr, Does.Contain("divide-y"));
    }
}
