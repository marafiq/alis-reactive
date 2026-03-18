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
}
