using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

/// <summary>
/// Covers the PreventDefault extension on each args class that SF honors the <c>cancel</c> flag on.
/// </summary>
[TestFixture]
public class WhenHandlingArgsExtensions : FusionTestBase
{
    [Test]
    public void BeginEdit_PreventDefault_emits_cancel_true_on_event_source()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<FusionInPlaceEditorBeginEditArgs>("beginEdit", (args, p) =>
        {
            args.PreventDefault(p);
        });

        var json = plan.RenderFormatted();
        AssertSchemaValid(json);

        Assert.That(json, Does.Contain("\"set\""));
        Assert.That(json, Does.Contain("\"cancel\""));
        Assert.That(json, Does.Contain("true"));
    }

    [Test]
    public void EndEdit_PreventDefault_emits_cancel_true_on_event_source()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<FusionInPlaceEditorEndEditArgs>("endEdit", (args, p) =>
        {
            args.PreventDefault(p);
        });

        var json = plan.RenderFormatted();
        AssertSchemaValid(json);

        Assert.That(json, Does.Contain("\"set\""));
        Assert.That(json, Does.Contain("\"cancel\""));
        Assert.That(json, Does.Contain("true"));
    }

    [Test]
    public void ActionBegin_PreventDefault_emits_cancel_true_on_event_source()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<FusionInPlaceEditorActionBeginArgs>("actionBegin", (args, p) =>
        {
            args.PreventDefault(p);
        });

        var json = plan.RenderFormatted();
        AssertSchemaValid(json);

        Assert.That(json, Does.Contain("\"set\""));
        Assert.That(json, Does.Contain("\"cancel\""));
        Assert.That(json, Does.Contain("true"));
    }

}
