namespace Alis.Reactive.FluentValidator.UnitTests;

/// <summary>
/// Documents known extraction gaps with evidence.
///
/// KNOWN GAP 1 — IsInEnum: Cannot extract without reflection on the enum type.
/// Server validates correctly; client has no rule. ALIS004 analyzer warns at dev time.
///
/// KNOWN GAP 2 — TimeOnly/TimeSpan: InferCoerceAs returns null for time types.
/// Comparison rules extract but use string comparison instead of numeric/time.
///
/// Both gaps are documented in CLAUDE.md § "Validation Extraction — Design Rationale".
/// </summary>
[TestFixture]
public class WhenExtractingUnsupportedRules
{
    private readonly FluentValidationAdapter _adapter = AdapterFactory.Create();

    // ── Known Gap 1: IsInEnum — server-only, no reflection ──

    [Test]
    public void IsInEnum_extracts_zero_client_rules_because_enum_values_require_reflection()
    {
        // EnumValidator has: NotEmpty on Name + IsInEnum on CareLevel
        // IsInEnum is server-only — extracting enum values would require reflection.
        // ALIS004 analyzer warns the developer at compile time.
        var desc = _adapter.ExtractRules(typeof(EnumValidator), "testForm");
        Assert.That(desc, Is.Not.Null);

        // Name extracts fine
        var nameField = desc!.Fields.FirstOrDefault(f => f.FieldName == "Name");
        Assert.That(nameField, Is.Not.Null);
        Assert.That(nameField!.Rules[0].Rule, Is.EqualTo("required"));

        // CareLevel is absent — known gap, not a bug
        var careLevelField = desc.Fields.FirstOrDefault(f => f.FieldName == "CareLevel");
        Assert.That(careLevelField, Is.Null,
            "IsInEnum is server-only (no reflection) — field correctly absent from client extraction");
    }

    [Test]
    public void server_rejects_invalid_enum_that_client_cannot_catch()
    {
        var validator = new EnumValidator();
        var validResult = validator.Validate(new TestModel { Name = "Jane", CareLevel = CareLevel.Assisted });
        Assert.That(validResult.IsValid, Is.True);

        var invalidResult = validator.Validate(new TestModel { Name = "Jane", CareLevel = (CareLevel)999 });
        Assert.That(invalidResult.IsValid, Is.False, "Server catches invalid enum — client relies on server");
    }

    // ── Known Gap 2: TimeOnly coercion not supported ────────

    [Test]
    public void TimeOnly_comparison_extracts_with_null_coerceAs()
    {
        // ShiftEnd > ShiftStart — extracts as gt with field="ShiftStart"
        // coerceAs is null because InferCoerceAs doesn't handle TimeOnly.
        // Runtime falls back to string comparison — wrong for times like "9:30" vs "10:00".
        // Documented as known limitation in CLAUDE.md.
        var desc = _adapter.ExtractRules(typeof(TimeOnlyComparisonValidator), "testForm");
        Assert.That(desc, Is.Not.Null);

        var field = desc!.Fields.FirstOrDefault(f => f.FieldName == "ShiftEnd");
        Assert.That(field, Is.Not.Null, "ShiftEnd should be extracted");

        var rule = field!.Rules[0];
        Assert.That(rule.Rule, Is.EqualTo("gt"));
        Assert.That(rule.CoerceAs, Is.Null,
            "Known gap: TimeOnly has no coercion type — runtime uses string comparison");
    }

    // ── Verified: custom messages and cross-property work ────

    [Test]
    public void custom_message_with_fv_placeholder_uses_generic_fallback()
    {
        // .WithMessage("Name must be at least {MinLength} characters long.")
        // Adapter detects {MinLength} placeholder and falls back to generic message.
        // This is acceptable — the generic message still conveys the constraint.
        var desc = _adapter.ExtractRules(typeof(BraceInCustomMessageValidator), "testForm");
        Assert.That(desc, Is.Not.Null);

        var rule = desc!.Fields[0].Rules[0];
        Assert.That(rule.Message, Does.Contain("at least 3"),
            "Generic fallback message includes the actual constraint value");
    }

    [Test]
    public void nested_cross_property_extracts_field_name_correctly()
    {
        // WorkAddress.ZipCode has NotEqual(x => x.City) — same nesting level
        var desc = _adapter.ExtractRules(typeof(NestedCrossPropertyValidator), "testForm");
        Assert.That(desc, Is.Not.Null);

        var zipField = desc!.Fields.FirstOrDefault(f => f.FieldName == "WorkAddress.ZipCode");
        Assert.That(zipField, Is.Not.Null);

        var notEqualRule = zipField!.Rules.FirstOrDefault(r => r.Rule == "notEqualTo" || r.Rule == "notEqual");
        Assert.That(notEqualRule, Is.Not.Null);
        Assert.That(notEqualRule!.Field, Is.EqualTo("City"),
            "Same-level cross-property correctly uses property name");
    }
}
