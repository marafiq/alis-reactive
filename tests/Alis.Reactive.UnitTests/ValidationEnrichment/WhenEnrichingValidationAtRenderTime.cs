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
        ReactivePlanConfig.UseFormValidationExtractor(new StubExtractor());
    }

    [TearDown]
    public void TearDown()
    {
        ReactivePlanConfig.Reset();
    }

    [Test]
    public void Validation_fields_reference_registered_bindings_in_csharp()
    {
        var plan = new ReactivePlan<EnrichmentTestModel>();

        plan.RegisterComponent("Name", new ComponentRegistration("name-input", "native", "Name", "value", "textbox", "string"));
        plan.RegisterComponent("Email", new ComponentRegistration("email-input", "native", "Email", "value", "textbox", "string"));

        var trigger = new Builders.TriggerBuilder<EnrichmentTestModel>(plan);
        trigger.DomReady(p =>
        {
            p.Post("/save", g => g.IncludeAll())
             .Validate(new FormValidation("test-form",
                 new System.Collections.Generic.List<ValidationField>
                 {
                     new("Name", new() { new("required", "Name required") }),
                     new("Email", new() { new("email", "Bad email") }),
                 }));
        });

        var json = plan.Render();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var fields = root
            .GetProperty("workflows")[0]
            .GetProperty("run")
            .GetProperty("request")
            .GetProperty("validation")
            .GetProperty("fields");
        var bindings = root.GetProperty("bindings");

        var nameField = fields[0];
        Assert.That(nameField.GetProperty("binding").GetString(), Is.EqualTo("Name"));

        var nameBinding = bindings.GetProperty("Name");
        Assert.That(nameBinding.GetProperty("object").GetString(), Is.EqualTo("component::name-input"));
        Assert.That(nameBinding.GetProperty("valueMember").GetString(), Is.EqualTo("value"));
    }

    [Test]
    public void Validation_fields_without_registered_components_remain_symbolic()
    {
        var plan = new ReactivePlan<EnrichmentTestModel>();
        // No components registered for Address.Street

        var trigger = new Builders.TriggerBuilder<EnrichmentTestModel>(plan);
        trigger.DomReady(p =>
        {
            p.Post("/save", g => g.IncludeAll())
             .Validate(new FormValidation("test-form",
                 new System.Collections.Generic.List<ValidationField>
                 {
                     new("Address.Street", new() { new("required", "Street required") }),
                 }));
        });

        var json = plan.Render();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var field = root
            .GetProperty("workflows")[0]
            .GetProperty("run")
            .GetProperty("request")
            .GetProperty("validation")
            .GetProperty("fields")[0];

        Assert.That(field.GetProperty("binding").GetString(), Is.EqualTo("Address.Street"));
        Assert.That(root.GetProperty("bindings").TryGetProperty("Address.Street", out _), Is.False);
    }

    [Test]
    public void Registered_components_serialize_through_bindings_not_field_enrichment()
    {
        var plan = new ReactivePlan<EnrichmentTestModel>();
        plan.RegisterComponent("Name", new ComponentRegistration("name-input", "fusion", "Name", "value", "autocomplete", "string"));

        var trigger = new Builders.TriggerBuilder<EnrichmentTestModel>(plan);
        trigger.DomReady(p =>
        {
            p.Post("/save", g => g.IncludeAll())
             .Validate(new FormValidation("form",
                 new System.Collections.Generic.List<ValidationField>
                 {
                     new("Name", new() { new("required", "Required") }),
                 }));
        });

        using var doc = JsonDocument.Parse(plan.Render());
        var root = doc.RootElement;
        var binding = root.GetProperty("bindings").GetProperty("Name");
        var validationField = root.GetProperty("workflows")[0]
            .GetProperty("run")
            .GetProperty("request")
            .GetProperty("validation")
            .GetProperty("fields")[0];

        Assert.That(binding.GetProperty("object").GetString(), Is.EqualTo("component::name-input"));
        Assert.That(binding.GetProperty("valueMember").GetString(), Is.EqualTo("value"));
        Assert.That(validationField.GetProperty("binding").GetString(), Is.EqualTo("Name"));
        Assert.That(validationField.TryGetProperty("fieldId", out _), Is.False);
    }

    private class StubExtractor : IFormValidationExtractor
    {
        public FormValidation? ExtractRules(System.Type validatorType, string formId) => null;
    }
}
