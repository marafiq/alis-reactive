namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenExtractingComparisonRules
{
    private readonly FluentValidationAdapter _adapter = AdapterFactory.Create();

    [Test]
    public void GreaterThanOrEqualTo_produces_min_rule()
    {
        var desc = _adapter.ExtractRules(typeof(MinComparisonValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc!.Fields[0].Rules[0].Rule, Is.EqualTo("min"));
        Assert.That(desc.Fields[0].Rules[0].Constraint, Is.EqualTo(0m));
    }

    [Test]
    public void LessThanOrEqualTo_produces_max_rule()
    {
        var desc = _adapter.ExtractRules(typeof(MaxComparisonValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc!.Fields[0].Rules[0].Rule, Is.EqualTo("max"));
        Assert.That(desc.Fields[0].Rules[0].Constraint, Is.EqualTo(500000m));
    }

    [Test]
    public void GreaterThan_produces_gt_rule()
    {
        var desc = _adapter.ExtractRules(typeof(StrictGreaterThanValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc!.Fields[0].Rules.Count, Is.EqualTo(1));
        Assert.That(desc.Fields[0].Rules[0].Rule, Is.EqualTo("gt"));
        Assert.That(desc.Fields[0].Rules[0].Constraint, Is.EqualTo(0m));
    }

    [Test]
    public void LessThan_produces_lt_rule()
    {
        var desc = _adapter.ExtractRules(typeof(StrictLessThanValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc!.Fields[0].Rules.Count, Is.EqualTo(1));
        Assert.That(desc.Fields[0].Rules[0].Rule, Is.EqualTo("lt"));
        Assert.That(desc.Fields[0].Rules[0].Constraint, Is.EqualTo(1000000m));
    }
}
