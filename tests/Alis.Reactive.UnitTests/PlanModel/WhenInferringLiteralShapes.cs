using Alis.Reactive.PlanModel;

namespace Alis.Reactive.UnitTests.PlanModel;

[TestFixture]
public class WhenInferringLiteralShapes
{
    [Test]
    public void null_literal_has_no_value_shape()
    {
        Assert.That(Shape.FromValue(null), Is.EqualTo(Shape.None));
    }

    [Test]
    public void unknown_clr_type_is_still_dynamic_any()
    {
        Assert.That(Shape.FromUnknownClrType(null), Is.EqualTo(Shape.Any));
    }
}
