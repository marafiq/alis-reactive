namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenExtractingAllRuleTypes
{
    private readonly FluentValidationAdapter _adapter = new();

    [Test]
    public void All_FV_mappable_rule_types_extracted_correctly()
    {
        var desc = _adapter.ExtractRules(typeof(AllRulesValidator), "testForm");

        Assert.That(desc, Is.Not.Null);

        var fieldNames = desc!.Fields.Select(f => f.FieldName).ToList();
        Assert.That(fieldNames, Does.Contain("Name"));
        Assert.That(fieldNames, Does.Contain("Email"));
        Assert.That(fieldNames, Does.Contain("Phone"));
        Assert.That(fieldNames, Does.Contain("Age"));
        Assert.That(fieldNames, Does.Contain("Salary"));
    }

    [Test]
    public void Name_has_required_and_maxLength()
    {
        var desc = _adapter.ExtractRules(typeof(AllRulesValidator), "testForm");

        var nameField = desc!.Fields.First(f => f.FieldName == "Name");
        var ruleTypes = nameField.Rules.Select(r => r.Rule).ToList();
        Assert.That(ruleTypes, Does.Contain("required"));
        Assert.That(ruleTypes, Does.Contain("maxLength"));
    }

    [Test]
    public void Email_has_email_rule()
    {
        var desc = _adapter.ExtractRules(typeof(AllRulesValidator), "testForm");

        var emailField = desc!.Fields.First(f => f.FieldName == "Email");
        Assert.That(emailField.Rules[0].Rule, Is.EqualTo("email"));
    }

    [Test]
    public void Phone_has_regex_rule()
    {
        var desc = _adapter.ExtractRules(typeof(AllRulesValidator), "testForm");

        var phoneField = desc!.Fields.First(f => f.FieldName == "Phone");
        Assert.That(phoneField.Rules[0].Rule, Is.EqualTo("regex"));
    }

    [Test]
    public void Age_has_range_rule()
    {
        var desc = _adapter.ExtractRules(typeof(AllRulesValidator), "testForm");

        var ageField = desc!.Fields.First(f => f.FieldName == "Age");
        Assert.That(ageField.Rules[0].Rule, Is.EqualTo("range"));
    }

    [Test]
    public void Salary_has_min_and_max_rules()
    {
        var desc = _adapter.ExtractRules(typeof(AllRulesValidator), "testForm");

        var salaryField = desc!.Fields.First(f => f.FieldName == "Salary");
        var ruleTypes = salaryField.Rules.Select(r => r.Rule).ToList();
        Assert.That(ruleTypes, Does.Contain("min"));
        Assert.That(ruleTypes, Does.Contain("max"));
    }
}
