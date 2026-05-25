namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenProjectingComparisonRules
{
    private readonly FluentValidationAdapter _adapter = AdapterFactory.Create();

    [Test]
    public void GreaterThanOrEqualTo_produces_min_rule_with_number_coercion()
    {
        var desc = _adapter.ProjectRules(typeof(MinComparisonValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc[0].Rules[0].Kind, Is.EqualTo(ValidationRuleKind.Min));
        Assert.That(desc[0].Rules[0].ConstraintValue(), Is.EqualTo(0m));
        Assert.That(desc[0].Rules[0].Shape, Is.EqualTo(Alis.Reactive.PlanModel.Shape.Number));
    }

    [Test]
    public void LessThanOrEqualTo_produces_max_rule_with_number_coercion()
    {
        var desc = _adapter.ProjectRules(typeof(MaxComparisonValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc[0].Rules[0].Kind, Is.EqualTo(ValidationRuleKind.Max));
        Assert.That(desc[0].Rules[0].ConstraintValue(), Is.EqualTo(500000m));
        Assert.That(desc[0].Rules[0].Shape, Is.EqualTo(Alis.Reactive.PlanModel.Shape.Number));
    }

    [Test]
    public void GreaterThan_produces_gt_rule_with_number_coercion()
    {
        var desc = _adapter.ProjectRules(typeof(StrictGreaterThanValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc[0].Rules.Count, Is.EqualTo(1));
        Assert.That(desc[0].Rules[0].Kind, Is.EqualTo(ValidationRuleKind.GreaterThan));
        Assert.That(desc[0].Rules[0].ConstraintValue(), Is.EqualTo(0m));
        Assert.That(desc[0].Rules[0].Shape, Is.EqualTo(Alis.Reactive.PlanModel.Shape.Number));
    }

    [Test]
    public void LessThan_produces_lt_rule_with_number_coercion()
    {
        var desc = _adapter.ProjectRules(typeof(StrictLessThanValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc[0].Rules.Count, Is.EqualTo(1));
        Assert.That(desc[0].Rules[0].Kind, Is.EqualTo(ValidationRuleKind.LessThan));
        Assert.That(desc[0].Rules[0].ConstraintValue(), Is.EqualTo(1000000m));
        Assert.That(desc[0].Rules[0].Shape, Is.EqualTo(Alis.Reactive.PlanModel.Shape.Number));
    }
}
