using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests;

/// <summary>
/// Tests the .Reactive() wiring path end-to-end.
/// NativeCheckList.Reactive() creates ONE entry targeting the hidden input's change event.
/// checklist.ts syncs checkbox values into the hidden input and dispatches change.
/// </summary>
[TestFixture]
public class WhenWiringNativeCheckListReactiveExtension : NativeTestBase
{
    [Test]
    public Task Component_event_trigger_produces_valid_plan()
    {
        var plan = CreatePlan();

        Trigger(plan).CustomEvent<NativeCheckListChangeArgs>("allergies-changed", (args, p) =>
            p.Element("echo").SetText("Allergies changed"));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Component_event_trigger_with_condition()
    {
        var plan = CreatePlan();

        Trigger(plan).CustomEvent<NativeCheckListChangeArgs>("allergies-guarded", (args, p) =>
            p.When(args, x => x.Value).NotEmpty()
                .Then(then => then.Element("panel").Show())
                .Else(else_ => else_.Element("panel").Hide()));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Single_entry_regardless_of_option_count()
    {
        var plan = CreatePlan();

        // Even with many options, only 1 entry on the hidden input
        Trigger(plan).CustomEvent<NativeCheckListChangeArgs>("option-toggled", (args, p) =>
            p.Element("echo").SetText("Option toggled"));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
