using Alis.Reactive.DesignSystem.Layout;
using NUnit.Framework;

namespace Alis.Reactive.DesignSystem.Tests.Layout;

[TestFixture]
public class WhenBuildingDividers
{
    [Test]
    public void Plain_classes_contain_border_and_margin()
    {
        var classes = DividerCss.PlainClasses();
        Assert.That(classes, Does.Contain("border-t"));
        Assert.That(classes, Does.Contain("border-border"));
        Assert.That(classes, Does.Contain("my-4"));
    }

    [Test]
    public void Plain_merges_css_class()
    {
        var classes = DividerCss.PlainClasses("mt-8");
        Assert.That(classes, Does.Contain("mt-8"));
        Assert.That(classes, Does.Contain("border-t"));
    }

    [Test]
    public void Dashed_classes_contain_dashed_border()
    {
        var classes = DividerCss.DashedClasses();
        Assert.That(classes, Does.Contain("border-dashed"));
        Assert.That(classes, Does.Contain("border-t"));
    }

    [Test]
    public void Dashed_merges_css_class()
    {
        var classes = DividerCss.DashedClasses("my-8");
        Assert.That(classes, Does.Contain("my-8"));
        Assert.That(classes, Does.Contain("border-dashed"));
    }

    [Test]
    public void Labeled_wrapper_has_relative_and_margin()
    {
        var classes = DividerCss.LabeledWrapperClasses();
        Assert.That(classes, Does.Contain("relative"));
        Assert.That(classes, Does.Contain("my-4"));
    }

    [Test]
    public void Labeled_wrapper_merges_css_class()
    {
        var classes = DividerCss.LabeledWrapperClasses("my-8");
        Assert.That(classes, Does.Contain("my-8"));
        Assert.That(classes, Does.Contain("relative"));
    }

    [Test]
    public void Labeled_line_outer_has_positioning()
    {
        var classes = DividerCss.LabeledLineOuterClasses();
        Assert.That(classes, Does.Contain("absolute"));
        Assert.That(classes, Does.Contain("inset-0"));
    }

    [Test]
    public void Labeled_line_inner_has_full_width_border()
    {
        var classes = DividerCss.LabeledLineInnerClasses();
        Assert.That(classes, Does.Contain("w-full"));
        Assert.That(classes, Does.Contain("border-t"));
        Assert.That(classes, Does.Contain("border-border"));
    }

    [Test]
    public void Labeled_text_wrapper_has_centered_flex()
    {
        var classes = DividerCss.LabeledTextWrapperClasses();
        Assert.That(classes, Does.Contain("relative"));
        Assert.That(classes, Does.Contain("flex"));
        Assert.That(classes, Does.Contain("justify-center"));
    }

    [Test]
    public void Labeled_text_uses_bg_surface_not_bg_white()
    {
        var classes = DividerCss.LabeledTextClasses();
        Assert.That(classes, Does.Contain("bg-surface"));
        Assert.That(classes, Does.Not.Contain("bg-white"));
    }
}
