using Alis.Reactive;
using Alis.Reactive.Fusion.Components;
using static VerifyNUnit.Verifier;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenReactingToFusionDropDownListEvents : FusionTestBase
{
    [Test]
    public Task Changed_event_wires_component_trigger()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<FusionDropDownListChangeArgs>("status-changed", (args, p) =>
            p.Component<FusionDropDownList>(m => m.Status).SetValue("US"));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Changed_event_with_condition()
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
    public Task Changed_event_with_cross_vendor_actions()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<FusionDropDownListChangeArgs>("cross-vendor", (args, p) =>
        {
            p.Component<FusionDropDownList>(m => m.Status).SetValue(null);
            p.Element("status").SetText("Reset via DropDown event");
        });
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
