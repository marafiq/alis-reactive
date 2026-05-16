using Alis.Reactive.DesignSystem.Tokens;
using Alis.Reactive.NativeTagHelpers.Card;
using Alis.Reactive.NativeTagHelpers.Tests.Infrastructure;
using Microsoft.AspNetCore.Razor.TagHelpers;
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

        tagHelper.Process(context, output);

        var classAttr = output.Attributes["class"]?.Value?.ToString();
        Assert.That(classAttr, Does.Contain("border-error"));
    }

    [Test]
    public void Card_renders_with_start_and_end_tag()
    {
        var tagHelper = new NativeCardTagHelper();
        var context = CreateContext("native-card");
        var output = CreateOutput("native-card");
        output.TagMode = TagMode.SelfClosing;

        tagHelper.Process(context, output);

        Assert.That(output.TagMode, Is.EqualTo(TagMode.StartTagAndEndTag));
    }

    [Test]
    public void Card_header_renders_div_with_padding()
    {
        var tagHelper = new NativeCardHeaderTagHelper();
        var context = CreateContext("native-card-header");
        var output = CreateOutput("native-card-header");

        tagHelper.Process(context, output);

        Assert.That(output.TagName, Is.EqualTo("div"));
        Assert.That(output.Attributes["class"]?.Value?.ToString(), Does.Contain("px-6"));
    }

    [Test]
    public void Card_header_with_divider_adds_bottom_border()
    {
        var tagHelper = new NativeCardHeaderTagHelper { Divider = CardDivider.Header };
        var context = CreateContext("native-card-header");
        var output = CreateOutput("native-card-header");

        tagHelper.Process(context, output);

        Assert.That(output.Attributes["class"]?.Value?.ToString(), Does.Contain("border-b"));
    }

    [Test]
    public void Card_body_renders_div_with_padding_classes()
    {
        var tagHelper = new NativeCardBodyTagHelper();
        var context = CreateContext("native-card-body");
        var output = CreateOutput("native-card-body");

        tagHelper.Process(context, output);

        Assert.That(output.TagName, Is.EqualTo("div"));
        Assert.That(output.Attributes["class"]?.Value?.ToString(), Does.Contain("p-6"));
    }

    [Test]
    public void Card_body_merges_custom_css_class()
    {
        var tagHelper = new NativeCardBodyTagHelper { CssClass = "space-y-4" };
        var context = CreateContext("native-card-body");
        var output = CreateOutput("native-card-body");

        tagHelper.Process(context, output);

        Assert.That(output.Attributes["class"]?.Value?.ToString(), Does.Contain("space-y-4"));
    }

    [Test]
    public void Card_footer_with_divider_adds_top_border()
    {
        var tagHelper = new NativeCardFooterTagHelper { Divider = CardDivider.Footer };
        var context = CreateContext("native-card-footer");
        var output = CreateOutput("native-card-footer");

        tagHelper.Process(context, output);

        Assert.That(output.TagName, Is.EqualTo("div"));
        Assert.That(output.Attributes["class"]?.Value?.ToString(), Does.Contain("border-t"));
    }
}
