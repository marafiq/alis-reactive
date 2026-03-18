using Alis.Reactive.DesignSystem.Layout;
using Alis.Reactive.DesignSystem.Tokens;
using Alis.Reactive.NativeTagHelpers.Card;
using Alis.Reactive.NativeTagHelpers.Tests.Infrastructure;
using NUnit.Framework;

namespace Alis.Reactive.NativeTagHelpers.Tests;

[TestFixture]
public class WhenRenderingCard : TagHelperTestBase
{
    [Test]
    public void Card_renders_div_with_card_classes()
    {
        var tagHelper = new NativeCardTagHelper();
        var context = CreateContext("native-card");
        var output = CreateOutput("native-card");

        tagHelper.Init(context);
        tagHelper.Process(context, output);

        Assert.That(output.TagName, Is.EqualTo("div"));
        var classAttr = output.Attributes["class"]?.Value?.ToString();
        Assert.That(classAttr, Does.Contain("bg-surface-elevated"));
        Assert.That(classAttr, Does.Contain("rounded-2xl"));
    }

    [Test]
    public void Card_with_accent_adds_border_class()
    {
        var tagHelper = new NativeCardTagHelper { Accent = AccentColor.Error };
        var context = CreateContext("native-card");
        var output = CreateOutput("native-card");

        tagHelper.Init(context);
        tagHelper.Process(context, output);

        var classAttr = output.Attributes["class"]?.Value?.ToString();
        Assert.That(classAttr, Does.Contain("border-error"));
    }
}
