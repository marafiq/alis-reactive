namespace Alis.Reactive.UnitTests;

[TestFixture]
public class WhenTriggeringOnServerPush : PlanTestBase
{
    [Test]
    public Task Url_is_wired()
    {
        var plan = CreatePlan();
        Trigger(plan).ServerPush("/api/stream", p => p.Dispatch("update"));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Event_type_filters_named_events()
    {
        var plan = CreatePlan();
        Trigger(plan).ServerPush("/api/stream", "notification", p =>
            p.Element("msg").SetText("received"));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Typed_payload_produces_source_binding()
    {
        var plan = CreatePlan();
        Trigger(plan).ServerPush<TestModel>("/api/stream", "update", (payload, p) =>
            p.Element("name").SetText(payload, x => x.Id));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Multiple_triggers_on_same_url_produce_separate_entries()
    {
        var plan = CreatePlan();
        Trigger(plan)
            .ServerPush("/api/stream", "event-a", p => p.Dispatch("a"))
            .ServerPush("/api/stream", "event-b", p => p.Dispatch("b"));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
