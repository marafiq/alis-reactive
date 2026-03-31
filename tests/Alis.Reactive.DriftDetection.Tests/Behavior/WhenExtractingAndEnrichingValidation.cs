using System.Text.Json;
using Alis.Reactive.DriftDetection.Tests.Infrastructure;
using Alis.Reactive.DriftDetection.Tests.Validation;
using Alis.Reactive.FluentValidator;
using Alis.Reactive.Native.Components;
using Alis.Reactive.Native.Extensions;
using FluentValidation;

namespace Alis.Reactive.DriftDetection.Tests.Behavior;

[TestFixture]
public class WhenExtractingAndEnrichingValidation : DriftTestBase
{
    private static bool _extractorRegistered;

    [OneTimeSetUp]
    public void RegisterExtractor()
    {
        if (_extractorRegistered)
            return;

        ReactivePlanConfig.UseValidationExtractor(new FluentValidationAdapter(type =>
        {
            if (type == typeof(TestValidator))
                return new TestValidator();
            return null;
        }));

        _extractorRegistered = true;
    }

    [Test]
    public void resident_submission_validation_is_extracted_and_stamped_during_render()
    {
        AssertDefinitionPropertiesExactly("ValidationDescriptor", "formId", "planId", "fields");

        var json = RenderValidatedPlan();
        AssertSchemaValid(json);

        var validation = GetValidationDescriptor(json);
        AssertElementPropertiesExactly(validation, "formId", "planId", "fields");
        Assert.That(validation.GetProperty("planId").GetString(), Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void resident_validation_fields_are_enriched_from_registered_components()
    {
        AssertDefinitionPropertiesExactly("ValidationField",
            "fieldName", "rules", "fieldId", "vendor", "readExpr", "coerceAs");

        var json = RenderValidatedPlan();
        AssertSchemaValid(json);

        var nameField = GetValidationField(json, "Name");
        AssertElementPropertiesExactly(nameField,
            "fieldName", "rules", "fieldId", "vendor", "readExpr", "coerceAs");
    }

    [Test]
    public void resident_validation_rules_cover_constraint_when_and_cross_field_shapes()
    {
        AssertDefinitionPropertiesExactly("ValidationRule",
            "rule", "message", "constraint", "field", "coerceAs", "when");

        var json = RenderValidatedPlan();
        AssertSchemaValid(json);

        var monthlyRateRule = GetValidationRule(json, "MonthlyRate", "gt");
        var veteranRule = GetValidationRule(json, "VeteranId", "required");
        var physicianMatchRule = GetValidationRule(json, "PhysicianName", "equalTo");

        AssertElementPropertiesExactly(monthlyRateRule, "rule", "message", "constraint", "coerceAs");
        AssertElementPropertiesExactly(veteranRule, "rule", "message", "when");
        AssertElementPropertiesExactly(physicianMatchRule, "rule", "message", "field");
        Assert.That(physicianMatchRule.GetProperty("field").GetString(), Is.EqualTo("Name"));
    }

    [Test]
    public void resident_validation_conditions_cover_truthy_and_eq_variants()
    {
        AssertDefinitionPropertiesExactly("ValidationCondition", "field", "op", "value");

        var json = RenderValidatedPlan();
        AssertSchemaValid(json);

        var veteranWhen = GetValidationRule(json, "VeteranId", "required").GetProperty("when");
        var physicianRequiredWhen = GetValidationRule(json, "PhysicianName", "required").GetProperty("when");

        AssertElementPropertiesExactly(veteranWhen, "field", "op");
        Assert.That(veteranWhen.GetProperty("field").GetString(), Is.EqualTo("IsVeteran"));
        Assert.That(veteranWhen.GetProperty("op").GetString(), Is.EqualTo("truthy"));

        AssertElementPropertiesExactly(physicianRequiredWhen, "field", "op", "value");
        Assert.That(physicianRequiredWhen.GetProperty("field").GetString(), Is.EqualTo("CareLevel"));
        Assert.That(physicianRequiredWhen.GetProperty("op").GetString(), Is.EqualTo("eq"));
        Assert.That(physicianRequiredWhen.GetProperty("value").GetString(), Is.EqualTo("Memory Care"));
    }

    private static string RenderValidatedPlan()
    {
        var plan = CreatePlan();

        Html.InputField(plan, m => m.Name)
            .NativeTextBox(b => b.Placeholder("Resident name"));

        On(plan, t => t.DomReady(p =>
            p.Post("/api/residents", g => g.IncludeAll())
             .Validate<TestValidator>("resident-form")));

        return plan.Render();
    }

    private static JsonElement GetValidationDescriptor(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement
            .GetProperty("entries")[0]
            .GetProperty("reaction")
            .GetProperty("request")
            .GetProperty("validation")
            .Clone();
    }

    private static JsonElement GetValidationField(string json, string fieldName)
    {
        using var doc = JsonDocument.Parse(json);
        foreach (var field in doc.RootElement
                     .GetProperty("entries")[0]
                     .GetProperty("reaction")
                     .GetProperty("request")
                     .GetProperty("validation")
                     .GetProperty("fields")
                     .EnumerateArray())
        {
            if (field.GetProperty("fieldName").GetString() == fieldName)
                return field.Clone();
        }

        throw new AssertionException($"Validation field '{fieldName}' was not found.");
    }

    private static JsonElement GetValidationRule(string json, string fieldName, string ruleName)
    {
        var field = GetValidationField(json, fieldName);
        foreach (var rule in field.GetProperty("rules").EnumerateArray())
        {
            if (rule.GetProperty("rule").GetString() == ruleName)
                return rule.Clone();
        }

        throw new AssertionException(
            $"Validation rule '{ruleName}' was not found for field '{fieldName}'.");
    }

    private static void AssertElementPropertiesExactly(JsonElement element, params string[] expectedProperties)
    {
        var actual = element.EnumerateObject().Select(x => x.Name).OrderBy(x => x).ToList();
        var expected = expectedProperties.OrderBy(x => x).ToList();

        Assert.That(actual, Is.EqualTo(expected),
            $"JSON properties drifted. Expected: [{string.Join(", ", expected)}]. " +
            $"Actual: [{string.Join(", ", actual)}].");
    }
}
