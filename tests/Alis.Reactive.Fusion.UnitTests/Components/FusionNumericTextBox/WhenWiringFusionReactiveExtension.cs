using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

/// <summary>
/// Tests the .Reactive() wiring path end-to-end for FusionNumericTextBox.
/// Uses TriggerBuilder to verify plan serialization + schema conformance.
/// </summary>
[TestFixture]
public class WhenWiringFusionReactiveExtension : FusionTestBase
{
    [Test]
    public Task Component_event_trigger_produces_valid_plan()
    {
        var plan = CreatePlan();

        Trigger(plan).CustomEvent<FusionNumericTextBoxChangeArgs>("amount-changed", (args, p) =>
            p.Element("echo").SetText("Amount changed"));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Component_event_trigger_with_condition()
    {
        var plan = CreatePlan();

        Trigger(plan).CustomEvent<FusionNumericTextBoxChangeArgs>("amount-guarded", (args, p) =>
            p.When(args, x => x.Value).Gte(100m)
                .Then(then => then.Component<FusionNumericTextBox>(m => m.Amount).SetValue(100))
                .Else(else_ => else_.Element("echo").SetText("Under limit")));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Component_event_trigger_with_cross_vendor_mutations()
    {
        var plan = CreatePlan();

        Trigger(plan).CustomEvent<FusionNumericTextBoxChangeArgs>("amount-reset", (args, p) =>
        {
            p.Component<FusionNumericTextBox>(m => m.Amount).SetValue(0);
            p.Element("echo").SetText("Reset");
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
