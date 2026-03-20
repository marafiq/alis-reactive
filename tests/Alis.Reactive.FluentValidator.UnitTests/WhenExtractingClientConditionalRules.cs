namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenExtractingClientConditionalRules
{
    private readonly FluentValidationAdapter _adapter = AdapterFactory.Create();

    [Test]
    public void WhenField_truthy_extracts_conditional_rule()
    {
        var desc = _adapter.ExtractRules(typeof(ReactiveConditionalValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var jobTitle = desc!.Fields.First(f => f.FieldName == "JobTitle");
        Assert.That(jobTitle.Rules, Has.Count.EqualTo(1));
        Assert.That(jobTitle.Rules[0].Rule, Is.EqualTo("required"));
        Assert.That(jobTitle.Rules[0].When, Is.Not.Null);
        Assert.That(jobTitle.Rules[0].When!.Field, Is.EqualTo("IsEmployed"));
        Assert.That(jobTitle.Rules[0].When.Op, Is.EqualTo("truthy"));
    }

    [Test]
    public void WhenField_unconditional_rules_still_extracted()
    {
        var desc = _adapter.ExtractRules(typeof(ReactiveConditionalValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var name = desc!.Fields.First(f => f.FieldName == "Name");
        Assert.That(name.Rules[0].Rule, Is.EqualTo("required"));
        Assert.That(name.Rules[0].When, Is.Null);
    }

    [Test]
    public void WhenField_multiple_rules_in_block_all_get_condition()
    {
        var desc = _adapter.ExtractRules(typeof(ReactiveMultipleRulesValidator), "testForm");

        Assert.That(desc, Is.Not.Null);

        var jobTitle = desc!.Fields.First(f => f.FieldName == "JobTitle");
        Assert.That(jobTitle.Rules.Count, Is.GreaterThanOrEqualTo(2));
        Assert.That(jobTitle.Rules.All(r => r.When != null), Is.True);
        Assert.That(jobTitle.Rules.All(r => r.When!.Field == "IsEmployed"), Is.True);

        var salary = desc.Fields.First(f => f.FieldName == "Salary");
        Assert.That(salary.Rules[0].When, Is.Not.Null);
        Assert.That(salary.Rules[0].When!.Field, Is.EqualTo("IsEmployed"));
    }

    [Test]
    public void WhenField_eq_extracts_equality_condition()
    {
        var desc = _adapter.ExtractRules(typeof(ReactiveEqConditionValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var email = desc!.Fields.First(f => f.FieldName == "Email");
        Assert.That(email.Rules[0].When, Is.Not.Null);
        Assert.That(email.Rules[0].When!.Field, Is.EqualTo("Name"));
        Assert.That(email.Rules[0].When.Op, Is.EqualTo("eq"));
        Assert.That(email.Rules[0].When.Value, Is.EqualTo("Admin"));
    }

    [Test]
    public void Plain_When_still_skipped_in_mixed_validator()
    {
        var desc = _adapter.ExtractRules(typeof(ReactiveMixedValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var fieldNames = desc!.Fields.Select(f => f.FieldName).ToList();

        // Name (unconditional) and JobTitle (WhenField) should be present
        Assert.That(fieldNames, Does.Contain("Name"));
        Assert.That(fieldNames, Does.Contain("JobTitle"));
        // Salary (.When() server-only) should NOT be present
        Assert.That(fieldNames, Does.Not.Contain("Salary"));
    }

    [Test]
    public void WhenField_all_rule_types_extract_with_condition()
    {
        var desc = _adapter.ExtractRules(typeof(ConditionalAllRulesValidator), "testForm");

        Assert.That(desc, Is.Not.Null);

        // Verify each rule type has the IsEmployed condition
        void AssertConditionalRule(string fieldName, string expectedRule, string label)
        {
            var field = desc!.Fields.FirstOrDefault(f => f.FieldName == fieldName);
            Assert.That(field, Is.Not.Null, $"{label}: field '{fieldName}' missing");
            var rule = field!.Rules.FirstOrDefault(r => r.Rule == expectedRule);
            Assert.That(rule, Is.Not.Null, $"{label}: rule '{expectedRule}' missing on '{fieldName}'");
            Assert.That(rule!.When, Is.Not.Null, $"{label}: condition missing");
            Assert.That(rule.When!.Field, Is.EqualTo("IsEmployed"), $"{label}: wrong condition field");
            Assert.That(rule.When.Op, Is.EqualTo("truthy"), $"{label}: wrong condition op");
        }

        AssertConditionalRule("Name", "required", "NotEmpty");
        AssertConditionalRule("Name", "minLength", "MinimumLength");
        AssertConditionalRule("Name", "maxLength", "MaximumLength");
        AssertConditionalRule("Email", "email", "EmailAddress");
        AssertConditionalRule("Phone", "regex", "Matches");
        AssertConditionalRule("Age", "range", "InclusiveBetween");
        AssertConditionalRule("Salary", "min", "GreaterThanOrEqualTo");
        AssertConditionalRule("Salary", "max", "LessThanOrEqualTo");
        AssertConditionalRule("Salary", "gt", "GreaterThan");
        AssertConditionalRule("Salary", "lt", "LessThan");
        AssertConditionalRule("ConfirmEmail", "equalTo", "Equal");
    }

    [Test]
    public void WhenField_condition_source_field_included_in_descriptor()
    {
        var desc = _adapter.ExtractRules(typeof(ReactiveConditionalValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var fieldNames = desc!.Fields.Select(f => f.FieldName).ToList();
        // IsEmployed must appear so the runtime can read its value
        Assert.That(fieldNames, Does.Contain("IsEmployed"));
    }
}
