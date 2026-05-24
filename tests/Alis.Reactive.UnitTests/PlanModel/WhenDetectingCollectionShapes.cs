using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.UnitTests.PlanModel;

[TestFixture]
public class WhenDetectingCollectionShapes
{
    [Test]
    public void enumerable_shapes_are_arrays_with_item_contracts()
    {
        Assert.That(
            Shape.FromClrType(typeof(IEnumerable<string>)),
            Is.EqualTo(Shape.ArrayOf(Shape.String)));
    }

    [Test]
    public void list_shapes_are_arrays_with_item_contracts()
    {
        Assert.That(
            Shape.FromClrType(typeof(List<int>)),
            Is.EqualTo(Shape.ArrayOf(Shape.Number)));
    }

    [Test]
    public void dictionaries_are_not_projected_as_arrays_of_key_value_pairs()
    {
        Assert.That(
            Shape.FromClrType(typeof(Dictionary<string, int>)),
            Is.EqualTo(Shape.Any));
    }

    [Test]
    public void nullable_scalars_do_not_have_collection_item_shape()
    {
        var source = new TestSource<int?>();

        Assert.That(source.ElementShape, Is.EqualTo(Shape.None));
    }

    [Test]
    public void collection_sources_expose_their_item_shape()
    {
        var source = new TestSource<IReadOnlyList<string>>();

        Assert.That(source.ElementShape, Is.EqualTo(Shape.String));
    }

    private sealed class TestSource<T> : TypedSource<T>
    {
        internal override ValueProducer ToValueProducer() => ValueProducer.Null();
    }
}
