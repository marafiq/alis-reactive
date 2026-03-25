namespace Alis.Reactive.FluentValidator.UnitTests;

/// <summary>
/// WhenField<DateTime> and WhenFieldNot<DateTime> serialize condition values as Unix ms
/// (DateTimeOffset.ToUnixTimeMilliseconds) so that TS domConditionReader can produce
/// matching Unix ms strings via Date.getTime() for eq/neq comparison.
/// </summary>
[TestFixture]
public class WhenExtractingDateTimeConditions
{
    private readonly FluentValidationAdapter _adapter = AdapterFactory.Create();

    [Test]
    public void WhenField_DateTime_eq_serializes_value_as_unix_ms()
    {
        var desc = _adapter.ExtractRules(typeof(DateTimeConditionValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var nameField = desc!.Fields.First(f => f.FieldName == "Name");
        Assert.That(nameField.Rules, Has.Count.EqualTo(1));
        Assert.That(nameField.Rules[0].When, Is.Not.Null);
        Assert.That(nameField.Rules[0].When!.Field, Is.EqualTo("AdmissionDate"));
        Assert.That(nameField.Rules[0].When.Op, Is.EqualTo("eq"));

        // Value must be Unix ms (long), NOT an ISO string
        var condValue = nameField.Rules[0].When.Value;
        Assert.That(condValue, Is.TypeOf<long>(),
            "DateTime condition value must be serialized as Unix ms (long)");

        // Verify the actual timestamp: 2026-07-01T00:00:00Z
        var expected = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();
        Assert.That(condValue, Is.EqualTo(expected));
    }

    [Test]
    public void WhenFieldNot_DateTime_neq_serializes_value_as_unix_ms()
    {
        var desc = _adapter.ExtractRules(typeof(DateTimeNeqConditionValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var scoreField = desc!.Fields.First(f => f.FieldName == "Score");
        Assert.That(scoreField.Rules[0].When, Is.Not.Null);
        Assert.That(scoreField.Rules[0].When!.Field, Is.EqualTo("AdmissionDate"));
        Assert.That(scoreField.Rules[0].When.Op, Is.EqualTo("neq"));

        var condValue = scoreField.Rules[0].When.Value;
        Assert.That(condValue, Is.TypeOf<long>(),
            "DateTime condition value must be serialized as Unix ms (long)");

        var expected = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();
        Assert.That(condValue, Is.EqualTo(expected));
    }

    [Test]
    public void WhenFieldNot_string_neq_keeps_string_value()
    {
        // Verify non-DateTime conditions are NOT affected by Unix ms serialization
        var desc = _adapter.ExtractRules(typeof(ReactiveNeqConditionValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var emailField = desc!.Fields.First(f => f.FieldName == "Email");
        var condition = emailField.Rules[0].When;
        Assert.That(condition, Is.Not.Null);
        Assert.That(condition!.Value, Is.EqualTo("Independent"),
            "String condition values must remain as strings, not converted to Unix ms");
    }
}
