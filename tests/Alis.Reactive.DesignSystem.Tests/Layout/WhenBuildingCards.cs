using Alis.Reactive.DesignSystem.Layout;
using Alis.Reactive.DesignSystem.Tokens;
using NUnit.Framework;

namespace Alis.Reactive.DesignSystem.Tests.Layout;

[TestFixture]
public class WhenBuildingCards
{
    [Test]
    public void Card_classes_include_base_styling()
    {
        var classes = CardCss.CardClasses();
        Assert.That(classes, Does.Contain("bg-surface-elevated"));
        Assert.That(classes, Does.Contain("rounded-2xl"));
        Assert.That(classes, Does.Contain("border"));
    }

    [Test]
    public void Card_flat_elevation_has_no_shadow()
    {
        var classes = CardCss.CardClasses(CardElevation.Flat);
        Assert.That(classes, Does.Not.Contain("shadow"));
    }

    [Test]
    public void Card_high_elevation_uses_shadow_lg()
    {
        var classes = CardCss.CardClasses(CardElevation.High);
        Assert.That(classes, Does.Contain("shadow-lg"));
    }

    [Test]
    public void Header_with_divider_includes_border()
    {
        var classes = CardCss.HeaderClasses(CardDivider.Header);
        Assert.That(classes, Does.Contain("border-b"));
    }

    [Test]
    public void Header_without_divider_has_no_border()
    {
        var classes = CardCss.HeaderClasses(CardDivider.None);
        Assert.That(classes, Does.Not.Contain("border-b"));
    }

    [Test]
    public void Body_compact_uses_smaller_padding()
    {
        var classes = CardCss.BodyClasses(CardPadding.Compact);
        Assert.That(classes, Does.Contain("px-5"));
        Assert.That(classes, Does.Contain("py-4"));
    }

    [Test]
    public void Body_standard_uses_responsive_padding()
    {
        var classes = CardCss.BodyClasses(CardPadding.Standard);
        Assert.That(classes, Does.Contain("p-6"));
        Assert.That(classes, Does.Contain("sm:p-8"));
    }
}
