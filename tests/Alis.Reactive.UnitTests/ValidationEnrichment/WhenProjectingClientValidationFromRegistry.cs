using System.Text.Json;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive.UnitTests.ValidationEnrichment;

public class RegistryValidationModel
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? ConfirmEmail { get; set; }
    public bool ReceiveNotifications { get; set; }
    public DateTime AdmissionDate { get; set; }
}

[TestFixture]
public class WhenProjectingClientValidationFromRegistry
{
    [TearDown]
    public void TearDown()
    {
        ReactivePlanConfig.Reset();
    }

    [Test]
    public void Registry_projects_a_typed_required_field_into_the_validation_container()
    {
        ReactivePlanConfig.UseClientValidationProjectionSource(
            ClientValidationProjectionRegistry.Create(registry =>
                registry.For<RegistryValidator, RegistryValidationModel>(validation =>
                    validation.Field(model => model.Name).Required("Name is required"))));

        var plan = new ReactivePlan<RegistryValidationModel>();
        RegisterInput(plan, "Name", "name-input", "value", Shape.String);

        AddValidatedRequest(plan);

        var validation = ValidationRuleForField(plan.Render(), "Name");

        Assert.That(validation.GetProperty("component").GetString(), Is.EqualTo("name-input"));
        Assert.That(validation.GetProperty("serverFieldName").GetString(), Is.EqualTo("Name"));
        Assert.That(validation.GetProperty("rules")[0].GetProperty("name").GetString(), Is.EqualTo("required"));
        Assert.That(validation.GetProperty("rules")[0].GetProperty("message").GetString(), Is.EqualTo("Name is required"));
    }

    [Test]
    public void Registry_deferred_fields_use_the_token_shape_projected_from_the_typed_field()
    {
        ReactivePlanConfig.UseClientValidationProjectionSource(
            ClientValidationProjectionRegistry.Create(registry =>
                registry.For<RegistryValidator, RegistryValidationModel>(validation =>
                    validation.Field(model => model.AdmissionDate).Required("Admission date is required"))));

        var plan = new ReactivePlan<RegistryValidationModel>();

        AddValidatedRequest(plan);

        var validation = ValidationRuleForField(plan.Render(), "AdmissionDate");

        Assert.That(validation.GetProperty("component").GetString(), Does.EndWith("__AdmissionDate"));
        Assert.That(
            validation.GetProperty("value").GetProperty("shape").GetProperty("kind").GetString(),
            Is.EqualTo("date"));
    }

    [Test]
    public void Registry_peer_rules_bind_other_value_through_registered_component_contracts()
    {
        ReactivePlanConfig.UseClientValidationProjectionSource(
            ClientValidationProjectionRegistry.Create(registry =>
                registry.For<RegistryValidator, RegistryValidationModel>(validation =>
                    validation
                        .Field(model => model.ConfirmEmail)
                        .EqualTo(model => model.Email, "Confirm email must match email"))));

        var plan = new ReactivePlan<RegistryValidationModel>();
        RegisterInput(plan, "ConfirmEmail", "confirm-email-input", "value", Shape.String);
        RegisterInput(plan, "Email", "email-input", "currentText", Shape.String);

        AddValidatedRequest(plan);

        var otherValue = ValidationRuleForField(plan.Render(), "ConfirmEmail")
            .GetProperty("rules")[0]
            .GetProperty("execution")
            .GetProperty("otherValue")
            .GetProperty("value");

        Assert.That(otherValue.GetProperty("from").GetProperty("component").GetString(), Is.EqualTo("email-input"));
        Assert.That(otherValue.GetProperty("member").GetString(), Is.EqualTo("currentText"));
        Assert.That(otherValue.GetProperty("shape").GetProperty("kind").GetString(), Is.EqualTo("string"));
    }

    [Test]
    public void Registry_conditions_bind_activation_through_registered_component_contracts()
    {
        ReactivePlanConfig.UseClientValidationProjectionSource(
            ClientValidationProjectionRegistry.Create(registry =>
                registry.For<RegistryValidator, RegistryValidationModel>(validation =>
                    validation.When(
                        condition => condition.Field(model => model.ReceiveNotifications).Truthy(),
                        then => then.Field(model => model.Email).Required("Email is required")))));

        var plan = new ReactivePlan<RegistryValidationModel>();
        RegisterInput(plan, "Email", "email-input", "value", Shape.String);
        RegisterInput(plan, "ReceiveNotifications", "notify-input", "checked", Shape.Boolean);

        AddValidatedRequest(plan);

        var condition = ValidationRuleForField(plan.Render(), "Email")
            .GetProperty("rules")[0]
            .GetProperty("execution")
            .GetProperty("activation")
            .GetProperty("condition");

        Assert.That(condition.GetProperty("left").GetProperty("from").GetProperty("component").GetString(), Is.EqualTo("notify-input"));
        Assert.That(condition.GetProperty("left").GetProperty("member").GetString(), Is.EqualTo("checked"));
        Assert.That(condition.GetProperty("shape").GetProperty("kind").GetString(), Is.EqualTo("boolean"));
    }

    [Test]
    public void Registry_fails_with_source_type_context_when_no_projection_is_registered()
    {
        ReactivePlanConfig.UseClientValidationProjectionSource(
            ClientValidationProjectionRegistry.Create(_ => { }));

        var plan = new ReactivePlan<RegistryValidationModel>();

        AddValidatedRequest(plan);

        var exception = Assert.Throws<InvalidOperationException>(() => plan.Render());

        Assert.That(exception!.Message, Does.Contain(typeof(RegistryValidator).FullName));
        Assert.That(exception.Message, Does.Contain("registry.For"));
    }

    private static void AddValidatedRequest(ReactivePlan<RegistryValidationModel> plan)
    {
        var trigger = new Builders.TriggerBuilder<RegistryValidationModel>(plan, plan.Context);
        trigger.DomReady(p => p.Post("/save", gather => gather.IncludeAll()).Validate<RegistryValidator>("test-form"));
    }

    private static JsonElement ValidationRuleForField(string renderedPlan, string fieldName)
    {
        using var document = JsonDocument.Parse(renderedPlan);
        var validations = document.RootElement
            .GetProperty("components")
            .GetProperty("test-form")
            .GetProperty("container")
            .GetProperty("validationRules");

        foreach (var validation in validations.EnumerateArray())
        {
            var serverFieldName = validation.GetProperty("serverFieldName").GetString();
            if (serverFieldName == fieldName)
                return validation.Clone();
        }

        throw new AssertionException($"Expected validation field '{fieldName}' to be projected.");
    }

    private static void RegisterInput(
        ReactivePlan<RegistryValidationModel> plan,
        string bindingPath,
        string componentId,
        string valueMember,
        Shape shape)
    {
        var identity = RegisteredComponentIdentity.For(componentId, "native");
        var binding = RegisteredComponentBinding.For(bindingPath, valueMember);
        var componentKind = ComponentKind.Of("textbox");

        plan.RegisterInputComponent(ComponentRegistration.RegisteredInput(
            identity,
            binding,
            componentKind,
            shape));
    }

    private sealed class RegistryValidator { }
}
