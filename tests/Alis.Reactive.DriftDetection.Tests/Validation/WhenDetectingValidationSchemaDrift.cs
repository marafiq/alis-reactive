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
    [OneTimeSetUp]
    public void RegisterExtractor()
    {
        // Register the FluentValidation adapter so Validate<T>() extracts rules at Render()
        try
        {
            ReactivePlanConfig.UseValidationExtractor(
                new FluentValidationAdapter(type =>
                {
                    if (type == typeof(TestValidator))
                        return new TestValidator();
                    return null;
                }));
        }
        catch (InvalidOperationException)
        {
            // Already registered (parallel test execution)
        }
    }

    [Test]
    public void validation_descriptor_conforms()
    {
        // ValidationDescriptor: formId, planId, fields
        // planId is set at Render() time from plan.PlanId
        var plan = CreatePlan();

        // Register component so validation field enrichment works
        RegisterNameComponent(plan);

        On(plan, t => t.DomReady(p =>
            p.Post("/api/residents", g => g.IncludeAll())
             .Validate<TestValidator>("resident-form")));

        var json = plan.Render();
        AssertSchemaValid(json);
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
            }
        }

        Assert.That(foundEnrichedField, Is.True,
            "Name field should be enriched with fieldId, vendor, readExpr from ComponentsMap");
    }

    [Test]
    public void validation_rule_with_all_properties_conforms()
    {
        // ValidationRule: rule, message, constraint, field, coerceAs, when
        // The TestValidator has conditional rules (WhenField) which produce 'when'
        // and numeric rules which produce 'constraint' and 'coerceAs'
        var plan = CreatePlan();
        RegisterNameComponent(plan);

        On(plan, t => t.DomReady(p =>
            p.Post("/api/residents", g => g.IncludeAll())
             .Validate<TestValidator>("resident-form")));

        var json = plan.Render();
        AssertSchemaValid(json);
    }

    [Test]
    public void validation_condition_with_value_conforms()
    {
        // ValidationCondition: field, op, value
        // WhenField(x => x.IsVeteran, ...) produces a condition with op="truthy"
        // WhenField<TProp>(x => x.CareLevel, "Memory Care", ...) would produce op="eq" + value
        var plan = CreatePlan();
        RegisterNameComponent(plan);

        On(plan, t => t.DomReady(p =>
            p.Post("/api/residents", g => g.IncludeAll())
             .Validate<TestValidator>("resident-form")));

        var json = plan.Render();
        AssertSchemaValid(json);

        // Verify at least one rule has a 'when' condition
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var fields = doc.RootElement
            .GetProperty("entries")[0]
            .GetProperty("reaction")
            .GetProperty("request")
            .GetProperty("validation")
            .GetProperty("fields");

        bool foundConditionalRule = false;
        for (int i = 0; i < fields.GetArrayLength(); i++)
        {
            var rules = fields[i].GetProperty("rules");
            for (int j = 0; j < rules.GetArrayLength(); j++)
            {
                if (rules[j].TryGetProperty("when", out _))
                    foundConditionalRule = true;
            }
        }

        Assert.That(foundConditionalRule, Is.True,
            "At least one validation rule should have a 'when' condition from WhenField()");
    }

    /// <summary>
    /// Registers a NativeTextBox component for the Name field so validation enrichment works.
    /// Uses InputField + NativeTextBox which adds to ComponentsMap before HTML render.
    /// </summary>
    private static void RegisterNameComponent(ReactivePlan<ResidentModel> plan)
    {
        try
        {
            Html.InputField(plan, m => m.Name)
                .NativeTextBox(b => b.Placeholder("Resident name"));
        }
        catch (NotImplementedException)
        {
            // TestHtmlHelper.TextBoxFor — component already registered in ComponentsMap
        }
    }
}
