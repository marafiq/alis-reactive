using Alis.Reactive.DesignSystem.Tokens;
using NUnit.Framework;

namespace Alis.Reactive.DesignSystem.Tests.Tokens;

[TestFixture]
public class WhenMappingTokens
{
    [Test]
    public void Gap_returns_tailwind_class_for_each_spacing_scale()
    {
        Assert.That(TokenMap.Gap(SpacingScale.None), Is.EqualTo("gap-0"));
        Assert.That(TokenMap.Gap(SpacingScale.Base), Is.EqualTo("gap-4"));
        Assert.That(TokenMap.Gap(SpacingScale.Max), Is.EqualTo("gap-16"));
    }

    [Test]
    public void Cols_returns_tailwind_class_for_each_grid_column()
    {
        Assert.That(TokenMap.Cols(GridCols.C1), Is.EqualTo("grid-cols-1"));
        Assert.That(TokenMap.Cols(GridCols.C3), Is.EqualTo("grid-cols-3"));
        Assert.That(TokenMap.Cols(GridCols.C6), Is.EqualTo("grid-cols-6"));
    }

    [Test]
    public void Items_returns_tailwind_class_for_each_alignment()
    {
        Assert.That(TokenMap.Items(AlignItems.Center), Is.EqualTo("items-center"));
        Assert.That(TokenMap.Items(AlignItems.Baseline), Is.EqualTo("items-baseline"));
    }

    [Test]
    public void Justify_returns_tailwind_class_for_each_justification()
    {
        Assert.That(TokenMap.Justify(JustifyContent.Between), Is.EqualTo("justify-between"));
        Assert.That(TokenMap.Justify(JustifyContent.Evenly), Is.EqualTo("justify-evenly"));
    }

    [Test]
    public void Color_returns_tailwind_class_for_each_text_color()
    {
        Assert.That(TokenMap.Color(TextColor.Primary), Is.EqualTo("text-text-primary"));
        Assert.That(TokenMap.Color(TextColor.Error), Is.EqualTo("text-error"));
    }

    [Test]
    public void Size_returns_tailwind_class_for_each_text_size()
    {
        Assert.That(TokenMap.Size(TextSize.Xs), Is.EqualTo("text-xs"));
        Assert.That(TokenMap.Size(TextSize.Xl), Is.EqualTo("text-xl"));
    }

    [Test]
    public void Accent_returns_tailwind_class_for_each_accent_color()
    {
        Assert.That(TokenMap.Accent(AccentColor.Primary), Is.EqualTo("border-accent"));
        Assert.That(TokenMap.Accent(AccentColor.Error), Is.EqualTo("border-error"));
    }
}
