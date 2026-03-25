using System.Text.Json;
using Alis.Reactive.Validation;

namespace Alis.Reactive.UnitTests.ValidationEnrichment;

public class EnrichmentTestModel
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public EnrichmentAddress? Address { get; set; }
}

public class EnrichmentAddress
{
    public string? Street { get; set; }
    public string? City { get; set; }
}

[TestFixture]
public class WhenEnrichingValidationAtRenderTime
{
    [SetUp]
    public void SetUp()
    {
        ReactivePlanConfig.Reset();
        ReactivePlanConfig.UseValidationExtractor(new StubExtractor());
    }

    [TearDown]
    public void TearDown()
    {
        ReactivePlanConfig.Reset();
    }

    [Test]
    public void Fields_with_matching_components_are_enriched_in_csharp()
    {
        var plan = new ReactivePlan<EnrichmentTestModel>();

        plan.AddToComponentsMap("Name", new ComponentRegistration("name-input", "native", "Name", "value", "textbox", "string"));
        plan.AddToComponentsMap("Email", new ComponentRegistration("email-input", "native", "Email", "value", "textbox", "string"));

        var trigger = new Builders.TriggerBuilder<EnrichmentTestModel>(plan);
        trigger.DomReady(p =>
        {
            p.Post("/save", g => g.IncludeAll())
             .Validate(new ValidationDescriptor("test-form",
                 new System.Collections.Generic.List<ValidationField>
                 {
                     new("Name", new() { new("required", "Name required") }),
                     new("Email", new() { new("email", "Bad email") }),
                 }));
        });

        var json = plan.Render();
        using var doc = JsonDocument.Parse(json);
        var fields = doc.RootElement
            .GetProperty("entries")[0]
            .GetProperty("reaction")
            .GetProperty("request")
            .GetProperty("validation")
            .GetProperty("fields");

        var nameField = fields[0];
        Assert.That(nameField.GetProperty("fieldId").GetString(), Is.EqualTo("name-input"));
        Assert.That(nameField.GetProperty("vendor").GetString(), Is.EqualTo("native"));
        Assert.That(nameField.GetProperty("readExpr").GetString(), Is.EqualTo("value"));
    }

    [Test]
    public void Fields_without_components_remain_symbolic_for_ts_enrichment()
    {
        var plan = new ReactivePlan<EnrichmentTestModel>();
        // No components registered for Address.Street

        var trigger = new Builders.TriggerBuilder<EnrichmentTestModel>(plan);
        trigger.DomReady(p =>
        {
            p.Post("/save", g => g.IncludeAll())
             .Validate(new ValidationDescriptor("test-form",
                 new System.Collections.Generic.List<ValidationField>
                 {
                     new("Address.Street", new() { new("required", "Street required") }),
                 }));
        });

        var json = plan.Render();
        using var doc = JsonDocument.Parse(json);
        var field = doc.RootElement
            .GetProperty("entries")[0]
            .GetProperty("reaction")
            .GetProperty("request")
            .GetProperty("validation")
            .GetProperty("fields")[0];

        // Should NOT have enrichment properties (JsonIgnoreCondition.WhenWritingNull)
        Assert.That(field.TryGetProperty("fieldId", out _), Is.False);
        Assert.That(field.TryGetProperty("vendor", out _), Is.False);
        Assert.That(field.TryGetProperty("readExpr", out _), Is.False);
    }

    [Test]
    public void Enriched_fields_serialize_with_fieldId_vendor_readExpr()
    {
        var plan = new ReactivePlan<EnrichmentTestModel>();
        plan.AddToComponentsMap("Name", new ComponentRegistration("name-input", "fusion", "Name", "value", "autocomplete", "string"));

        var trigger = new Builders.TriggerBuilder<EnrichmentTestModel>(plan);
        trigger.DomReady(p =>
        {
            p.Post("/save", g => g.IncludeAll())
             .Validate(new ValidationDescriptor("form",
                 new System.Collections.Generic.List<ValidationField>
                 {
                     new("Name", new() { new("required", "Required") }),
                 }));
        });

        var json = plan.Render();

        Assert.That(json, Does.Contain("\"fieldId\":\"name-input\""));
        Assert.That(json, Does.Contain("\"vendor\":\"fusion\""));
        Assert.That(json, Does.Contain("\"readExpr\":\"value\""));
    }

    private class StubExtractor : IValidationExtractor
    {
        public ValidationDescriptor? ExtractRules(System.Type validatorType, string formId) => null;
    }
}
