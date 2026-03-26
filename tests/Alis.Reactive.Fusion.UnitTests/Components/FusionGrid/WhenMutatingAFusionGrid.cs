using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenMutatingAFusionGrid : FusionTestBase
{
    [Test]
    public Task Refresh_produces_call_mutation()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<FusionGrid>("residents-grid").Refresh());
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task SetDataSource_from_event_produces_correct_plan()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<GridDataPayload>("data-loaded",
            (payload, p) =>
                p.Component<FusionGrid>("residents-grid").SetDataSource(payload, x => x.Items));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Refresh_followed_by_element_mutation()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Component<FusionGrid>("residents-grid").Refresh();
            p.Element("status").SetText("refreshed");
        });
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}

public class GridDataPayload
{
    public object? Items { get; set; }
}
