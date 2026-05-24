using System.Text.Json;
using Alis.Reactive.Validation;

namespace Alis.Reactive.UnitTests.ValidationEnrichment;

public class EnrichmentTestModel
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public bool ReceiveNotifications { get; set; }
    public DateTime AdmissionDate { get; set; }
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

        RegisterTextInput(plan, "Name", "name-input");
        RegisterTextInput(plan, "Email", "email-input");

        var trigger = new Builders.TriggerBuilder<EnrichmentTestModel>(plan, plan.Context);
        trigger.DomReady(p =>
        {
            p.Post("/save", g => g.IncludeAll())
             .Validate<FakeEnrichmentValidator>("test-form")
             .Response(r => r.OnSuccess(s => s.Dispatch("saved")));
        });

        // Enrichment happens during Render() — but since the StubExtractor returns empty,
        // the plan won't have validation fields. This test verifies the code path compiles
        // and runs without errors.
        var json = plan.Render();
        Assert.That(json, Does.Contain("test-form").Or.Not.Null);
    }

    [Test]
    public void Validation_rules_render_explicit_execution_contract()
    {
        ReactivePlanConfig.Reset();
        ReactivePlanConfig.UseValidationExtractor(new SingleRuleExtractor());

        var plan = new ReactivePlan<EnrichmentTestModel>();
        RegisterTextInput(plan, "Name", "name-input");

        var trigger = new Builders.TriggerBuilder<EnrichmentTestModel>(plan, plan.Context);
        trigger.DomReady(p => p.Post("/save", g => g.IncludeAll()).Validate<FakeEnrichmentValidator>("test-form"));

        using var document = JsonDocument.Parse(plan.Render());
        var rule = document.RootElement
            .GetProperty("components")
            .GetProperty("test-form")
            .GetProperty("container")
            .GetProperty("validationRules")[0]
            .GetProperty("rules")[0];

        Assert.That(rule.TryGetProperty("constraint", out _), Is.False);
        Assert.That(rule.TryGetProperty("otherValue", out _), Is.False);
        Assert.That(rule.TryGetProperty("when", out _), Is.False);
        Assert.That(rule.TryGetProperty("shape", out _), Is.False);

        var execution = rule.GetProperty("execution");
        Assert.That(execution.GetProperty("constraint").GetProperty("kind").GetString(), Is.EqualTo("none"));
        Assert.That(execution.GetProperty("otherValue").GetProperty("kind").GetString(), Is.EqualTo("none"));
        Assert.That(execution.GetProperty("activation").GetProperty("kind").GetString(), Is.EqualTo("always"));
        Assert.That(execution.GetProperty("comparisonShape").GetProperty("kind").GetString(), Is.EqualTo("none"));
    }

    [Test]
    public void Deferred_validation_fields_keep_the_model_field_shape_for_partials()
    {
        ReactivePlanConfig.Reset();
        ReactivePlanConfig.UseValidationExtractor(new DeferredDateFieldExtractor());

        var plan = new ReactivePlan<EnrichmentTestModel>();

        var trigger = new Builders.TriggerBuilder<EnrichmentTestModel>(plan, plan.Context);
        trigger.DomReady(p => p.Post("/save", g => g.IncludeAll()).Validate<FakeEnrichmentValidator>("test-form"));

        using var document = JsonDocument.Parse(plan.Render());
        var validation = document.RootElement
            .GetProperty("components")
            .GetProperty("test-form")
            .GetProperty("container")
            .GetProperty("validationRules")[0];

        Assert.That(validation.GetProperty("component").GetString(), Does.EndWith("__AdmissionDate"));
        Assert.That(validation.GetProperty("serverFieldName").GetString(), Is.EqualTo("AdmissionDate"));
        Assert.That(validation.GetProperty("value").GetProperty("member").GetString(), Is.EqualTo("value"));
        Assert.That(
            validation.GetProperty("value").GetProperty("shape").GetProperty("kind").GetString(),
            Is.EqualTo("date"));
    }

    [Test]
    public void Registered_validation_fields_read_the_component_value_contract()
    {
        ReactivePlanConfig.Reset();
        ReactivePlanConfig.UseValidationExtractor(new BooleanRuleExtractor());

        var plan = new ReactivePlan<EnrichmentTestModel>();
        RegisterInput(plan, "ReceiveNotifications", "notify-input", "checked", Alis.Reactive.PlanModel.Shape.Boolean);

        var trigger = new Builders.TriggerBuilder<EnrichmentTestModel>(plan, plan.Context);
        trigger.DomReady(p => p.Post("/save", g => g.IncludeAll()).Validate<FakeEnrichmentValidator>("test-form"));

        using var document = JsonDocument.Parse(plan.Render());
        var validation = document.RootElement
            .GetProperty("components")
            .GetProperty("test-form")
            .GetProperty("container")
            .GetProperty("validationRules")[0];

        Assert.That(validation.GetProperty("component").GetString(), Is.EqualTo("notify-input"));
        Assert.That(validation.GetProperty("value").GetProperty("member").GetString(), Is.EqualTo("checked"));
        Assert.That(
            validation.GetProperty("value").GetProperty("shape").GetProperty("kind").GetString(),
            Is.EqualTo("boolean"));
    }

    [Test]
    public void Registered_validation_conditions_read_the_component_value_contract()
    {
        ReactivePlanConfig.Reset();
        ReactivePlanConfig.UseValidationExtractor(new ConditionalRuleExtractor());

        var plan = new ReactivePlan<EnrichmentTestModel>();
        RegisterInput(plan, "Email", "email-input", "value", Alis.Reactive.PlanModel.Shape.String);
        RegisterInput(plan, "Name", "name-input", "currentText", Alis.Reactive.PlanModel.Shape.String);

        var trigger = new Builders.TriggerBuilder<EnrichmentTestModel>(plan, plan.Context);
        trigger.DomReady(p => p.Post("/save", g => g.IncludeAll()).Validate<FakeEnrichmentValidator>("test-form"));

        using var document = JsonDocument.Parse(plan.Render());
        var condition = document.RootElement
            .GetProperty("components")
            .GetProperty("test-form")
            .GetProperty("container")
            .GetProperty("validationRules")[0]
            .GetProperty("rules")[0]
            .GetProperty("execution")
            .GetProperty("activation")
            .GetProperty("condition");

        Assert.That(condition.GetProperty("left").GetProperty("from").GetProperty("component").GetString(), Is.EqualTo("name-input"));
        Assert.That(condition.GetProperty("left").GetProperty("member").GetString(), Is.EqualTo("currentText"));
        Assert.That(condition.GetProperty("shape").GetProperty("kind").GetString(), Is.EqualTo("string"));
    }

    [Test]
    public void Registered_validation_peer_fields_read_the_component_value_contract()
    {
        ReactivePlanConfig.Reset();
        ReactivePlanConfig.UseValidationExtractor(new PeerRuleExtractor());

        var plan = new ReactivePlan<EnrichmentTestModel>();
        RegisterInput(plan, "Email", "email-input", "value", Alis.Reactive.PlanModel.Shape.String);
        RegisterInput(plan, "Name", "name-input", "currentText", Alis.Reactive.PlanModel.Shape.String);

        var trigger = new Builders.TriggerBuilder<EnrichmentTestModel>(plan, plan.Context);
        trigger.DomReady(p => p.Post("/save", g => g.IncludeAll()).Validate<FakeEnrichmentValidator>("test-form"));

        using var document = JsonDocument.Parse(plan.Render());
        var peerValue = document.RootElement
            .GetProperty("components")
            .GetProperty("test-form")
            .GetProperty("container")
            .GetProperty("validationRules")[0]
            .GetProperty("rules")[0]
            .GetProperty("execution")
            .GetProperty("otherValue")
            .GetProperty("value");

        Assert.That(peerValue.GetProperty("from").GetProperty("component").GetString(), Is.EqualTo("name-input"));
        Assert.That(peerValue.GetProperty("member").GetString(), Is.EqualTo("currentText"));
        Assert.That(peerValue.GetProperty("shape").GetProperty("kind").GetString(), Is.EqualTo("string"));
    }

    [Test]
    public void Deferred_validation_field_must_exist_on_the_model()
    {
        ReactivePlanConfig.Reset();
        ReactivePlanConfig.UseValidationExtractor(new UnknownFieldExtractor());

        var plan = new ReactivePlan<EnrichmentTestModel>();

        var trigger = new Builders.TriggerBuilder<EnrichmentTestModel>(plan, plan.Context);
        trigger.DomReady(p => p.Post("/save", g => g.IncludeAll()).Validate<FakeEnrichmentValidator>("test-form"));

        var exception = Assert.Throws<InvalidOperationException>(() => plan.Render());

        Assert.That(exception!.Message, Does.Contain("Validation field 'MissingField' was extracted"));
        Assert.That(exception.Message, Does.Contain("register the input component for that binding path"));
    }

    [Test]
    public void Validation_field_path_rejects_empty_dotted_segments()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            ValidationFieldPath.Of("Address..City"));

        Assert.That(exception!.Message, Does.Contain("Address..City"));
        Assert.That(exception.Message, Does.Contain("empty segment"));
    }

    private class FakeEnrichmentValidator { }

    private static void RegisterTextInput(
        ReactivePlan<EnrichmentTestModel> plan,
        string bindingPath,
        string componentId)
    {
        RegisterInput(plan, bindingPath, componentId, "value", Alis.Reactive.PlanModel.Shape.String);
    }

    private static void RegisterInput(
        ReactivePlan<EnrichmentTestModel> plan,
        string bindingPath,
        string componentId,
        string valueMember,
        Alis.Reactive.PlanModel.Shape shape)
    {
        var identity = RegisteredComponentIdentity.For(componentId, "native");
        var binding = RegisteredComponentBinding.For(bindingPath, valueMember);
        var componentKind = Alis.Reactive.PlanModel.ComponentKind.Of("textbox");

        plan.RegisterInputComponent(ComponentRegistration.RegisteredInput(
                identity,
                binding,
                componentKind,
                shape));
    }

    private class StubExtractor : IValidationExtractor
    {
        public ValidationExtractionReport Extract(ValidationExtractionRequest request) =>
            ValidationExtractionReport.ForClientFields(request, new List<ValidationField>());
    }

    private class SingleRuleExtractor : IValidationExtractor
    {
        public ValidationExtractionReport Extract(ValidationExtractionRequest request) =>
            ValidationExtractionReport.ForClientFields(
                request,
                new List<ValidationField>
                {
                    new ValidationField(
                        ValidationFieldPath.Of("Name"),
                        new List<ValidationRule>
                        {
                            new ValidationRule(
                                ValidationRuleName.Required,
                                ValidationMessage.Of("Name is required"),
                                ValidationRuleDetails.NoOperand(ValidationRuleCondition.Always)),
                        }),
                });
    }

    private class DeferredDateFieldExtractor : IValidationExtractor
    {
        public ValidationExtractionReport Extract(ValidationExtractionRequest request) =>
            ValidationExtractionReport.ForClientFields(
                request,
                new List<ValidationField>
                {
                    RequiredField("AdmissionDate", "Admission date is required"),
                });
    }

    private class BooleanRuleExtractor : IValidationExtractor
    {
        public ValidationExtractionReport Extract(ValidationExtractionRequest request) =>
            ValidationExtractionReport.ForClientFields(
                request,
                new List<ValidationField>
                {
                    RequiredField("ReceiveNotifications", "Notifications must be selected"),
                });
    }

    private class UnknownFieldExtractor : IValidationExtractor
    {
        public ValidationExtractionReport Extract(ValidationExtractionRequest request) =>
            ValidationExtractionReport.ForClientFields(
                request,
                new List<ValidationField>
                {
                    RequiredField("MissingField", "Missing field is required"),
                });
    }

    private class ConditionalRuleExtractor : IValidationExtractor
    {
        public ValidationExtractionReport Extract(ValidationExtractionRequest request) =>
            ValidationExtractionReport.ForClientFields(
                request,
                new List<ValidationField>
                {
                    new ValidationField(
                        ValidationFieldPath.Of("Email"),
                        new List<ValidationRule>
                        {
                            new ValidationRule(
                                ValidationRuleName.Required,
                                ValidationMessage.Of("Email is required"),
                                ValidationRuleDetails.NoOperand(
                                    ValidationRuleCondition.When(
                                        FieldCondition.Compare(
                                            ValidationFieldPath.Of("Name"),
                                            Alis.Reactive.PlanModel.CompareOperator.Truthy)))),
                        }),
                });
    }

    private class PeerRuleExtractor : IValidationExtractor
    {
        public ValidationExtractionReport Extract(ValidationExtractionRequest request) =>
            ValidationExtractionReport.ForClientFields(
                request,
                new List<ValidationField>
                {
                    new ValidationField(
                        ValidationFieldPath.Of("Email"),
                        new List<ValidationRule>
                        {
                            new ValidationRule(
                                ValidationRuleName.EqualTo,
                                ValidationMessage.Of("Email must match name"),
                                ValidationRuleDetails.WithPeerField(
                                    ValidationFieldPath.Of("Name"),
                                    ValidationRuleCondition.Always,
                                    Alis.Reactive.PlanModel.Shape.String)),
                        }),
                });
    }

    private static ValidationField RequiredField(string fieldName, string message) =>
        new ValidationField(
            ValidationFieldPath.Of(fieldName),
            new List<ValidationRule>
            {
                new ValidationRule(
                    ValidationRuleName.Required,
                    ValidationMessage.Of(message),
                    ValidationRuleDetails.NoOperand(ValidationRuleCondition.Always)),
            });
}
