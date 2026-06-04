namespace Alis.Reactive.PlaywrightTests.Validation.ClientRules;

using Alis.Reactive.FluentValidator;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

[TestFixture]
public sealed class WhenReactiveValidatorsDeclareClientRules
{
    [Test]
    public void declared_client_rules_emit_reactive_plan_client_rules()
    {
        using var doc = ClientValidationRulePlanHarness
            .RenderPlan<BuiltInClientRulesModel, BuiltInClientRulesValidator>();

        Assert.Multiple(() =>
        {
            Assert.That(
                ClientValidationRulePlanHarness.RuleNames(
                    ClientValidationRulePlanHarness.RulesFor(doc.RootElement, nameof(BuiltInClientRulesModel.Name))),
                Is.EqualTo(new[] { "required", "minLength", "maxLength", "regex" }));
            Assert.That(
                ClientValidationRulePlanHarness.RuleNames(
                    ClientValidationRulePlanHarness.RulesFor(doc.RootElement, nameof(BuiltInClientRulesModel.Email))),
                Is.EqualTo(new[] { "email" }));
            Assert.That(
                ClientValidationRulePlanHarness.RuleNames(
                    ClientValidationRulePlanHarness.RulesFor(doc.RootElement, nameof(BuiltInClientRulesModel.Card))),
                Is.EqualTo(new[] { "creditCard" }));
            Assert.That(
                ClientValidationRulePlanHarness.RuleNames(
                    ClientValidationRulePlanHarness.RulesFor(doc.RootElement, nameof(BuiltInClientRulesModel.EmptyCode))),
                Is.EqualTo(new[] { "empty" }));
            Assert.That(
                ClientValidationRulePlanHarness.RuleNames(
                    ClientValidationRulePlanHarness.RulesFor(doc.RootElement, nameof(BuiltInClientRulesModel.Score))),
                Is.EqualTo(new[] { "range", "exclusiveRange", "min", "max", "gt", "lt", "equalTo", "notEqual" }));
        });
    }

    [Test]
    public void async_rules_are_server_only_even_when_a_client_rule_is_declared()
    {
        using var doc = ClientValidationRulePlanHarness
            .RenderPlan<AsyncOnlyModel, AsyncOnlyValidator>();

        Assert.That(ValidationRuleCount(doc.RootElement), Is.Zero);
    }

    [Test]
    public void peer_rules_emit_typed_peer_value_reads()
    {
        using var doc = ClientValidationRulePlanHarness
            .RenderPlan<ConfirmEmailModel, ConfirmEmailValidator>();
        var rule = ClientValidationRulePlanHarness
            .RulesFor(doc.RootElement, nameof(ConfirmEmailModel.ConfirmEmail))
            .Single();
        var execution = rule.GetProperty("execution");

        Assert.Multiple(() =>
        {
            Assert.That(rule.GetProperty("name").GetString(), Is.EqualTo("equalTo"));
            Assert.That(rule.GetProperty("message").GetString(), Is.EqualTo("Emails must match."));
            Assert.That(execution.GetProperty("kind").GetString(), Is.EqualTo("peer"));
            Assert.That(execution
                .GetProperty("value")
                .GetProperty("from")
                .GetProperty("component")
                .GetString(), Is.EqualTo(IdGenerator.For(typeof(ConfirmEmailModel), nameof(ConfirmEmailModel.Email))));
        });
    }

    [Test]
    public void peer_comparison_rules_emit_peer_value_reads()
    {
        using var doc = ClientValidationRulePlanHarness
            .RenderPlan<StayWindow, BuiltInPeerComparisonValidator>();
        var rules = ClientValidationRulePlanHarness.RulesFor(
            doc.RootElement,
            nameof(StayWindow.DischargeDate));

        Assert.Multiple(() =>
        {
            Assert.That(ClientValidationRulePlanHarness.RuleNames(rules), Is.EqualTo(new[]
            {
                "gt",
                "min",
                "lt",
                "max",
                "equalTo",
                "notEqualTo"
            }));
            Assert.That(ClientValidationRulePlanHarness.PeerComponents(rules), Is.EqualTo(new[]
            {
                IdGenerator.For(typeof(StayWindow), nameof(StayWindow.AdmissionDate)),
                IdGenerator.For(typeof(StayWindow), nameof(StayWindow.AdmissionDate)),
                IdGenerator.For(typeof(StayWindow), nameof(StayWindow.ReviewDate)),
                IdGenerator.For(typeof(StayWindow), nameof(StayWindow.ReviewDate)),
                IdGenerator.For(typeof(StayWindow), nameof(StayWindow.AdmissionDate)),
                IdGenerator.For(typeof(StayWindow), nameof(StayWindow.ReviewDate))
            }));
        });
    }

    [Test]
    public void whenfield_declares_client_activation_and_guard_fields()
    {
        using var doc = ClientValidationRulePlanHarness
            .RenderPlan<AssessmentModel, ReactiveAssessmentValidator>();
        var rule = ClientValidationRulePlanHarness
            .RulesFor(doc.RootElement, nameof(AssessmentModel.Score))
            .Single();
        var activation = rule.GetProperty("execution").GetProperty("activation");
        var condition = activation.GetProperty("condition");

        Assert.Multiple(() =>
        {
            Assert.That(activation.GetProperty("kind").GetString(), Is.EqualTo("when"));
            Assert.That(condition.GetProperty("kind").GetString(), Is.EqualTo("compare"));
            Assert.That(condition
                .GetProperty("left")
                .GetProperty("from")
                .GetProperty("component")
                .GetString(), Is.EqualTo(IdGenerator.For(typeof(AssessmentModel), nameof(AssessmentModel.IsVeteran))));
        });
    }

    [Test]
    public void whenfields_declares_composed_client_activation()
    {
        using var doc = ClientValidationRulePlanHarness
            .RenderPlan<AssessmentModel, ComposedAssessmentValidator>();
        var rule = ClientValidationRulePlanHarness
            .RulesFor(doc.RootElement, nameof(AssessmentModel.Notes))
            .Single();
        var condition = rule.GetProperty("execution")
            .GetProperty("activation")
            .GetProperty("condition");

        Assert.Multiple(() =>
        {
            Assert.That(condition.GetProperty("kind").GetString(), Is.EqualTo("all"));
            Assert.That(ClientValidationRulePlanHarness.ConditionLeftComponents(condition), Is.EqualTo(new[]
            {
                IdGenerator.For(typeof(AssessmentModel), nameof(AssessmentModel.IsVeteran)),
                IdGenerator.For(typeof(AssessmentModel), nameof(AssessmentModel.Score))
            }));
        });
    }

    [Test]
    public void whenfield_array_contains_declares_collection_item_activation()
    {
        using var doc = ClientValidationRulePlanHarness
            .RenderPlan<TaggedAssessmentModel, TaggedAssessmentValidator>();
        var rule = ClientValidationRulePlanHarness
            .RulesFor(doc.RootElement, nameof(TaggedAssessmentModel.ReviewNote))
            .Single();
        var condition = rule.GetProperty("execution")
            .GetProperty("activation")
            .GetProperty("condition");

        Assert.Multiple(() =>
        {
            Assert.That(condition.GetProperty("kind").GetString(), Is.EqualTo("compare"));
            Assert.That(condition.GetProperty("op").GetString(), Is.EqualTo("array-contains"));
            Assert.That(condition
                .GetProperty("left")
                .GetProperty("from")
                .GetProperty("component")
                .GetString(), Is.EqualTo(IdGenerator.For(typeof(TaggedAssessmentModel), nameof(TaggedAssessmentModel.Tags))));
            Assert.That(condition.GetProperty("shape").GetProperty("kind").GetString(), Is.EqualTo("array"));
            Assert.That(condition.GetProperty("itemShape").GetProperty("kind").GetString(), Is.EqualTo("string"));
            Assert.That(condition
                .GetProperty("right")
                .GetProperty("value")
                .GetProperty("value")
                .GetString(), Is.EqualTo("fall-risk"));
        });
    }

    [Test]
    public void rules_under_regular_fluentvalidation_conditions_stay_server_only()
    {
        using var doc = ClientValidationRulePlanHarness
            .RenderPlan<AssessmentModel, MixedConditionValidator>();

        Assert.That(ValidationRuleCount(doc.RootElement), Is.Zero);
    }

    [Test]
    public void fluent_validation_registrations_merge_across_service_modules()
    {
        var services = new ServiceCollection();
        services.AddReactiveFluentValidation(rules => rules.Add<BuiltInClientRulesValidator>());
        services.AddReactiveFluentValidation(rules => rules.Add<ConfirmEmailValidator>());

        using var provider = services.BuildServiceProvider();
        using var builtInRules = ClientValidationRulePlanHarness
            .RenderPlan<BuiltInClientRulesModel, BuiltInClientRulesValidator>(provider);
        using var confirmEmailRules = ClientValidationRulePlanHarness
            .RenderPlan<ConfirmEmailModel, ConfirmEmailValidator>(provider);

        Assert.Multiple(() =>
        {
            Assert.That(
                ClientValidationRulePlanHarness.RuleNames(
                    ClientValidationRulePlanHarness.RulesFor(
                        builtInRules.RootElement,
                        nameof(BuiltInClientRulesModel.Email))),
                Is.EqualTo(new[] { "email" }));
            Assert.That(
                ClientValidationRulePlanHarness.RuleNames(
                    ClientValidationRulePlanHarness.RulesFor(
                        confirmEmailRules.RootElement,
                        nameof(ConfirmEmailModel.ConfirmEmail))),
                Is.EqualTo(new[] { "equalTo" }));
        });
    }

    [Test]
    public void assembly_scan_registers_client_metadata_validators_and_leaves_server_validators_server_only()
    {
        AssemblyScanServerOnlyValidator.InstanceCount = 0;
        var services = new ServiceCollection();
        services.AddSingleton(new AssemblyScanMessages("Client name is required."));
        services.AddReactiveFluentValidation(rules =>
            rules.AddFromAssemblyContaining<AssemblyScanClientValidator>());

        using var provider = services.BuildServiceProvider();
        using var doc = ClientValidationRulePlanHarness
            .RenderPlan<AssemblyScanModel, AssemblyScanClientValidator>(provider);
        var rule = ClientValidationRulePlanHarness
            .RulesFor(doc.RootElement, nameof(AssemblyScanModel.Name))
            .Single();

        Assert.Multiple(() =>
        {
            Assert.That(rule.GetProperty("name").GetString(), Is.EqualTo("required"));
            Assert.That(rule.GetProperty("message").GetString(), Is.EqualTo("Client name is required."));
            Assert.That(AssemblyScanServerOnlyValidator.InstanceCount, Is.Zero);
        });

        _ = provider.GetRequiredService<IValidator<AssemblyScanServerOnlyModel>>();
        Assert.That(AssemblyScanServerOnlyValidator.InstanceCount, Is.EqualTo(1));
    }

    private static int ValidationRuleCount(System.Text.Json.JsonElement root)
    {
        return root
            .GetProperty("components")
            .GetProperty("validation-form")
            .GetProperty("container")
            .GetProperty("validationRules")
            .GetArrayLength();
    }
}

internal sealed class AssemblyScanMessages(string requiredNameMessage)
{
    internal string RequiredNameMessage { get; } = requiredNameMessage;
}

internal sealed class AssemblyScanModel
{
    public string? Name { get; set; }
}

internal sealed class AssemblyScanClientValidator : ReactiveValidator<AssemblyScanModel>
{
    public AssemblyScanClientValidator(AssemblyScanMessages messages)
    {
        ClientRule(model => model.Name)
            .Required(messages.RequiredNameMessage);
    }
}

internal sealed class AssemblyScanServerOnlyModel
{
    public string? ServerOnly { get; set; }
}

internal sealed class AssemblyScanServerOnlyValidator : AbstractValidator<AssemblyScanServerOnlyModel>
{
    internal static int InstanceCount { get; set; }

    public AssemblyScanServerOnlyValidator()
    {
        InstanceCount++;
        RuleFor(model => model.ServerOnly).NotEmpty();
    }
}
