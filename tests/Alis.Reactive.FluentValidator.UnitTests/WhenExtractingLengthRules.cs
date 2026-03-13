using Alis.Reactive;

namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenExtractingLengthRules
{
    private readonly FluentValidationAdapter _adapter = new();
    private static readonly IReadOnlyDictionary<string, ComponentRegistration> _map = TestComponentsMap.ForTestModel();

    [Test]
    public void MaximumLength_produces_maxLength_rule()
    {
        var desc = _adapter.ExtractRules(typeof(MaxLengthValidator), "testForm", _map);

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc!.Fields[0].Rules[0].Rule, Is.EqualTo("maxLength"));
        Assert.That(desc.Fields[0].Rules[0].Constraint, Is.EqualTo(100));
    }

    [Test]
    public void MinimumLength_produces_minLength_rule()
    {
        var desc = _adapter.ExtractRules(typeof(MinLengthValidator), "testForm", _map);

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc!.Fields[0].Rules[0].Rule, Is.EqualTo("minLength"));
        Assert.That(desc.Fields[0].Rules[0].Constraint, Is.EqualTo(3));
    }

    [Test]
    public void Both_minLength_and_maxLength_on_same_field()
    {
        var desc = _adapter.ExtractRules(typeof(BothLengthValidator), "testForm", _map);

        Assert.That(desc, Is.Not.Null);
        var rules = desc!.Fields[0].Rules;
        Assert.That(rules, Has.Count.GreaterThanOrEqualTo(2));

        var ruleTypes = rules.Select(r => r.Rule).ToList();
        Assert.That(ruleTypes, Does.Contain("minLength"));
        Assert.That(ruleTypes, Does.Contain("maxLength"));
    }
}
