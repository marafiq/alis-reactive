using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive.UnitTests.ValidationEnrichment;

[TestFixture]
public class WhenBuildingValidationRangeDescriptorShapes
{
    [Test]
    public void range_descriptors_are_declared_as_arrays_of_the_endpoint_shape()
    {
        var shape = ValidationRangeDescriptorShape.FromEndpointShape(Shape.Number);

        Assert.That(shape, Is.EqualTo(Shape.ArrayOf(Shape.Number)));
    }

    [Test]
    public void missing_endpoint_shape_keeps_an_array_descriptor_with_unconstrained_items()
    {
        var shape = ValidationRangeDescriptorShape.FromEndpointShape(Shape.None);

        Assert.That(shape, Is.EqualTo(Shape.ArrayOf(Shape.Any)));
    }
}
