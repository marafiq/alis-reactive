using System.Text.Json;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;
using static Alis.Reactive.ModuleTests.TriggersAndPipelines.CarePlans;

namespace Alis.Reactive.ModuleTests.TriggersAndPipelines;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAnAppEventIsDispatched
{
    private const string OnLoad = "ready";

    [Test]
    public void a_dispatch_with_no_payload_calls_the_documents_dispatch_with_the_events_name_and_null()
    {
        var html = Views.Of(new CareCall("North"));
        var plan = html.ReactivePlan<CareCall>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Dispatch(CareEvents.RoundStarted)));
        var json = JsonOf(html.RenderPlan(plan));
        var expected = Plan(Document, Handler(OnLoad, "[]", "overlap", Dispatches("roundStarted", """{"kind":"lit","value":null}""")));
        Assert.That(json, Is.EqualTo(expected));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_payload_the_view_writes_is_a_literal_in_the_live_form_under_the_apps_naming()
    {
        var html = Views.Of(new CareCall("North"), json => json.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        var plan = html.ReactivePlan<CareCall>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
            pipeline.Dispatch(CareEvents.Flagged, new ResidentFlagged { Room = 12, Reason = "fall", WardName = "East" })));
        var json = JsonOf(html.RenderPlan(plan));
        var expected = Plan(Document, Handler(OnLoad, "[]", "overlap",
            Dispatches("flagged", """{"kind":"lit","value":{"room":12,"reason":"fall","ward_name":"East"}}""")));
        Assert.That(json, Is.EqualTo(expected));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_payload_built_from_sources_is_read_when_the_step_runs_beside_its_literal_members()
    {
        var html = Views.Of(new CareCall("North"));
        var plan = html.ReactivePlan<CareCall>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
            pipeline.Dispatch(CareEvents.Flagged, pipeline.Object<ResidentFlagged>(flagged => flagged
                .Set(x => x.Room, pipeline.Plugin<CareDesk>().Room)
                .Set(x => x.Reason, "fall")))));
        var json = JsonOf(html.RenderPlan(plan));
        var expected = Plan(Document + "," + Desk, Handler(OnLoad, "[]", "overlap",
            Dispatches("flagged",
                "{\"kind\":\"record\",\"fields\":[{\"name\":\"room\",\"value\":{\"kind\":\"get\",\"path\":[\"room\"],\"members\":\"required\",\"target\":" + TheDesk + "}}," +
                """{"name":"reason","value":{"kind":"lit","value":"fall"}}]}""")));
        Assert.That(json, Is.EqualTo(expected));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_handler_passes_on_the_payload_it_heard_as_it_holds_it()
    {
        var html = Views.Of(new CareCall("North"));
        var plan = html.ReactivePlan<CareCall>();
        html.On(plan, trigger => trigger.CustomEvent(CareEvents.Flagged, (flagged, pipeline) => pipeline.Dispatch(CareEvents.Acknowledged, flagged)));
        var json = JsonOf(html.RenderPlan(plan));
        var expected = Plan(Document, Handler("flagged", """[0,"detail"]""", "overlap", Dispatches("acknowledged", Payload)));
        Assert.That(json, Is.EqualTo(expected));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }
}
