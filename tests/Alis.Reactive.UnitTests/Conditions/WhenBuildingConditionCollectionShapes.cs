using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.UnitTests.Conditions;

[TestFixture]
public class WhenBuildingConditionCollectionShapes
{
    [Test]
    public void collection_operands_are_declared_as_arrays_of_the_item_shape()
    {
        var shape = ConditionCollectionShape.FromItemShape(Shape.Number);

        Assert.That(shape, Is.EqualTo(Shape.ArrayOf(Shape.Number)));
    }

    [Test]
    public void missing_item_shape_keeps_a_collection_operand_with_unconstrained_items()
    {
        var shape = ConditionCollectionShape.FromItemShape(Shape.None);

        Assert.That(shape, Is.EqualTo(Shape.ArrayOf(Shape.Any)));
    }
}
