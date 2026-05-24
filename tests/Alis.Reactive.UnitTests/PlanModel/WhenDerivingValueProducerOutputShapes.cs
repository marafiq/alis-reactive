using Alis.Reactive.PlanModel;

namespace Alis.Reactive.UnitTests.PlanModel;

[TestFixture]
public class WhenDerivingValueProducerOutputShapes
{
    [Test]
    public void object_producers_derive_closed_shapes_from_field_outputs()
    {
        var producer = ValueProducer.Object(new Dictionary<string, ValueProducer>
        {
            ["name"] = ValueProducer.Literal("Ada"),
            ["age"] = ValueProducer.Literal(42)
        });

        Assert.That(producer.OutputShape, Is.EqualTo(Shape.ObjectOf(new Dictionary<string, Shape>
        {
            ["name"] = Shape.String,
            ["age"] = Shape.Number
        })));
    }

    [Test]
    public void array_producers_derive_homogeneous_item_shapes()
    {
        var producer = ValueProducer.Array(new[]
        {
            ValueProducer.Literal("alpha"),
            ValueProducer.Literal("beta")
        });

        Assert.That(producer.OutputShape, Is.EqualTo(Shape.ArrayOf(Shape.String)));
    }

    [Test]
    public void empty_array_producers_are_arrays_with_unconstrained_items()
    {
        var producer = ValueProducer.Array(Array.Empty<ValueProducer>());

        Assert.That(producer.OutputShape, Is.EqualTo(Shape.ArrayOf(Shape.Any)));
    }

    [Test]
    public void mixed_array_producers_are_arrays_with_unconstrained_items()
    {
        var producer = ValueProducer.Array(new[]
        {
            ValueProducer.Literal("alpha"),
            ValueProducer.Literal(42)
        });

        Assert.That(producer.OutputShape, Is.EqualTo(Shape.ArrayOf(Shape.Any)));
    }

    [Test]
    public void null_only_array_producers_are_arrays_with_unconstrained_items()
    {
        var producer = ValueProducer.Array(new[]
        {
            ValueProducer.Null()
        });

        Assert.That(producer.OutputShape, Is.EqualTo(Shape.ArrayOf(Shape.Any)));
    }
}
