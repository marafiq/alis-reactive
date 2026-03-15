namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenExecutingServerSideConditionalRules
{
    [Test]
    public void WhenField_truthy_fires_rule_when_condition_is_true()
    {
        var validator = new ReactiveConditionalValidator();
        var model = new TestModel { Name = "John", IsEmployed = true, JobTitle = null };

        var result = validator.Validate(model);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "JobTitle"), Is.True);
    }

    [Test]
    public void WhenField_truthy_skips_rule_when_condition_is_false()
    {
        var validator = new ReactiveConditionalValidator();
        var model = new TestModel { Name = "John", IsEmployed = false, JobTitle = null };

        var result = validator.Validate(model);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void WhenField_eq_fires_rule_when_field_matches_value()
    {
        var validator = new ReactiveEqConditionValidator();
        var model = new TestModel { Name = "Admin", Email = "not-an-email" };

        var result = validator.Validate(model);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "Email"), Is.True);
    }

    [Test]
    public void WhenField_eq_skips_rule_when_field_does_not_match()
    {
        var validator = new ReactiveEqConditionValidator();
        var model = new TestModel { Name = "RegularUser", Email = "not-an-email" };

        var result = validator.Validate(model);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void WhenField_multiple_rules_all_execute_on_server()
    {
        var validator = new ReactiveMultipleRulesValidator();
        var model = new TestModel { IsEmployed = true, JobTitle = "AB", Salary = -1m };

        var result = validator.Validate(model);

        Assert.That(result.IsValid, Is.False);
        // JobTitle too short (minLength 3) + Salary negative (min 0)
        Assert.That(result.Errors.Any(e => e.PropertyName == "JobTitle"), Is.True);
        Assert.That(result.Errors.Any(e => e.PropertyName == "Salary"), Is.True);
    }

    [Test]
    public void WhenField_multiple_rules_skip_when_condition_false()
    {
        var validator = new ReactiveMultipleRulesValidator();
        var model = new TestModel { IsEmployed = false, JobTitle = null, Salary = -1m };

        var result = validator.Validate(model);

        Assert.That(result.IsValid, Is.True);
    }
}
