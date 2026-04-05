using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

/// <summary>
/// Tests the .Reactive() wiring path end-to-end for FusionDropDownList.
/// Uses TriggerBuilder to verify plan serialization + schema conformance.
/// </summary>
[TestFixture]
public class WhenWiringFusionDropDownListReactiveExtension : FusionTestBase
{
    [Test]
    public Task Component_event_trigger_produces_valid_plan()
    {
        var plan = CreatePlan();

        Trigger(plan).CustomEvent<FusionDropDownListChangeArgs>("status-changed", (args, p) =>
            p.Element("echo").SetText("Status changed"));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Component_event_trigger_with_condition()
    {
        var plan = CreatePlan();

        Trigger(plan).CustomEvent<FusionDropDownListChangeArgs>("status-guarded", (args, p) =>
            p.When(args, x => x.Value).Eq("US")
                .Then(then => then.Component<FusionDropDownList>(m => m.Status).SetValue("US"))
                .Else(else_ => else_.Element("echo").SetText("Not US")));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Component_event_trigger_with_cross_vendor_mutations()
    {
        var plan = CreatePlan();

        Trigger(plan).CustomEvent<FusionDropDownListChangeArgs>("status-reset", (args, p) =>
        {
            p.Component<FusionDropDownList>(m => m.Status).SetValue(null);
            p.Element("echo").SetText("Reset");
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
