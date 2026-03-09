namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenExtractingRangeRule
{
    private readonly FluentValidationAdapter _adapter = new();

    [Test]
    public void InclusiveBetween_produces_range_rule_with_array_constraint()
    {
        var desc = _adapter.ExtractRules(typeof(RangeValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var rule = desc!.Fields[0].Rules[0];
        Assert.That(rule.Rule, Is.EqualTo("range"));

        var constraint = rule.Constraint as object[];
        Assert.That(constraint, Is.Not.Null);
        Assert.That(constraint!, Has.Length.EqualTo(2));
        Assert.That(constraint[0], Is.EqualTo(0));
        Assert.That(constraint[1], Is.EqualTo(120));
    }
}
