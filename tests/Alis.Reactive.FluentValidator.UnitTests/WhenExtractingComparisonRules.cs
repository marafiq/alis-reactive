using Alis.Reactive;

namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenExtractingComparisonRules
{
    private readonly FluentValidationAdapter _adapter = new();
    private static readonly IReadOnlyDictionary<string, ComponentRegistration> _map = TestComponentsMap.ForTestModel();

    [Test]
    public void GreaterThanOrEqualTo_produces_min_rule()
    {
        var desc = _adapter.ExtractRules(typeof(MinComparisonValidator), "testForm", _map);

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc!.Fields[0].Rules[0].Rule, Is.EqualTo("min"));
        Assert.That(desc.Fields[0].Rules[0].Constraint, Is.EqualTo(0m));
    }

    [Test]
    public void LessThanOrEqualTo_produces_max_rule()
    {
        var desc = _adapter.ExtractRules(typeof(MaxComparisonValidator), "testForm", _map);

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc!.Fields[0].Rules[0].Rule, Is.EqualTo("max"));
        Assert.That(desc.Fields[0].Rules[0].Constraint, Is.EqualTo(500000m));
    }
}
