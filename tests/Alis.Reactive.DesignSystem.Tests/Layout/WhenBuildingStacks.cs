using Alis.Reactive.DesignSystem.Layout;
using Alis.Reactive.DesignSystem.Tokens;
using NUnit.Framework;

namespace Alis.Reactive.DesignSystem.Tests.Layout;

[TestFixture]
public class WhenBuildingStacks
{
    [Test]
    public void VStack_classes_include_flex_col_and_gap()
    {
        var classes = VStackCss.Classes(SpacingScale.Base);
        Assert.That(classes, Does.Contain("flex"));
        Assert.That(classes, Does.Contain("flex-col"));
        Assert.That(classes, Does.Contain("gap-4"));
    }

    [Test]
    public void VStack_with_divideY_includes_divide_classes()
    {
        var classes = VStackCss.Classes(SpacingScale.Base, divideY: true);
        Assert.That(classes, Does.Contain("divide-y"));
    }

    [Test]
    public void HStack_classes_include_flex_and_gap()
    {
        var classes = HStackCss.Classes(SpacingScale.Sm);
        Assert.That(classes, Does.Contain("flex"));
        Assert.That(classes, Does.Contain("gap-2"));
        Assert.That(classes, Does.Contain("items-center"));
    }

    [Test]
    public void HStack_with_wrap_includes_flex_wrap()
    {
        var classes = HStackCss.Classes(SpacingScale.Sm, wrap: true);
        Assert.That(classes, Does.Contain("flex-wrap"));
    }

    [Test]
    public void HStack_with_custom_alignment()
    {
        var classes = HStackCss.Classes(SpacingScale.Sm, align: AlignItems.Start, justify: JustifyContent.Between);
        Assert.That(classes, Does.Contain("items-start"));
        Assert.That(classes, Does.Contain("justify-between"));
    }
}
