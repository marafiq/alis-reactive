namespace Alis.Reactive.UnitTests;

[TestFixture]
public class WhenTriggeringOnSignalR : PlanTestBase
{
    [Test]
    public Task Hub_url_and_method_are_wired()
    {
        var plan = CreatePlan();
        Trigger(plan).SignalR("/hubs/notifications", "ReceiveUpdate", p =>
            p.Dispatch("update-received"));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Typed_payload_produces_source_binding()
    {
        var plan = CreatePlan();
        Trigger(plan).SignalR<TestModel>("/hubs/data", "ReceiveData", (payload, p) =>
            p.Element("value").SetText(payload, x => x.Id));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Multiple_methods_on_same_hub_produce_separate_entries()
    {
        var plan = CreatePlan();
        Trigger(plan)
            .SignalR("/hubs/notifications", "ReceiveAlert", p => p.Dispatch("alert"))
            .SignalR("/hubs/notifications", "ReceiveUpdate", p => p.Dispatch("update"));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Different_hubs_produce_entries_with_different_hub_urls()
    {
        var plan = CreatePlan();
        Trigger(plan)
            .SignalR("/hubs/notifications", "Receive", p => p.Dispatch("notif"))
            .SignalR("/hubs/live-data", "StatusChanged", p => p.Dispatch("status"));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
