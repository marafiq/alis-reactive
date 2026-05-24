using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenProjectingConditionalRules
{
    private readonly FluentValidationAdapter _adapter = AdapterFactory.Create();

    [Test]
    public void Conditional_rules_with_When_are_skipped()
    {
        var desc = _adapter.ProjectRules(typeof(ConditionalValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        // Only Name should appear (JobTitle has .When() so it's skipped)
        Assert.That(desc, Has.Count.EqualTo(1));
        Assert.That(desc[0].FieldName, Is.EqualTo("Name"));
    }

    [Test]
    public void ReactiveValidator_WhenField_merges_conditional_rules()
    {
        var desc = _adapter.ProjectRules(typeof(ConditionalProviderValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var fieldNames = desc.Select(f => f.FieldName).ToList();
        Assert.That(fieldNames, Does.Contain("Name"));
        Assert.That(fieldNames, Does.Contain("JobTitle"));

        var jobTitleField = desc.First(f => f.FieldName == "JobTitle");
        Assert.That(jobTitleField.Rules, Has.Count.EqualTo(1));
        Assert.That(jobTitleField.Rules[0].Rule, Is.EqualTo("required"));
        Assert.That(jobTitleField.Rules[0].Condition(), Is.Not.Null);
        var when = (FieldCompare)jobTitleField.Rules[0].Condition()!;
        Assert.That(when.Field, Is.EqualTo("IsEmployed"));
        Assert.That(when.Op, Is.EqualTo("truthy"));
    }

    [Test]
    public void Server_only_When_wrapping_WhenField_skips_client_projection()
    {
        var report = _adapter.ProjectValidation(typeof(ServerOnlyWhenWrapsClientGuardValidator), "testForm");

        Assert.That(report.Fields.Select(field => field.FieldName), Does.Not.Contain("JobTitle"));
        Assert.That(report.SkippedRules, Has.Count.EqualTo(1));
        Assert.That(report.SkippedRules[0].FieldName, Is.EqualTo("JobTitle"));
        Assert.That(
            report.SkippedRules[0].Reason,
            Is.EqualTo(ClientRuleProjectionSkipReason.FluentValidationConditionWithoutClientGuard));

        var validator = new ServerOnlyWhenWrapsClientGuardValidator();
        var result = validator.Validate(new TestModel
        {
            Age = 17,
            IsEmployed = true,
            JobTitle = ""
        });
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void WhenField_wrapping_server_only_When_skips_client_projection()
    {
        var report = _adapter.ProjectValidation(typeof(ClientGuardWrapsServerOnlyWhenValidator), "testForm");

        Assert.That(report.Fields.Select(field => field.FieldName), Does.Not.Contain("JobTitle"));
        Assert.That(report.SkippedRules, Has.Count.EqualTo(1));
        Assert.That(report.SkippedRules[0].FieldName, Is.EqualTo("JobTitle"));
        Assert.That(
            report.SkippedRules[0].Reason,
            Is.EqualTo(ClientRuleProjectionSkipReason.FluentValidationConditionWithoutClientGuard));
    }

    [Test]
    public void Server_only_Otherwise_wrapping_WhenField_skips_client_projection()
    {
        var report = _adapter.ProjectValidation(typeof(ServerOnlyOtherwiseWrapsClientGuardValidator), "testForm");

        Assert.That(report.Fields.Select(field => field.FieldName), Does.Not.Contain("JobTitle"));
        var jobTitleSkip = report.SkippedRules.Single(rule => rule.FieldName == "JobTitle");
        Assert.That(
            jobTitleSkip.Reason,
            Is.EqualTo(ClientRuleProjectionSkipReason.FluentValidationConditionWithoutClientGuard));
    }
}
