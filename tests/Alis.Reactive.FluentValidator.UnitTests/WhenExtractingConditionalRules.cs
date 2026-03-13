namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenExtractingConditionalRules
{
    private readonly FluentValidationAdapter _adapter = new();

    [Test]
    public void Conditional_rules_with_When_are_skipped()
    {
        var desc = _adapter.ExtractRules(typeof(ConditionalValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        // Only Name should appear (JobTitle has .When() so it's skipped)
        Assert.That(desc!.Fields, Has.Count.EqualTo(1));
        Assert.That(desc.Fields[0].FieldName, Is.EqualTo("Name"));
    }

    [Test]
    public void IConditionalRuleProvider_merges_conditional_rules_with_When_field()
    {
        var desc = _adapter.ExtractRules(typeof(ConditionalProviderValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var fieldNames = desc!.Fields.Select(f => f.FieldName).ToList();
        Assert.That(fieldNames, Does.Contain("Name"));
        Assert.That(fieldNames, Does.Contain("JobTitle"));

        var jobTitleField = desc.Fields.First(f => f.FieldName == "JobTitle");
        Assert.That(jobTitleField.Rules, Has.Count.EqualTo(1));
        Assert.That(jobTitleField.Rules[0].Rule, Is.EqualTo("required"));
        Assert.That(jobTitleField.Rules[0].When, Is.Not.Null);
        Assert.That(jobTitleField.Rules[0].When!.Field, Is.EqualTo("IsEmployed"));
        Assert.That(jobTitleField.Rules[0].When.Op, Is.EqualTo("truthy"));
    }
}
