using Alis.Reactive;
using Alis.Reactive.Fusion.Components;
using static VerifyNUnit.Verifier;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenReactingToFusionNumericTextBoxEvents : FusionTestBase
{
    [Test]
    public Task Changed_event_wires_component_trigger()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<FusionNumericTextBoxChangeArgs>("amount-changed", (args, p) =>
            p.Component<FusionNumericTextBox>(m => m.Amount).SetValue(999));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Changed_event_with_condition()
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
    public Task Changed_event_with_cross_vendor_actions()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<FusionNumericTextBoxChangeArgs>("cross-vendor", (args, p) =>
        {
            p.Component<FusionNumericTextBox>(m => m.Amount).SetValue(0);
            p.Element("status").SetText("Reset via Fusion event");
        });
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
