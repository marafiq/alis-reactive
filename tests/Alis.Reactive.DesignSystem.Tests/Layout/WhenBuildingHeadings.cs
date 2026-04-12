using Alis.Reactive.DesignSystem.Layout;
using Alis.Reactive.DesignSystem.Tokens;
using NUnit.Framework;

namespace Alis.Reactive.DesignSystem.Tests.Layout;

[TestFixture]
public class WhenBuildingHeadings
{
    [Test]
    public void H1_uses_largest_size()
    {
        var classes = HeadingCss.Classes(HeadingLevel.H1);
        Assert.That(classes, Does.Contain("text-3xl"));
        Assert.That(classes, Does.Contain("font-extrabold"));
    }

    [Test]
    public void H6_uses_small_uppercase()
    {
        var classes = HeadingCss.Classes(HeadingLevel.H6);
        Assert.That(classes, Does.Contain("text-sm"));
        Assert.That(classes, Does.Contain("uppercase"));
    }

    [Test]
    public void All_headings_include_font_display()
    {
        foreach (var level in new[] { HeadingLevel.H1, HeadingLevel.H2, HeadingLevel.H3, HeadingLevel.H4, HeadingLevel.H5, HeadingLevel.H6 })
        {
            Assert.That(HeadingCss.Classes(level), Does.Contain("font-display"));
        }
    }

    [Test]
    public void Default_spacing_is_sm_which_produces_mb_2()
    {
        var classes = HeadingCss.Classes(HeadingLevel.H2);
        Assert.That(classes, Does.Contain("mb-2"));
    }

    [Test]
    public void Spacing_none_omits_margin()
    {
        var classes = HeadingCss.Classes(HeadingLevel.H2, ElementSpacing.None);
        Assert.That(classes, Does.Not.Contain("mb-"));
    }

    [Test]
    public void Spacing_md_produces_mb_4()
    {
        var classes = HeadingCss.Classes(HeadingLevel.H2, ElementSpacing.Md);
        Assert.That(classes, Does.Contain("mb-4"));
    }

    [Test]
    public void Spacing_base_produces_mb_3()
    {
        var classes = HeadingCss.Classes(HeadingLevel.H2, ElementSpacing.Base);
        Assert.That(classes, Does.Contain("mb-3"));
    }

    [Test]
    public void Spacing_lg_produces_mb_6()
    {
        var classes = HeadingCss.Classes(HeadingLevel.H2, ElementSpacing.Lg);
        Assert.That(classes, Does.Contain("mb-6"));
    }

    [Test]
    public void Overline_has_muted_uppercase()
    {
        var classes = HeadingCss.OverlineClasses();
        Assert.That(classes, Does.Contain("text-xs"));
        Assert.That(classes, Does.Contain("uppercase"));
        Assert.That(classes, Does.Contain("text-text-muted"));
    }
}
