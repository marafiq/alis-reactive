namespace Alis.Reactive.PlaywrightTests.Validation.Projection;

[TestFixture]
public sealed class WhenFluentValidationAdapterProjectsClientRules
{
    [Test]
    public void async_rules_are_server_only_even_when_a_client_projection_is_declared()
    {
        using var doc = ValidationProjectionPlanHarness
            .RenderPlan<AsyncOnlyModel, AsyncOnlyValidator>();

        Assert.That(ValidationRuleCount(doc.RootElement), Is.Zero);
    }

    [Test]
    public void custom_rules_project_only_through_the_typed_client_bridge()
    {
        using var doc = ValidationProjectionPlanHarness
            .RenderPlan<ConfirmEmailModel, ConfirmEmailValidator>();
        var rule = ValidationProjectionPlanHarness
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
    public void reactive_whenfield_projects_client_activation_and_declares_guard_fields()
    {
        using var doc = ValidationProjectionPlanHarness
            .RenderPlan<AssessmentModel, ReactiveAssessmentValidator>();
        var rule = ValidationProjectionPlanHarness
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
    public void rules_under_regular_fluentvalidation_conditions_stay_server_only()
    {
        using var doc = ValidationProjectionPlanHarness
            .RenderPlan<AssessmentModel, MixedConditionValidator>();

        Assert.That(ValidationRuleCount(doc.RootElement), Is.Zero);
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
