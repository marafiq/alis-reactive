using Alis.Reactive;

namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenExtractingMultipleRulesPerField
{
    private readonly FluentValidationAdapter _adapter = new();
    private static readonly IReadOnlyDictionary<string, ComponentRegistration> _map = TestComponentsMap.ForTestModel();

    [Test]
    public void All_rules_for_single_field_extracted_in_order()
    {
        var desc = _adapter.ExtractRules(typeof(MultipleRulesValidator), "testForm", _map);

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc!.Fields, Has.Count.EqualTo(1));

        var rules = desc.Fields[0].Rules;
        Assert.That(rules, Has.Count.GreaterThanOrEqualTo(3));

        var ruleTypes = rules.Select(r => r.Rule).ToList();
        Assert.That(ruleTypes, Does.Contain("required"));
        Assert.That(ruleTypes, Does.Contain("minLength"));
        Assert.That(ruleTypes, Does.Contain("maxLength"));
    }
}
