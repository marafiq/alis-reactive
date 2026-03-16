namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenExtractingFalsyAndNeqConditions
{
    private readonly FluentValidationAdapter _adapter = AdapterFactory.Create();

    [Test]
    public void WhenFieldNot_bool_extracts_falsy_condition()
    {
        var desc = _adapter.ExtractRules(typeof(ReactiveFalsyConditionValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var jobTitle = desc!.Fields.First(f => f.FieldName == "JobTitle");
        Assert.That(jobTitle.Rules, Has.Count.EqualTo(1));
        Assert.That(jobTitle.Rules[0].Rule, Is.EqualTo("required"));
        Assert.That(jobTitle.Rules[0].Message, Is.EqualTo("Explain why not employed"));
        Assert.That(jobTitle.Rules[0].When, Is.Not.Null);
        Assert.That(jobTitle.Rules[0].When!.Field, Is.EqualTo("IsEmployed"));
        Assert.That(jobTitle.Rules[0].When.Op, Is.EqualTo("falsy"));
        Assert.That(jobTitle.Rules[0].When.Value, Is.Null);
    }

    [Test]
    public void WhenFieldNot_value_extracts_neq_condition()
    {
        var desc = _adapter.ExtractRules(typeof(ReactiveNeqConditionValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var email = desc!.Fields.First(f => f.FieldName == "Email");
        Assert.That(email.Rules[0].Rule, Is.EqualTo("required"));
        Assert.That(email.Rules[0].When, Is.Not.Null);
        Assert.That(email.Rules[0].When!.Field, Is.EqualTo("Name"));
        Assert.That(email.Rules[0].When.Op, Is.EqualTo("neq"));
        Assert.That(email.Rules[0].When.Value, Is.EqualTo("Independent"));
    }

    [Test]
    public void WhenFieldNot_unconditional_rules_still_extracted()
    {
        var desc = _adapter.ExtractRules(typeof(ReactiveFalsyConditionValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var name = desc!.Fields.First(f => f.FieldName == "Name");
        Assert.That(name.Rules[0].Rule, Is.EqualTo("required"));
        Assert.That(name.Rules[0].When, Is.Null);
    }

    [Test]
    public void Falsy_server_validation_fires_when_bool_is_false()
    {
        var validator = new ReactiveFalsyConditionValidator();
        var model = new TestModel { Name = "John", IsEmployed = false, JobTitle = null };

        var result = validator.Validate(model);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "JobTitle"), Is.True);
    }

    [Test]
    public void Falsy_server_validation_skips_when_bool_is_true()
    {
        var validator = new ReactiveFalsyConditionValidator();
        var model = new TestModel { Name = "John", IsEmployed = true, JobTitle = null };

        var result = validator.Validate(model);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Neq_server_validation_fires_when_value_differs()
    {
        var validator = new ReactiveNeqConditionValidator();
        var model = new TestModel { Name = "Admin", Email = null };

        var result = validator.Validate(model);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == "Email"), Is.True);
    }

    [Test]
    public void Neq_server_validation_skips_when_value_matches()
    {
        var validator = new ReactiveNeqConditionValidator();
        var model = new TestModel { Name = "Independent", Email = null };

        var result = validator.Validate(model);

        Assert.That(result.IsValid, Is.True);
    }
}
