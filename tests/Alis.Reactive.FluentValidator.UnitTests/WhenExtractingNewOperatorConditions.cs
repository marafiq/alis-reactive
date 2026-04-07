using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenExtractingNewOperatorConditions
{
    private readonly FluentValidationAdapter _adapter = AdapterFactory.Create();

    // ── Ordering operators ─────────────────────────────────────────────────

    [Test]
    public void WhenFieldGt_extracts_gt_condition()
    {
        var fields = _adapter.ExtractRules(typeof(WhenFieldGtValidator), "form");
        var jobTitle = fields.First(f => f.FieldName == "JobTitle");
        var when = (FieldCompare)jobTitle.Rules[0].When!;

        Assert.That(when.Field, Is.EqualTo("Age"));
        Assert.That(when.Op, Is.EqualTo("gt"));
        Assert.That(when.Value, Is.EqualTo(18));
    }

    [Test]
    public void WhenFieldGte_extracts_gte_condition_with_decimal()
    {
        var fields = _adapter.ExtractRules(typeof(WhenFieldGteValidator), "form");
        var email = fields.First(f => f.FieldName == "Email");
        var when = (FieldCompare)email.Rules[0].When!;

        Assert.That(when.Field, Is.EqualTo("Salary"));
        Assert.That(when.Op, Is.EqualTo("gte"));
        Assert.That(when.Value, Is.EqualTo(50000m));
    }

    [Test]
    public void WhenFieldLt_extracts_lt_condition()
    {
        var fields = _adapter.ExtractRules(typeof(WhenFieldLtValidator), "form");
        var name = fields.First(f => f.FieldName == "Name");
        var when = (FieldCompare)name.Rules[0].When!;

        Assert.That(when.Field, Is.EqualTo("Age"));
        Assert.That(when.Op, Is.EqualTo("lt"));
        Assert.That(when.Value, Is.EqualTo(18));
    }

    [Test]
    public void WhenFieldLte_extracts_lte_condition_with_decimal()
    {
        var fields = _adapter.ExtractRules(typeof(WhenFieldLteValidator), "form");
        var notes = fields.First(f => f.FieldName == "Notes");
        var when = (FieldCompare)notes.Rules[0].When!;

        Assert.That(when.Field, Is.EqualTo("Salary"));
        Assert.That(when.Op, Is.EqualTo("lte"));
        Assert.That(when.Value, Is.EqualTo(0m));
    }

    // ── Presence operators ─────────────────────────────────────────────────

    [Test]
    public void WhenFieldNull_extracts_is_null_condition()
    {
        var fields = _adapter.ExtractRules(typeof(WhenFieldNullValidator), "form");
        var notes = fields.First(f => f.FieldName == "Notes");
        var when = (FieldCompare)notes.Rules[0].When!;

        Assert.That(when.Field, Is.EqualTo("MiddleName"));
        Assert.That(when.Op, Is.EqualTo("is-null"));
        Assert.That(when.Value, Is.Null);
    }

    [Test]
    public void WhenFieldNotNull_extracts_not_null_condition()
    {
        var fields = _adapter.ExtractRules(typeof(WhenFieldNotNullValidator), "form");
        var name = fields.First(f => f.FieldName == "Name");
        var when = (FieldCompare)name.Rules[0].When!;

        Assert.That(when.Field, Is.EqualTo("MiddleName"));
        Assert.That(when.Op, Is.EqualTo("not-null"));
        Assert.That(when.Value, Is.Null);
    }

    [Test]
    public void WhenFieldEmpty_extracts_is_empty_condition()
    {
        var fields = _adapter.ExtractRules(typeof(WhenFieldEmptyValidator), "form");
        var phone = fields.First(f => f.FieldName == "Phone");
        var when = (FieldCompare)phone.Rules[0].When!;

        Assert.That(when.Field, Is.EqualTo("Email"));
        Assert.That(when.Op, Is.EqualTo("is-empty"));
        Assert.That(when.Value, Is.Null);
    }

    [Test]
    public void WhenFieldNotEmpty_extracts_not_empty_condition()
    {
        var fields = _adapter.ExtractRules(typeof(WhenFieldNotEmptyValidator), "form");
        var name = fields.First(f => f.FieldName == "Name");
        var when = (FieldCompare)name.Rules[0].When!;

        Assert.That(when.Field, Is.EqualTo("Notes"));
        Assert.That(when.Op, Is.EqualTo("not-empty"));
        Assert.That(when.Value, Is.Null);
    }

    // ── Membership operators ───────────────────────────────────────────────

    [Test]
    public void WhenFieldIn_extracts_in_condition_with_array_value()
    {
        var fields = _adapter.ExtractRules(typeof(WhenFieldInValidator), "form");
        var notes = fields.First(f => f.FieldName == "Notes");
        var when = (FieldCompare)notes.Rules[0].When!;

        Assert.That(when.Field, Is.EqualTo("CareLevel"));
        Assert.That(when.Op, Is.EqualTo("in"));
        Assert.That(when.Value, Is.InstanceOf<object[]>());
        var values = (object[])when.Value!;
        Assert.That(values, Is.EqualTo(new object[] { "memory-care", "skilled-nursing" }));
    }

    [Test]
    public void WhenFieldNotIn_extracts_not_in_condition_with_array_value()
    {
        var fields = _adapter.ExtractRules(typeof(WhenFieldNotInValidator), "form");
        var phone = fields.First(f => f.FieldName == "Phone");
        var when = (FieldCompare)phone.Rules[0].When!;

        Assert.That(when.Field, Is.EqualTo("CareLevel"));
        Assert.That(when.Op, Is.EqualTo("not-in"));
        Assert.That(when.Value, Is.InstanceOf<object[]>());
        var values = (object[])when.Value!;
        Assert.That(values, Is.EqualTo(new object[] { "independent", "assisted" }));
    }

    [Test]
    public void WhenFieldBetween_extracts_between_condition_with_range()
    {
        var fields = _adapter.ExtractRules(typeof(WhenFieldBetweenValidator), "form");
        var jobTitle = fields.First(f => f.FieldName == "JobTitle");
        var when = (FieldCompare)jobTitle.Rules[0].When!;

        Assert.That(when.Field, Is.EqualTo("Age"));
        Assert.That(when.Op, Is.EqualTo("between"));
        Assert.That(when.Value, Is.InstanceOf<object[]>());
        var range = (object[])when.Value!;
        Assert.That(range[0], Is.EqualTo(18));
        Assert.That(range[1], Is.EqualTo(65));
    }

    // ── Text operators ─────────────────────────────────────────────────────

    [Test]
    public void WhenFieldContains_extracts_contains_condition()
    {
        var fields = _adapter.ExtractRules(typeof(WhenFieldContainsValidator), "form");
        var phone = fields.First(f => f.FieldName == "Phone");
        var when = (FieldCompare)phone.Rules[0].When!;

        Assert.That(when.Field, Is.EqualTo("Notes"));
        Assert.That(when.Op, Is.EqualTo("contains"));
        Assert.That(when.Value, Is.EqualTo("urgent"));
    }

    [Test]
    public void WhenFieldStartsWith_extracts_starts_with_condition()
    {
        var fields = _adapter.ExtractRules(typeof(WhenFieldStartsWithValidator), "form");
        var email = fields.First(f => f.FieldName == "Email");
        var when = (FieldCompare)email.Rules[0].When!;

        Assert.That(when.Field, Is.EqualTo("Name"));
        Assert.That(when.Op, Is.EqualTo("starts-with"));
        Assert.That(when.Value, Is.EqualTo("Dr."));
    }

    [Test]
    public void WhenFieldEndsWith_extracts_ends_with_condition()
    {
        var fields = _adapter.ExtractRules(typeof(WhenFieldEndsWithValidator), "form");
        var jobTitle = fields.First(f => f.FieldName == "JobTitle");
        var when = (FieldCompare)jobTitle.Rules[0].When!;

        Assert.That(when.Field, Is.EqualTo("Email"));
        Assert.That(when.Op, Is.EqualTo("ends-with"));
        Assert.That(when.Value, Is.EqualTo("@hospital.org"));
    }

    // ── Condition source fields included ───────────────────────────────────

    [Test]
    public void Condition_source_fields_are_included_for_all_operators()
    {
        // Every WhenField* condition field must appear in the extracted fields
        // so the runtime can read its value.
        void AssertSourceField(Type validatorType, string expectedSourceField)
        {
            var fields = _adapter.ExtractRules(validatorType, "form");
            var fieldNames = fields.Select(f => f.FieldName).ToList();
            Assert.That(fieldNames, Does.Contain(expectedSourceField),
                $"{validatorType.Name}: condition source field '{expectedSourceField}' missing");
        }

        AssertSourceField(typeof(WhenFieldGtValidator), "Age");
        AssertSourceField(typeof(WhenFieldGteValidator), "Salary");
        AssertSourceField(typeof(WhenFieldLtValidator), "Age");
        AssertSourceField(typeof(WhenFieldLteValidator), "Salary");
        AssertSourceField(typeof(WhenFieldNullValidator), "MiddleName");
        AssertSourceField(typeof(WhenFieldNotNullValidator), "MiddleName");
        AssertSourceField(typeof(WhenFieldEmptyValidator), "Email");
        AssertSourceField(typeof(WhenFieldNotEmptyValidator), "Notes");
        AssertSourceField(typeof(WhenFieldInValidator), "CareLevel");
        AssertSourceField(typeof(WhenFieldNotInValidator), "CareLevel");
        AssertSourceField(typeof(WhenFieldBetweenValidator), "Age");
        AssertSourceField(typeof(WhenFieldContainsValidator), "Notes");
        AssertSourceField(typeof(WhenFieldStartsWithValidator), "Name");
        AssertSourceField(typeof(WhenFieldEndsWithValidator), "Email");
    }
}
