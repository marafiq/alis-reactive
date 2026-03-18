using Alis.Reactive.DesignSystem.Tokens;
using NUnit.Framework;

namespace Alis.Reactive.DesignSystem.Tests.Tokens;

[TestFixture]
public class WhenMergingClasses
{
    [Test]
    public void Returns_generated_when_user_class_is_null()
    {
        Assert.That(CssUtils.MergeClasses("flex gap-4", null), Is.EqualTo("flex gap-4"));
    }

    [Test]
    public void Returns_generated_when_user_class_is_empty()
    {
        Assert.That(CssUtils.MergeClasses("flex gap-4", ""), Is.EqualTo("flex gap-4"));
    }

    [Test]
    public void Appends_user_class_to_generated()
    {
        Assert.That(CssUtils.MergeClasses("flex gap-4", "mt-2"), Is.EqualTo("flex gap-4 mt-2"));
    }

    [Test]
    public void Trims_whitespace_from_user_class()
    {
        Assert.That(CssUtils.MergeClasses("flex", "  mt-2  "), Is.EqualTo("flex mt-2"));
    }
}
