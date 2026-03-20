namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenExtractingFullCoverageRules
{
    private readonly FluentValidationAdapter _adapter = AdapterFactory.Create();

    // ── required ─────────────────────────────────────────────

    [Test]
    public void Name_required_extracts_correctly()
    {
        var desc = _adapter.ExtractRules(typeof(FullCoverageValidator), "testForm")!;
        var rule = desc.Fields.First(f => f.FieldName == "Name").Rules.First(r => r.Rule == "required");
        Assert.That(rule.Constraint, Is.Null);
        Assert.That(rule.Field, Is.Null);
        Assert.That(rule.CoerceAs, Is.Null);
    }

    // ── empty ────────────────────────────────────────────────

    [Test]
    public void Nickname_empty_extracts_correctly()
    {
        var desc = _adapter.ExtractRules(typeof(FullCoverageValidator), "testForm")!;
        var rule = desc.Fields.First(f => f.FieldName == "Nickname").Rules.First(r => r.Rule == "empty");
        Assert.That(rule.Constraint, Is.Null);
    }

    // ── minLength / maxLength ────────────────────────────────

    [Test]
    public void Name_minLength_extracts_with_constraint()
    {
        var desc = _adapter.ExtractRules(typeof(FullCoverageValidator), "testForm")!;
        var rule = desc.Fields.First(f => f.FieldName == "Name").Rules.First(r => r.Rule == "minLength");
        Assert.That(rule.Constraint, Is.EqualTo(3));
    }

    [Test]
    public void Name_maxLength_extracts_with_constraint()
    {
        var desc = _adapter.ExtractRules(typeof(FullCoverageValidator), "testForm")!;
        var rule = desc.Fields.First(f => f.FieldName == "Name").Rules.First(r => r.Rule == "maxLength");
        Assert.That(rule.Constraint, Is.EqualTo(100));
    }

    // ── email ────────────────────────────────────────────────

    [Test]
    public void Email_extracts_email_rule()
    {
        var desc = _adapter.ExtractRules(typeof(FullCoverageValidator), "testForm")!;
        var rule = desc.Fields.First(f => f.FieldName == "Email").Rules.First(r => r.Rule == "email");
        Assert.That(rule.Constraint, Is.Null);
    }

    // ── regex ────────────────────────────────────────────────

    [Test]
    public void Phone_extracts_regex_with_pattern()
    {
        var desc = _adapter.ExtractRules(typeof(FullCoverageValidator), "testForm")!;
        var rule = desc.Fields.First(f => f.FieldName == "Phone").Rules.First(r => r.Rule == "regex");
        Assert.That(rule.Constraint, Is.EqualTo(@"^\d{3}-\d{3}-\d{4}$"));
    }

    // ── creditCard ───────────────────────────────────────────

    [Test]
    public void CreditCardNumber_extracts_creditCard_rule()
    {
        var desc = _adapter.ExtractRules(typeof(FullCoverageValidator), "testForm")!;
        var rule = desc.Fields.First(f => f.FieldName == "CreditCardNumber").Rules.First(r => r.Rule == "creditCard");
        Assert.That(rule.Constraint, Is.Null);
    }

    // ── range (inclusive) with coerceAs ───────────────────────

    [Test]
    public void Age_range_extracts_with_number_coercion()
    {
        var desc = _adapter.ExtractRules(typeof(FullCoverageValidator), "testForm")!;
        var rule = desc.Fields.First(f => f.FieldName == "Age").Rules.First(r => r.Rule == "range");
        var constraint = rule.Constraint as object[];
        Assert.That(constraint, Is.Not.Null);
        Assert.That(constraint![0], Is.EqualTo(0));
        Assert.That(constraint[1], Is.EqualTo(120));
        Assert.That(rule.CoerceAs, Is.EqualTo("number"));
    }

    // ── exclusiveRange with coerceAs ─────────────────────────

    [Test]
    public void Score_exclusiveRange_extracts_with_number_coercion()
    {
        var desc = _adapter.ExtractRules(typeof(FullCoverageValidator), "testForm")!;
        var rule = desc.Fields.First(f => f.FieldName == "Score").Rules.First(r => r.Rule == "exclusiveRange");
        var constraint = rule.Constraint as object[];
        Assert.That(constraint, Is.Not.Null);
        Assert.That(constraint![0], Is.EqualTo(0m));
        Assert.That(constraint[1], Is.EqualTo(100m));
        Assert.That(rule.CoerceAs, Is.EqualTo("number"));
    }

    // ── min / max with coerceAs: "number" ────────────────────

    [Test]
    public void Salary_min_extracts_with_number_coercion()
    {
        var desc = _adapter.ExtractRules(typeof(FullCoverageValidator), "testForm")!;
        var rule = desc.Fields.First(f => f.FieldName == "Salary").Rules.First(r => r.Rule == "min");
        Assert.That(rule.Constraint, Is.EqualTo(0m));
        Assert.That(rule.CoerceAs, Is.EqualTo("number"));
        Assert.That(rule.Field, Is.Null);
    }

    [Test]
    public void Salary_max_extracts_with_number_coercion()
    {
        var desc = _adapter.ExtractRules(typeof(FullCoverageValidator), "testForm")!;
        var rule = desc.Fields.First(f => f.FieldName == "Salary").Rules.First(r => r.Rule == "max");
        Assert.That(rule.Constraint, Is.EqualTo(500000m));
        Assert.That(rule.CoerceAs, Is.EqualTo("number"));
    }

    // ── gt / lt with coerceAs: "number" ──────────────────────

    [Test]
    public void MonthlyRate_gt_extracts_with_number_coercion()
    {
        var desc = _adapter.ExtractRules(typeof(FullCoverageValidator), "testForm")!;
        var rule = desc.Fields.First(f => f.FieldName == "MonthlyRate").Rules.First(r => r.Rule == "gt");
        Assert.That(rule.Constraint, Is.EqualTo(0m));
        Assert.That(rule.CoerceAs, Is.EqualTo("number"));
    }

    [Test]
    public void MonthlyRate_lt_extracts_with_number_coercion()
    {
        var desc = _adapter.ExtractRules(typeof(FullCoverageValidator), "testForm")!;
        var rule = desc.Fields.First(f => f.FieldName == "MonthlyRate").Rules.First(r => r.Rule == "lt");
        Assert.That(rule.Constraint, Is.EqualTo(1000000m));
        Assert.That(rule.CoerceAs, Is.EqualTo("number"));
    }

    // ── equalTo (cross-property via field) ───────────────────

    [Test]
    public void ConfirmEmail_equalTo_extracts_with_field()
    {
        var desc = _adapter.ExtractRules(typeof(FullCoverageValidator), "testForm")!;
        var rule = desc.Fields.First(f => f.FieldName == "ConfirmEmail").Rules.First(r => r.Rule == "equalTo");
        Assert.That(rule.Field, Is.EqualTo("Email"));
        Assert.That(rule.Constraint, Is.Null);
    }

    // ── notEqualTo (cross-property via field) ────────────────

    [Test]
    public void AlternateEmail_notEqualTo_extracts_with_field()
    {
        var desc = _adapter.ExtractRules(typeof(FullCoverageValidator), "testForm")!;
        var rule = desc.Fields.First(f => f.FieldName == "AlternateEmail").Rules.First(r => r.Rule == "notEqualTo");
        Assert.That(rule.Field, Is.EqualTo("Email"));
        Assert.That(rule.Constraint, Is.Null);
    }

    // ── notEqual (fixed value) ───────────────────────────────

    [Test]
    public void Status_notEqual_extracts_with_constraint()
    {
        var desc = _adapter.ExtractRules(typeof(FullCoverageValidator), "testForm")!;
        var rule = desc.Fields.First(f => f.FieldName == "Status").Rules.First(r => r.Rule == "notEqual");
        Assert.That(rule.Constraint, Is.EqualTo("deleted"));
        Assert.That(rule.Field, Is.Null);
    }

    // ── min with coerceAs: "date" ────────────────────────────

    [Test]
    public void AdmissionDate_min_extracts_with_date_coercion_and_ISO_format()
    {
        var desc = _adapter.ExtractRules(typeof(FullCoverageValidator), "testForm")!;
        var rule = desc.Fields.First(f => f.FieldName == "AdmissionDate").Rules.First(r => r.Rule == "min");
        Assert.That(rule.Constraint, Is.EqualTo("2020-01-01"));
        Assert.That(rule.CoerceAs, Is.EqualTo("date"));
        Assert.That(rule.Field, Is.Null);
    }

    // ── gt cross-property with coerceAs: "date" ─────────────

    [Test]
    public void DischargeDate_gt_extracts_with_field_and_date_coercion()
    {
        var desc = _adapter.ExtractRules(typeof(FullCoverageValidator), "testForm")!;
        var rule = desc.Fields.First(f => f.FieldName == "DischargeDate").Rules.First(r => r.Rule == "gt");
        Assert.That(rule.Field, Is.EqualTo("AdmissionDate"));
        Assert.That(rule.Constraint, Is.Null);
        Assert.That(rule.CoerceAs, Is.EqualTo("date"));
    }

    // ── All fields present ───────────────────────────────────

    [Test]
    public void All_18_fields_are_extracted()
    {
        var desc = _adapter.ExtractRules(typeof(FullCoverageValidator), "testForm")!;
        var fieldNames = desc.Fields.Select(f => f.FieldName).ToList();
        Assert.That(fieldNames, Does.Contain("Name"));
        Assert.That(fieldNames, Does.Contain("Email"));
        Assert.That(fieldNames, Does.Contain("Phone"));
        Assert.That(fieldNames, Does.Contain("CreditCardNumber"));
        Assert.That(fieldNames, Does.Contain("Age"));
        Assert.That(fieldNames, Does.Contain("Score"));
        Assert.That(fieldNames, Does.Contain("Salary"));
        Assert.That(fieldNames, Does.Contain("MonthlyRate"));
        Assert.That(fieldNames, Does.Contain("ConfirmEmail"));
        Assert.That(fieldNames, Does.Contain("AlternateEmail"));
        Assert.That(fieldNames, Does.Contain("Status"));
        Assert.That(fieldNames, Does.Contain("AdmissionDate"));
        Assert.That(fieldNames, Does.Contain("DischargeDate"));
        Assert.That(fieldNames, Does.Contain("Nickname"));
    }

    // ── Conditional parity: same rules under WhenField ───────

    [Test]
    public void All_rules_extract_identically_under_WhenField_condition()
    {
        var unconditional = _adapter.ExtractRules(typeof(FullCoverageValidator), "testForm")!;
        var conditional = _adapter.ExtractRules(typeof(FullCoverageConditionalValidator), "testForm")!;

        // Same fields exist (plus IsEmployed condition source)
        var unconditionalFields = unconditional.Fields.Select(f => f.FieldName).OrderBy(x => x).ToList();
        var conditionalFields = conditional.Fields.Select(f => f.FieldName).Where(f => f != "IsEmployed").OrderBy(x => x).ToList();
        Assert.That(conditionalFields, Is.EqualTo(unconditionalFields));

        // Every rule in conditional has the IsEmployed condition
        foreach (var field in conditional.Fields.Where(f => f.FieldName != "IsEmployed"))
        {
            foreach (var rule in field.Rules)
            {
                Assert.That(rule.When, Is.Not.Null, $"Rule '{rule.Rule}' on '{field.FieldName}' missing condition");
                Assert.That(rule.When!.Field, Is.EqualTo("IsEmployed"), $"Rule '{rule.Rule}' on '{field.FieldName}' wrong condition field");
                Assert.That(rule.When.Op, Is.EqualTo("truthy"), $"Rule '{rule.Rule}' on '{field.FieldName}' wrong condition op");
            }
        }

        // Same rule types, constraints, fields, and coerceAs values
        foreach (var uField in unconditional.Fields)
        {
            var cField = conditional.Fields.First(f => f.FieldName == uField.FieldName);
            Assert.That(cField.Rules.Count, Is.EqualTo(uField.Rules.Count),
                $"'{uField.FieldName}' rule count mismatch");
            for (int i = 0; i < uField.Rules.Count; i++)
            {
                Assert.That(cField.Rules[i].Rule, Is.EqualTo(uField.Rules[i].Rule),
                    $"'{uField.FieldName}'[{i}] rule type mismatch");
                Assert.That(cField.Rules[i].CoerceAs, Is.EqualTo(uField.Rules[i].CoerceAs),
                    $"'{uField.FieldName}'[{i}] coerceAs mismatch");
                Assert.That(cField.Rules[i].Field, Is.EqualTo(uField.Rules[i].Field),
                    $"'{uField.FieldName}'[{i}] field mismatch");
                Assert.That(cField.Rules[i].Constraint?.ToString(), Is.EqualTo(uField.Rules[i].Constraint?.ToString()),
                    $"'{uField.FieldName}'[{i}] constraint mismatch");
            }
        }
    }
}
