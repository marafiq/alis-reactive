using Alis.Reactive.DesignSystem.Layout;
using NUnit.Framework;

namespace Alis.Reactive.DesignSystem.Tests.Layout;

[TestFixture]
public class WhenBuildingKvPairs
{
    [Test]
    public void Stacked_dt_has_muted_uppercase_typography()
    {
        var classes = KvCss.StackedDtClasses();
        Assert.That(classes, Does.Contain("text-xs"));
        Assert.That(classes, Does.Contain("uppercase"));
        Assert.That(classes, Does.Contain("text-text-muted"));
    }

    [Test]
    public void Stacked_dd_has_primary_text()
    {
        var classes = KvCss.StackedDdClasses();
        Assert.That(classes, Does.Contain("text-sm"));
        Assert.That(classes, Does.Contain("text-text-primary"));
    }

    [Test]
    public void Inline_wrapper_has_flex_layout()
    {
        var classes = KvCss.InlineWrapperClasses();
        Assert.That(classes, Does.Contain("flex"));
        Assert.That(classes, Does.Contain("gap-2"));
    }

    [Test]
    public void Inline_wrapper_merges_css_class()
    {
        var classes = KvCss.InlineWrapperClasses("my-2");
        Assert.That(classes, Does.Contain("my-2"));
        Assert.That(classes, Does.Contain("flex"));
    }

    [Test]
    public void Inline_dt_has_muted_text()
    {
        var classes = KvCss.InlineDtClasses();
        Assert.That(classes, Does.Contain("text-sm"));
        Assert.That(classes, Does.Contain("text-text-muted"));
    }

    [Test]
    public void Inline_dd_has_primary_text()
    {
        var classes = KvCss.InlineDdClasses();
        Assert.That(classes, Does.Contain("text-sm"));
        Assert.That(classes, Does.Contain("text-text-primary"));
    }
}
