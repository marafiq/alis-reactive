using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator.UnitTests;

/// <summary>
/// WhenField<DateTime> and WhenFieldNot<DateTime> serialize condition values
/// using the same date-shaped plan literals as validation constraints.
/// </summary>
[TestFixture]
public class WhenProjectingDateTimeConditions
{
    private readonly FluentValidationAdapter _adapter = AdapterFactory.Create();

    [Test]
    public void WhenField_DateTime_eq_serializes_value_as_date_literal()
    {
        var desc = _adapter.ProjectRules(typeof(DateTimeConditionValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var nameField = desc.First(f => f.FieldName == "Name");
        Assert.That(nameField.Rules, Has.Count.EqualTo(1));
        var when = (FieldCompare)nameField.Rules[0].WhenCondition();
        Assert.That(when.Field, Is.EqualTo("AdmissionDate"));
        Assert.That(when.Op, Is.EqualTo(FieldComparisonOperator.Equal));

        var condValue = when.OperandValue();
        Assert.That(condValue, Is.TypeOf<string>(),
            "DateTime condition value must use the date-shaped plan literal format");
        Assert.That(condValue, Is.EqualTo("2026-07-01"));
    }

    [Test]
    public void WhenFieldNot_DateTime_neq_serializes_value_as_date_literal()
    {
        var desc = _adapter.ProjectRules(typeof(DateTimeNeqConditionValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var scoreField = desc.First(f => f.FieldName == "Score");
        var neqWhen = (FieldCompare)scoreField.Rules[0].WhenCondition();
        Assert.That(neqWhen.Field, Is.EqualTo("AdmissionDate"));
        Assert.That(neqWhen.Op, Is.EqualTo(FieldComparisonOperator.NotEqual));

        var condValue = neqWhen.OperandValue();
        Assert.That(condValue, Is.TypeOf<string>(),
            "DateTime condition value must use the date-shaped plan literal format");
        Assert.That(condValue, Is.EqualTo("2026-01-01"));
    }

    [Test]
    public void WhenFieldNot_string_neq_keeps_string_value()
    {
        // Verify non-DateTime conditions are NOT affected by Unix ms serialization
        var desc = _adapter.ProjectRules(typeof(ReactiveNeqConditionValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var emailField = desc.First(f => f.FieldName == "Email");
        var condition = emailField.Rules[0].WhenCondition();
        Assert.That(condition, Is.Not.Null);
        var strWhen = (FieldCompare)condition!;
        Assert.That(strWhen.OperandValue(), Is.EqualTo("Independent"),
            "String condition values must remain as strings, not converted to Unix ms");
    }
}
