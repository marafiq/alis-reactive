namespace Alis.Reactive.UnitTests;

/// <summary>
/// ExpressionPathHelper is a property-path DSL, not an evaluator.
/// Computed expressions (method calls, binary ops, static helpers) must
/// throw immediately instead of producing malformed paths silently.
/// </summary>
[TestFixture]
public class WhenRejectingComputedExpressions
{
    public class Address
    {
        public string? City { get; set; }
    }

    public class Model
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public Address? Address { get; set; }
    }

    [Test]
    public void Method_call_throws_instead_of_producing_broken_path()
    {
        Assert.Throws<InvalidOperationException>(() =>
            ExpressionPathHelper.ToEventPath<Model>(m => m.Name!.ToUpper()));
    }

    [Test]
    public void Binary_expression_throws_instead_of_producing_broken_path()
    {
        Assert.Throws<InvalidOperationException>(() =>
            ExpressionPathHelper.ToEventPath<Model>(m => m.Name + "x"));
    }

    [Test]
    public void Static_method_call_throws_instead_of_producing_broken_path()
    {
        Assert.Throws<InvalidOperationException>(() =>
            ExpressionPathHelper.ToEventPath<Model>(m => string.Concat(m.Name, "x")));
    }

    [Test]
    public void Typed_overload_also_rejects_method_calls()
    {
        Assert.Throws<InvalidOperationException>(() =>
            ExpressionPathHelper.ToEventPath<Model, string>(m => m.Name!.ToUpper()));
    }

    [Test]
    public void ToElementId_rejects_method_calls()
    {
        Assert.Throws<InvalidOperationException>(() =>
            ExpressionPathHelper.ToElementId<Model>(m => m.Name!.ToUpper()));
    }

    [Test]
    public void ToPropertyName_rejects_method_calls()
    {
        Assert.Throws<InvalidOperationException>(() =>
            ExpressionPathHelper.ToPropertyName<Model>(m => m.Name!.ToUpper()));
    }
}
