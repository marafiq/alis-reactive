using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenProjectingClientConditionalRules
{
    private readonly FluentValidationAdapter _adapter = AdapterFactory.Create();

    [Test]
    public void WhenField_truthy_projects_conditional_rule()
    {
        var desc = _adapter.ProjectRules(typeof(ReactiveConditionalValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var jobTitle = desc.First(f => f.FieldName == "JobTitle");
        Assert.That(jobTitle.Rules, Has.Count.EqualTo(1));
        Assert.That(jobTitle.Rules[0].Kind, Is.EqualTo(ValidationRuleKind.Required));
        Assert.That(jobTitle.Rules[0].Condition(), Is.Not.Null);
        var when = (FieldCompare)jobTitle.Rules[0].Condition()!;
        Assert.That(when.Field, Is.EqualTo("IsEmployed"));
        Assert.That(when.Op, Is.EqualTo(FieldComparisonOperator.Truthy));
    }

    [Test]
    public void WhenField_unconditional_rules_still_project()
    {
        var desc = _adapter.ProjectRules(typeof(ReactiveConditionalValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var name = desc.First(f => f.FieldName == "Name");
        Assert.That(name.Rules[0].Kind, Is.EqualTo(ValidationRuleKind.Required));
        Assert.That(name.Rules[0].Condition(), Is.Null);
    }

    [Test]
    public void WhenField_multiple_rules_in_block_all_get_condition()
    {
        var desc = _adapter.ProjectRules(typeof(ReactiveMultipleRulesValidator), "testForm");

        Assert.That(desc, Is.Not.Null);

        var jobTitle = desc.First(f => f.FieldName == "JobTitle");
        Assert.That(jobTitle.Rules.Count, Is.GreaterThanOrEqualTo(2));
        Assert.That(jobTitle.Rules.All(r => r.Condition() != null), Is.True);
        Assert.That(jobTitle.Rules.All(r => r.Condition() is FieldCompare fc && fc.Field == "IsEmployed"), Is.True);

        var salary = desc.First(f => f.FieldName == "Salary");
        Assert.That(salary.Rules[0].Condition(), Is.Not.Null);
        var salaryWhen = (FieldCompare)salary.Rules[0].Condition()!;
        Assert.That(salaryWhen.Field, Is.EqualTo("IsEmployed"));
    }

    [Test]
    public void WhenField_eq_projects_equality_condition()
    {
        var desc = _adapter.ProjectRules(typeof(ReactiveEqConditionValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var email = desc.First(f => f.FieldName == "Email");
        Assert.That(email.Rules[0].Condition(), Is.Not.Null);
        var eqWhen = (FieldCompare)email.Rules[0].Condition()!;
        Assert.That(eqWhen.Field, Is.EqualTo("Name"));
        Assert.That(eqWhen.Op, Is.EqualTo(FieldComparisonOperator.Equal));
        Assert.That(eqWhen.OperandValue(), Is.EqualTo("Admin"));
    }

    [Test]
    public void WhenField_direct_nested_property_keeps_full_field_path()
    {
        var desc = _adapter.ProjectRules(typeof(DirectNestedWhenFieldValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var jobTitle = desc.First(f => f.FieldName == "JobTitle");
        Assert.That(jobTitle.Rules[0].Condition(), Is.Not.Null);
        var condition = (FieldCompare)jobTitle.Rules[0].Condition()!;
        Assert.That(condition.Field, Is.EqualTo("Address.City"));
        Assert.That(desc.Select(f => f.FieldName), Does.Contain("Address.City"));
    }

    [Test]
    public void Nested_WhenField_scopes_project_all_active_guards()
    {
        var desc = _adapter.ProjectRules(typeof(NestedReactiveConditionValidator), "testForm");

        Assert.That(desc, Is.Not.Null);

        var notes = desc.First(f => f.FieldName == "Notes");
        Assert.That(notes.Rules, Has.Count.EqualTo(1));

        var condition = notes.Rules[0].Condition();
        Assert.That(condition, Is.InstanceOf<FieldAll>());

        var all = (FieldAll)condition!;
        Assert.That(all.Terms, Has.Count.EqualTo(2));

        var outer = (FieldCompare)all.Terms[0];
        Assert.That(outer.Field, Is.EqualTo("IsEmployed"));
        Assert.That(outer.Op, Is.EqualTo(FieldComparisonOperator.Truthy));

        var inner = (FieldCompare)all.Terms[1];
        Assert.That(inner.Field, Is.EqualTo("CareLevel"));
        Assert.That(inner.Op, Is.EqualTo(FieldComparisonOperator.Equal));
        Assert.That(inner.OperandValue(), Is.EqualTo("memory-care"));

        Assert.That(desc.Select(f => f.FieldName), Does.Contain("IsEmployed"));
        Assert.That(desc.Select(f => f.FieldName), Does.Contain("CareLevel"));
    }

    [Test]
    public void Nested_WhenField_scope_exit_keeps_sibling_rules_on_outer_guard_only()
    {
        var desc = _adapter.ProjectRules(typeof(NestedReactiveConditionValidator), "testForm");

        var jobTitle = desc.First(f => f.FieldName == "JobTitle");
        Assert.That(jobTitle.Rules, Has.Count.EqualTo(1));

        var condition = jobTitle.Rules[0].Condition();
        Assert.That(condition, Is.InstanceOf<FieldCompare>());

        var compare = (FieldCompare)condition!;
        Assert.That(compare.Field, Is.EqualTo("IsEmployed"));
        Assert.That(compare.Op, Is.EqualTo(FieldComparisonOperator.Truthy));
    }

    [Test]
    public void Nested_WhenField_server_validation_uses_the_same_active_guard_stack()
    {
        var validator = new NestedReactiveConditionValidator();

        var bothGuardsPass = validator.Validate(new TestModel
        {
            IsEmployed = true,
            CareLevel = "memory-care",
            Notes = "",
            JobTitle = "Nurse"
        });
        Assert.That(bothGuardsPass.IsValid, Is.False);
        Assert.That(bothGuardsPass.Errors.Select(error => error.PropertyName), Does.Contain(nameof(TestModel.Notes)));

        var outerGuardFails = validator.Validate(new TestModel
        {
            IsEmployed = false,
            CareLevel = "memory-care",
            Notes = "",
            JobTitle = ""
        });
        Assert.That(outerGuardFails.IsValid, Is.True);

        var innerGuardFails = validator.Validate(new TestModel
        {
            IsEmployed = true,
            CareLevel = "assisted",
            Notes = "",
            JobTitle = "Nurse"
        });
        Assert.That(innerGuardFails.IsValid, Is.True);
    }

    [Test]
    public void Plain_When_still_skipped_in_mixed_validator()
    {
        var desc = _adapter.ProjectRules(typeof(ReactiveMixedValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var fieldNames = desc.Select(f => f.FieldName).ToList();

        // Name (unconditional) and JobTitle (WhenField) should be present
        Assert.That(fieldNames, Does.Contain("Name"));
        Assert.That(fieldNames, Does.Contain("JobTitle"));
        // Salary (.When() without WhenField) should not be projected into browser validation.
        Assert.That(fieldNames, Does.Not.Contain("Salary"));
    }

    [Test]
    public void WhenField_all_rule_types_project_with_condition()
    {
        var desc = _adapter.ProjectRules(typeof(ConditionalAllRulesValidator), "testForm");

        Assert.That(desc, Is.Not.Null);

        // Verify each rule type has the IsEmployed condition
        void AssertConditionalRule(string fieldName, ValidationRuleKind expectedRule, string label)
        {
            var field = desc.FirstOrDefault(f => f.FieldName == fieldName);
            Assert.That(field, Is.Not.Null, $"{label}: field '{fieldName}' missing");
            var rule = field!.Rules.FirstOrDefault(r => r.Kind == expectedRule);
            Assert.That(rule, Is.Not.Null, $"{label}: rule '{expectedRule}' missing on '{fieldName}'");
            Assert.That(rule!.Condition(), Is.Not.Null, $"{label}: condition missing");
            var ruleWhen = (FieldCompare)rule.Condition()!;
            Assert.That(ruleWhen.Field, Is.EqualTo("IsEmployed"), $"{label}: wrong condition field");
            Assert.That(ruleWhen.Op, Is.EqualTo(FieldComparisonOperator.Truthy), $"{label}: wrong condition op");
        }

        AssertConditionalRule("Name", ValidationRuleKind.Required, "NotEmpty");
        AssertConditionalRule("Name", ValidationRuleKind.MinLength, "MinimumLength");
        AssertConditionalRule("Name", ValidationRuleKind.MaxLength, "MaximumLength");
        AssertConditionalRule("Email", ValidationRuleKind.Email, "EmailAddress");
        AssertConditionalRule("Phone", ValidationRuleKind.Regex, "Matches");
        AssertConditionalRule("Age", ValidationRuleKind.Range, "InclusiveBetween");
        AssertConditionalRule("Salary", ValidationRuleKind.Min, "GreaterThanOrEqualTo");
        AssertConditionalRule("Salary", ValidationRuleKind.Max, "LessThanOrEqualTo");
        AssertConditionalRule("Salary", ValidationRuleKind.GreaterThan, "GreaterThan");
        AssertConditionalRule("Salary", ValidationRuleKind.LessThan, "LessThan");
        AssertConditionalRule("ConfirmEmail", ValidationRuleKind.EqualTo, "Equal");
    }

    [Test]
    public void WhenField_condition_source_field_included_in_descriptor()
    {
        var desc = _adapter.ProjectRules(typeof(ReactiveConditionalValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var fieldNames = desc.Select(f => f.FieldName).ToList();
        // IsEmployed must appear so the runtime can read its value
        Assert.That(fieldNames, Does.Contain("IsEmployed"));
    }
}
