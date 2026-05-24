using Alis.Reactive.Validation;
using FluentValidation;

namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenExtractingComposedConditions
{
    private readonly FluentValidationAdapter _adapter = AdapterFactory.Create();

    // ── And composition ────────────────────────────────────────────────────

    [Test]
    public void WhenFields_And_extracts_FieldAll_with_two_terms()
    {
        var fields = _adapter.ExtractRules(typeof(WhenFieldsAndValidator), "form");
        var jobTitle = fields.First(f => f.FieldName == "JobTitle");
        var when = jobTitle.Rules[0].Condition();

        Assert.That(when, Is.InstanceOf<FieldAll>());
        var all = (FieldAll)when!;
        Assert.That(all.Terms, Has.Count.EqualTo(2));

        var left = (FieldCompare)all.Terms[0];
        Assert.That(left.Field, Is.EqualTo("IsEmployed"));
        Assert.That(left.Op, Is.EqualTo("truthy"));

        var right = (FieldCompare)all.Terms[1];
        Assert.That(right.Field, Is.EqualTo("Age"));
        Assert.That(right.Op, Is.EqualTo("gte"));
        Assert.That(right.OperandValue(), Is.EqualTo(18));
    }

    [Test]
    public void WhenFields_And_includes_all_source_fields()
    {
        var fields = _adapter.ExtractRules(typeof(WhenFieldsAndValidator), "form");
        var fieldNames = fields.Select(f => f.FieldName).ToList();

        Assert.That(fieldNames, Does.Contain("IsEmployed"));
        Assert.That(fieldNames, Does.Contain("Age"));
    }

    // ── Or composition ─────────────────────────────────────────────────────

    [Test]
    public void WhenFields_Or_extracts_FieldAny_with_two_terms()
    {
        var fields = _adapter.ExtractRules(typeof(WhenFieldsOrValidator), "form");
        var notes = fields.First(f => f.FieldName == "Notes");
        var when = notes.Rules[0].Condition();

        Assert.That(when, Is.InstanceOf<FieldAny>());
        var any = (FieldAny)when!;
        Assert.That(any.Terms, Has.Count.EqualTo(2));

        var left = (FieldCompare)any.Terms[0];
        Assert.That(left.Field, Is.EqualTo("CareLevel"));
        Assert.That(left.Op, Is.EqualTo("eq"));
        Assert.That(left.OperandValue(), Is.EqualTo("memory-care"));

        var right = (FieldCompare)any.Terms[1];
        Assert.That(right.Field, Is.EqualTo("CareLevel"));
        Assert.That(right.Op, Is.EqualTo("eq"));
        Assert.That(right.OperandValue(), Is.EqualTo("skilled-nursing"));
    }

    // ── Not composition ────────────────────────────────────────────────────

    [Test]
    public void WhenFields_Not_extracts_FieldNot_wrapping_inner()
    {
        var fields = _adapter.ExtractRules(typeof(WhenFieldsNotValidator), "form");
        var notes = fields.First(f => f.FieldName == "Notes");
        var when = notes.Rules[0].Condition();

        Assert.That(when, Is.InstanceOf<FieldNot>());
        var not = (FieldNot)when!;

        var inner = (FieldCompare)not.Term;
        Assert.That(inner.Field, Is.EqualTo("IsEmployed"));
        Assert.That(inner.Op, Is.EqualTo("truthy"));
    }

    // ── Complex composition ────────────────────────────────────────────────

    [Test]
    public void WhenFields_complex_extracts_nested_tree()
    {
        // Validator: (employed AND salary > 50k) OR (age >= 65)
        var fields = _adapter.ExtractRules(typeof(WhenFieldsComplexValidator), "form");
        var email = fields.First(f => f.FieldName == "Email");
        var when = email.Rules[0].Condition();

        // Top level: Any (OR)
        Assert.That(when, Is.InstanceOf<FieldAny>());
        var or = (FieldAny)when!;
        Assert.That(or.Terms, Has.Count.EqualTo(2));

        // Left branch: All (AND) of (employed, salary > 50k)
        Assert.That(or.Terms[0], Is.InstanceOf<FieldAll>());
        var and = (FieldAll)or.Terms[0];
        Assert.That(and.Terms, Has.Count.EqualTo(2));

        var employed = (FieldCompare)and.Terms[0];
        Assert.That(employed.Field, Is.EqualTo("IsEmployed"));
        Assert.That(employed.Op, Is.EqualTo("truthy"));

        var salary = (FieldCompare)and.Terms[1];
        Assert.That(salary.Field, Is.EqualTo("Salary"));
        Assert.That(salary.Op, Is.EqualTo("gt"));
        Assert.That(salary.OperandValue(), Is.EqualTo(50000m));

        // Right branch: age >= 65
        var age = (FieldCompare)or.Terms[1];
        Assert.That(age.Field, Is.EqualTo("Age"));
        Assert.That(age.Op, Is.EqualTo("gte"));
        Assert.That(age.OperandValue(), Is.EqualTo(65));
    }

    [Test]
    public void WhenFields_complex_includes_all_source_fields()
    {
        var fields = _adapter.ExtractRules(typeof(WhenFieldsComplexValidator), "form");
        var fieldNames = fields.Select(f => f.FieldName).ToList();

        Assert.That(fieldNames, Does.Contain("IsEmployed"));
        Assert.That(fieldNames, Does.Contain("Salary"));
        Assert.That(fieldNames, Does.Contain("Age"));
    }

    // ── Server predicate verification ──────────────────────────────────────

    [Test]
    public void WhenFields_And_server_predicate_matches_both_conditions()
    {
        // Validator: employed AND age >= 18
        // Server validation should require both conditions
        var validator = new WhenFieldsAndValidator();

        // Both true → rules apply → validation should fail (empty JobTitle)
        var resultBothTrue = validator.Validate(new TestModel
        {
            IsEmployed = true, Age = 25, JobTitle = ""
        });
        Assert.That(resultBothTrue.IsValid, Is.False, "Both conditions met + empty field = invalid");

        // First false → rules skip → validation passes
        var resultFirstFalse = validator.Validate(new TestModel
        {
            IsEmployed = false, Age = 25, JobTitle = ""
        });
        Assert.That(resultFirstFalse.IsValid, Is.True, "First condition false = rules skipped");

        // Second false → rules skip → validation passes
        var resultSecondFalse = validator.Validate(new TestModel
        {
            IsEmployed = true, Age = 15, JobTitle = ""
        });
        Assert.That(resultSecondFalse.IsValid, Is.True, "Second condition false = rules skipped");
    }

    [Test]
    public void WhenFields_Or_server_predicate_matches_either_condition()
    {
        var validator = new WhenFieldsOrValidator();

        // First true → rules apply
        var result1 = validator.Validate(new TestModel { CareLevel = "memory-care", Notes = "" });
        Assert.That(result1.IsValid, Is.False, "First condition met + empty Notes = invalid");

        // Second true → rules apply
        var result2 = validator.Validate(new TestModel { CareLevel = "skilled-nursing", Notes = "" });
        Assert.That(result2.IsValid, Is.False, "Second condition met + empty Notes = invalid");

        // Neither true → rules skip
        var result3 = validator.Validate(new TestModel { CareLevel = "independent", Notes = "" });
        Assert.That(result3.IsValid, Is.True, "No condition met = rules skipped");
    }

    [Test]
    public void WhenFields_And_server_predicate_short_circuits_when_left_side_fails()
    {
        var validator = new ShortCircuitAndValidator();

        var result = validator.Validate(new ShortCircuitModel
        {
            Gate = false,
            Target = "",
            Nested = null
        });

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void WhenFields_Or_server_predicate_short_circuits_when_left_side_passes()
    {
        var validator = new ShortCircuitOrValidator();

        var result = validator.Validate(new ShortCircuitModel
        {
            Gate = true,
            Target = "",
            Nested = null
        });

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors[0].PropertyName, Is.EqualTo(nameof(ShortCircuitModel.Target)));
    }

    [Test]
    public void WhenFields_Not_server_predicate_inverts_condition()
    {
        var validator = new WhenFieldsNotValidator();

        // IsEmployed = true → NOT truthy = false → rules skip
        var result1 = validator.Validate(new TestModel { IsEmployed = true, Notes = "" });
        Assert.That(result1.IsValid, Is.True, "NOT(truthy) = false = rules skipped");

        // IsEmployed = false → NOT truthy = true → rules apply
        var result2 = validator.Validate(new TestModel { IsEmployed = false, Notes = "" });
        Assert.That(result2.IsValid, Is.False, "NOT(falsy) = true = rules apply, empty Notes invalid");
    }

}

public sealed class ShortCircuitModel
{
    public bool Gate { get; set; }
    public string Target { get; set; } = "";
    public ShortCircuitNested? Nested { get; set; }
}

public sealed class ShortCircuitNested
{
    public string Code { get; set; } = "";
}

public sealed class ShortCircuitAndValidator : ReactiveValidator<ShortCircuitModel>
{
    public ShortCircuitAndValidator()
    {
        WhenFields(
            c => c.Field(x => x.Gate).Truthy()
                  .And(c.Field(x => x.Nested!.Code).Eq("active")),
            () => RuleFor(x => x.Target).NotEmpty());
    }
}

public sealed class ShortCircuitOrValidator : ReactiveValidator<ShortCircuitModel>
{
    public ShortCircuitOrValidator()
    {
        WhenFields(
            c => c.Field(x => x.Gate).Truthy()
                  .Or(c.Field(x => x.Nested!.Code).Eq("active")),
            () => RuleFor(x => x.Target).NotEmpty());
    }
}
