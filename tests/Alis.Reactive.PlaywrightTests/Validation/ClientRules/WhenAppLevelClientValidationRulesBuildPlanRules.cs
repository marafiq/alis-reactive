namespace Alis.Reactive.PlaywrightTests.Validation.ClientRules;

[TestFixture]
public sealed class WhenAppLevelClientValidationRulesBuildPlanRules
{
    [Test]
    public void peer_ordered_rules_are_expressed_as_peer_value_reads()
    {
        using var doc = ClientValidationRulePlanHarness
            .RenderPlan<StayWindow, StayWindowClientValidation>();
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
                "max"
            }));
            Assert.That(ClientValidationRulePlanHarness.PeerComponents(rules), Is.EqualTo(new[]
            {
                IdGenerator.For(typeof(StayWindow), nameof(StayWindow.AdmissionDate)),
                IdGenerator.For(typeof(StayWindow), nameof(StayWindow.AdmissionDate)),
                IdGenerator.For(typeof(StayWindow), nameof(StayWindow.ReviewDate)),
                IdGenerator.For(typeof(StayWindow), nameof(StayWindow.ReviewDate))
            }));
        });
    }

    [Test]
    public void conditions_declare_the_fields_their_rules_read()
    {
        using var doc = ClientValidationRulePlanHarness
            .RenderPlan<ResidentAssessment, ResidentAssessmentClientValidation>();
        var rule = ClientValidationRulePlanHarness
            .RulesFor(doc.RootElement, nameof(ResidentAssessment.MemoryAssessmentScore))
            .Single();
        var activation = rule.GetProperty("execution").GetProperty("activation");
        var condition = activation.GetProperty("condition");

        Assert.Multiple(() =>
        {
            Assert.That(activation.GetProperty("kind").GetString(), Is.EqualTo("when"));
            Assert.That(condition.GetProperty("kind").GetString(), Is.EqualTo("all"));
            Assert.That(ClientValidationRulePlanHarness.ConditionLeftComponents(condition), Is.EqualTo(new[]
            {
                IdGenerator.For(typeof(ResidentAssessment), nameof(ResidentAssessment.IsVeteran)),
                IdGenerator.For(typeof(ResidentAssessment), nameof(ResidentAssessment.CareLevel))
            }));
        });
    }
}
