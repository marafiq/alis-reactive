using System.Text.Json;
using Alis.Reactive.Builders;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenMutatingAFusionGrid : FusionTestBase
{
    [Test]
    public void Refresh_produces_call_reaction()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<FusionGrid>("residents-grid").Refresh());

        var json = plan.RenderFormatted();

        Assert.That(json, Does.Contain("\"call\""), "Must contain a call reaction");
        Assert.That(json, Does.Contain("\"refresh\""), "Must target the refresh method");
        Assert.That(json, Does.Contain("residents-grid"), "Must reference the grid ID");
    }

    [Test]
    public void SetDataSource_from_response_produces_set_reaction()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/data")
             .Response(r => r.OnSuccess<GridResponsePayload>((json, s) =>
             {
                 s.Component<FusionGrid>("residents-grid").SetDataSource(json);
             }));
        });

        var planJson = plan.RenderFormatted();

        Assert.That(planJson, Does.Contain("\"set\""), "Must contain a set reaction");
        Assert.That(planJson, Does.Contain("\"dataSource\""), "Must target dataSource property");
        Assert.That(planJson, Does.Contain("responseBody"), "Must read from responseBody");
    }

    [Test]
    public void SetDataSource_from_response_with_path_produces_set_reaction()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/data")
             .Response(r => r.OnSuccess<GridResponsePayload>((json, s) =>
             {
                 s.Component<FusionGrid>("residents-grid").SetDataSource(json, j => j.Result);
             }));
        });

        var planJson = plan.RenderFormatted();

        Assert.That(planJson, Does.Contain("\"set\""), "Must contain a set reaction");
        Assert.That(planJson, Does.Contain("\"dataSource\""), "Must target dataSource property");
        Assert.That(planJson, Does.Contain("\"result\""), "Must read from result path");
    }

    [Test]
    public void SetDataSource_from_event_produces_set_reaction()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<GridDataPayload>("data-loaded",
            (payload, p) =>
                p.Component<FusionGrid>("residents-grid").SetDataSource(payload, x => x.Items));

        var planJson = plan.RenderFormatted();

        Assert.That(planJson, Does.Contain("\"set\""), "Must contain a set reaction");
        Assert.That(planJson, Does.Contain("\"dataSource\""), "Must target dataSource property");
        Assert.That(planJson, Does.Contain("\"items\""), "Must read from items path");
    }

    [Test]
    public void Refresh_followed_by_element_mutation_produces_sequential_reactions()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Component<FusionGrid>("residents-grid").Refresh();
            p.Element("status").SetText("refreshed");
        });

        var planJson = plan.RenderFormatted();

        Assert.That(planJson, Does.Contain("\"call\""), "Must contain call for Refresh");
        Assert.That(planJson, Does.Contain("\"refresh\""), "Must target refresh method");
        Assert.That(planJson, Does.Contain("\"set\""), "Must contain set for SetText");
        Assert.That(planJson, Does.Contain("refreshed"), "Must contain the text value");
    }
}

public class GridDataPayload
{
    public object? Items { get; set; }
}

public class GridResponsePayload
{
    public object? Result { get; set; }
    public int Count { get; set; }
}
