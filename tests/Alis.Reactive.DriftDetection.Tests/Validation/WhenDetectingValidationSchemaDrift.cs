using Alis.Reactive.DriftDetection.Tests.Infrastructure;
using Alis.Reactive.FluentValidator;
using Alis.Reactive.Native.Components;
using Alis.Reactive.Native.Extensions;
using Alis.Reactive.Validation;
using FluentValidation;

namespace Alis.Reactive.DriftDetection.Tests.Validation;

[TestFixture]
public class WhenDetectingValidationSchemaDrift : DriftTestBase
{
    private static bool _extractorRegistered;

    [OneTimeSetUp]
    public void RegisterExtractor()
    {
        // Register the FluentValidation adapter so Validate<T>() extracts rules at Render()
        if (_extractorRegistered) return;

        ReactivePlanConfig.UseValidationExtractor(
            new FluentValidationAdapter(type =>
            {
                if (type == typeof(TestValidator))
                    return new TestValidator();
                return null;
            }));
        _extractorRegistered = true;
    }

    [Test]
    public void validation_descriptor_conforms()
    {
        // ValidationDescriptor: formId, planId, fields (all required)
        AssertDefinitionPropertiesExactly("ValidationDescriptor", "formId", "planId", "fields");

        var plan = CreatePlan();

        // Register component so validation field enrichment works
        RegisterNameComponent(plan);

        On(plan, t => t.DomReady(p =>
            p.Post("/api/residents", g => g.IncludeAll())
             .Validate<TestValidator>("resident-form")));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertAllPropertiesPresent(json, "ValidationDescriptor",
            "entries[0].reaction.request.validation");
    }

    [Test]
    public void validation_descriptor_with_planid_conforms()
    {
        // Verify planId is populated on the validation descriptor
        var plan = CreatePlan();
        RegisterNameComponent(plan);

        On(plan, t => t.DomReady(p =>
            p.Post("/api/residents", g => g.IncludeAll())
             .Validate<TestValidator>("resident-form")));

        var json = plan.Render();
        AssertSchemaValid(json);

        // Verify planId is present in the validation descriptor
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var validation = doc.RootElement
            .GetProperty("entries")[0]
            .GetProperty("reaction")
            .GetProperty("request")
            .GetProperty("validation");

        Assert.That(validation.TryGetProperty("planId", out var planIdProp), Is.True,
            "ValidationDescriptor should have planId populated at Render() time");
        Assert.That(planIdProp.GetString(), Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void validation_field_with_enrichment_conforms()
    {
        // ValidationField: fieldName, rules, fieldId, vendor, readExpr, coerceAs
        // Enrichment fills fieldId, vendor, readExpr, coerceAs from ComponentsMap
        AssertDefinitionPropertiesExactly("ValidationField",
            "fieldName", "rules", "fieldId", "vendor", "readExpr", "coerceAs");

        var plan = CreatePlan();
        RegisterNameComponent(plan);

        On(plan, t => t.DomReady(p =>
            p.Post("/api/residents", g => g.IncludeAll())
             .Validate<TestValidator>("resident-form")));

        var json = plan.Render();
        AssertSchemaValid(json);

        // Verify the Name field was enriched
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var fields = doc.RootElement
            .GetProperty("entries")[0]
            .GetProperty("reaction")
            .GetProperty("request")
            .GetProperty("validation")
            .GetProperty("fields");

        // Find the Name field
        bool foundEnrichedField = false;
        for (int i = 0; i < fields.GetArrayLength(); i++)
        {
            var field = fields[i];
            if (field.GetProperty("fieldName").GetString() == "Name" &&
                field.TryGetProperty("fieldId", out _))
            {
                foundEnrichedField = true;
                Assert.That(field.GetProperty("vendor").GetString(), Is.Not.Null);
                Assert.That(field.GetProperty("readExpr").GetString(), Is.Not.Null);
                Assert.That(field.GetProperty("coerceAs").GetString(), Is.Not.Null,
                    "coerceAs should be enriched from ComponentRegistration");
            }
        }

        Assert.That(foundEnrichedField, Is.True,
            "Name field should be enriched with fieldId, vendor, readExpr, coerceAs from ComponentsMap");
    }

    [Test]
    public void validation_rule_with_all_properties_conforms()
    {
        // ValidationRule: rule, message, constraint, field, coerceAs, when
        // The TestValidator has conditional rules (WhenField) which produce 'when'
        // and numeric rules which produce 'constraint' and 'coerceAs'
        AssertDefinitionPropertiesExactly("ValidationRule",
            "rule", "message", "constraint", "field", "coerceAs", "when");

        var plan = CreatePlan();
        RegisterNameComponent(plan);

        On(plan, t => t.DomReady(p =>
            p.Post("/api/residents", g => g.IncludeAll())
             .Validate<TestValidator>("resident-form")));

        var json = plan.Render();
        AssertSchemaValid(json);

        // ValidationRule has optional properties that appear on different rules:
        // - 'constraint' + 'coerceAs': on numeric rules (GreaterThan)
        // - 'when': on conditional rules (WhenField)
        // - 'field': on cross-property rules (not in current TestValidator)
        // Verify at least one rule has constraint+coerceAs, and one has 'when'.
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var fields = doc.RootElement
            .GetProperty("entries")[0]
            .GetProperty("reaction")
            .GetProperty("request")
            .GetProperty("validation")
            .GetProperty("fields");

        bool foundConstraint = false;
        bool foundWhen = false;
        for (int i = 0; i < fields.GetArrayLength(); i++)
        {
            var rules = fields[i].GetProperty("rules");
            for (int j = 0; j < rules.GetArrayLength(); j++)
            {
                if (rules[j].TryGetProperty("constraint", out _))
                    foundConstraint = true;
                if (rules[j].TryGetProperty("when", out _))
                    foundWhen = true;
            }
        }

        Assert.That(foundConstraint, Is.True,
            "At least one validation rule should have 'constraint' (from GreaterThan)");
        Assert.That(foundWhen, Is.True,
            "At least one validation rule should have 'when' condition (from WhenField)");
    }

    [Test]
    public void validation_condition_with_value_conforms()
    {
        // ValidationCondition: field, op, value
        // WhenField(x => x.IsVeteran, ...) produces op="truthy" (no value)
        // WhenField<string>(x => x.CareLevel, "Memory Care", ...) produces op="eq" + value="Memory Care"
        AssertDefinitionPropertiesExactly("ValidationCondition", "field", "op", "value");

        var plan = CreatePlan();
        RegisterNameComponent(plan);

        On(plan, t => t.DomReady(p =>
            p.Post("/api/residents", g => g.IncludeAll())
             .Validate<TestValidator>("resident-form")));

        var json = plan.Render();
        AssertSchemaValid(json);

        // Verify conditions: one "truthy" (IsVeteran) and one "eq" with value (CareLevel)
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var fields = doc.RootElement
            .GetProperty("entries")[0]
            .GetProperty("reaction")
            .GetProperty("request")
            .GetProperty("validation")
            .GetProperty("fields");

        bool foundTruthy = false;
        bool foundEqWithValue = false;
        for (int i = 0; i < fields.GetArrayLength(); i++)
        {
            var rules = fields[i].GetProperty("rules");
            for (int j = 0; j < rules.GetArrayLength(); j++)
            {
                if (rules[j].TryGetProperty("when", out var when))
                {
                    var op = when.GetProperty("op").GetString();
                    if (op == "truthy")
                        foundTruthy = true;
                    if (op == "eq" && when.TryGetProperty("value", out var val)
                                   && val.GetString() == "Memory Care")
                        foundEqWithValue = true;
                }
            }
        }

        Assert.That(foundTruthy, Is.True,
            "WhenField(x => x.IsVeteran, ...) should produce op='truthy'");
        Assert.That(foundEqWithValue, Is.True,
            "WhenField<string>(x => x.CareLevel, \"Memory Care\", ...) should produce op='eq' with value");
    }

    /// Registers a NativeTextBox component for the Name field so validation enrichment works.
    /// Uses InputField + NativeTextBox which adds to ComponentsMap before HTML render.
    private static void RegisterNameComponent(ReactivePlan<ResidentModel> plan)
    {
        Html.InputField(plan, m => m.Name)
            .NativeTextBox(b => b.Placeholder("Resident name"));
    }
}
