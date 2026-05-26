namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenProjectingRangeRule
{
    private readonly FluentValidationAdapter _adapter = AdapterFactory.Create();

    [Test]
    public void InclusiveBetween_produces_range_rule_with_array_constraint()
    {
        var desc = _adapter.ProjectRules(typeof(RangeValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var rule = desc[0].Rules[0];
        Assert.That(rule.Kind, Is.EqualTo(ValidationRuleKind.Range));

        var range = rule.RangeOperand();
        Assert.That(range.LowerBound, Is.EqualTo(0));
        Assert.That(range.UpperBound, Is.EqualTo(120));
    }
}
