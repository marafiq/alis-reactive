using Alis.Reactive.PlanModel;

namespace Alis.Reactive.UnitTests.PlanModel;

[TestFixture]
public sealed class WhenMergingShapeContracts
{
    [Test]
    public void none_is_not_a_value_contract_wildcard()
    {
        var compatibility = ShapeContractCompatibility.MergeContracts(Shape.None, Shape.String);

        Assert.That(compatibility.IsConflict, Is.True);
    }

    [Test]
    public void any_can_be_refined_by_a_declared_contract()
    {
        var compatibility = ShapeContractCompatibility.MergeContracts(Shape.Any, Shape.String);

        Assert.That(compatibility.IsConflict, Is.False);
        Assert.That(compatibility.Shape, Is.EqualTo(Shape.String));
    }

    [Test]
    public void plugin_assignment_keeps_any_dynamic_but_rejects_no_value()
    {
        Assert.That(ShapeContractCompatibility.CanAccept(Shape.Any, Shape.String), Is.True);
        Assert.That(ShapeContractCompatibility.CanAccept(Shape.String, Shape.Any), Is.True);
        Assert.That(ShapeContractCompatibility.CanAccept(Shape.String, Shape.None), Is.False);
    }

    [Test]
    public void array_contracts_merge_item_shapes_when_one_side_is_any()
    {
        var compatibility = ShapeContractCompatibility.MergeContracts(
            Shape.ArrayOf(Shape.Any),
            Shape.ArrayOf(Shape.String));

        Assert.That(compatibility.IsConflict, Is.False);
        Assert.That(compatibility.Shape, Is.EqualTo(Shape.ArrayOf(Shape.String)));
        Assert.That(ShapeContractCompatibility.CanAccept(Shape.ArrayOf(Shape.Any), Shape.ArrayOf(Shape.String)), Is.True);
        Assert.That(ShapeContractCompatibility.CanAccept(Shape.ArrayOf(Shape.String), Shape.ArrayOf(Shape.Any)), Is.True);
    }

    [Test]
    public void array_contracts_reject_conflicting_item_shapes()
    {
        var compatibility = ShapeContractCompatibility.MergeContracts(
            Shape.ArrayOf(Shape.String),
            Shape.ArrayOf(Shape.Number));

        Assert.That(compatibility.IsConflict, Is.True);
        Assert.That(ShapeContractCompatibility.CanAccept(Shape.ArrayOf(Shape.String), Shape.ArrayOf(Shape.Number)), Is.False);
    }

    [Test]
    public void open_object_contract_can_be_refined_by_closed_object_contract()
    {
        var closed = Shape.ObjectOf(new Dictionary<string, Shape>
        {
            ["name"] = Shape.String
        });
        var compatibility = ShapeContractCompatibility.MergeContracts(Shape.OpenObject(), closed);

        Assert.That(compatibility.IsConflict, Is.False);
        Assert.That(compatibility.Shape, Is.EqualTo(closed));
        Assert.That(ShapeContractCompatibility.CanAccept(Shape.OpenObject(), closed), Is.True);
        Assert.That(ShapeContractCompatibility.CanAccept(closed, Shape.OpenObject()), Is.True);
    }

    [Test]
    public void closed_object_contract_accepts_actual_superset()
    {
        var expected = Shape.ObjectOf(new Dictionary<string, Shape>
        {
            ["name"] = Shape.String
        });
        var actual = Shape.ObjectOf(new Dictionary<string, Shape>
        {
            ["name"] = Shape.String,
            ["age"] = Shape.Number
        });

        Assert.That(ShapeContractCompatibility.CanAccept(expected, actual), Is.True);
    }

    [Test]
    public void closed_object_contract_rejects_actual_missing_required_field()
    {
        var expected = Shape.ObjectOf(new Dictionary<string, Shape>
        {
            ["name"] = Shape.String,
            ["age"] = Shape.Number
        });
        var actual = Shape.ObjectOf(new Dictionary<string, Shape>
        {
            ["name"] = Shape.String
        });

        Assert.That(ShapeContractCompatibility.CanAccept(expected, actual), Is.False);
    }

    [Test]
    public void object_contracts_merge_declared_fields()
    {
        var compatibility = ShapeContractCompatibility.MergeContracts(
            Shape.ObjectOf(new Dictionary<string, Shape>
            {
                ["name"] = Shape.String
            }),
            Shape.ObjectOf(new Dictionary<string, Shape>
            {
                ["age"] = Shape.Number
            }));

        Assert.That(compatibility.IsConflict, Is.False);
        Assert.That(compatibility.Shape, Is.EqualTo(Shape.ObjectOf(new Dictionary<string, Shape>
        {
            ["name"] = Shape.String,
            ["age"] = Shape.Number
        })));
    }

    [Test]
    public void object_contracts_reject_conflicting_field_shapes()
    {
        var compatibility = ShapeContractCompatibility.MergeContracts(
            Shape.ObjectOf(new Dictionary<string, Shape>
            {
                ["name"] = Shape.String
            }),
            Shape.ObjectOf(new Dictionary<string, Shape>
            {
                ["name"] = Shape.Number
            }));

        Assert.That(compatibility.IsConflict, Is.True);
    }

    [Test]
    public void shape_descriptions_include_nested_contracts()
    {
        var shape = Shape.ArrayOf(Shape.ObjectOf(new Dictionary<string, Shape>
        {
            ["name"] = Shape.String,
            ["age"] = Shape.Nullable(Shape.Number)
        }));

        Assert.That(shape.DescribeContract(), Is.EqualTo("array<object{name:string, age:nullable<number>}>"));
    }
}
