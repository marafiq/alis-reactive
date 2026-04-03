using System.Text.Json;
using Alis.Reactive;
using Alis.Reactive.DriftDetection.Tests.Infrastructure;
using Alis.Reactive.Native.Components;
using Alis.Reactive.Native.Extensions;

namespace Alis.Reactive.DriftDetection.Tests.Behavior;

[TestFixture]
public class WhenRenderingResidentPlanV2 : DriftTestBase
{
    [Test]
    public void resident_boot_workflows_render_dom_ready_and_document_event_subscriptions()
    {
        var plan = CreatePlan();
        On(plan, t => t
            .DomReady(p => p.Dispatch("resident-booted"))
            .CustomEvent("resident-refresh", p => p.Element("status").SetText("refreshed")));

        var json = plan.Render();
        AssertSchemaValid(json);

        using var doc = JsonDocument.Parse(json);
        var workflows = doc.RootElement.GetProperty("workflows");

        Assert.That(workflows.GetArrayLength(), Is.EqualTo(2));
        Assert.That(workflows[0].GetProperty("when").GetProperty("kind").GetString(), Is.EqualTo("dom-ready"));
        Assert.That(workflows[1].GetProperty("when").GetProperty("kind").GetString(), Is.EqualTo("document-event"));
        Assert.That(workflows[1].GetProperty("when").GetProperty("name").GetString(), Is.EqualTo("resident-refresh"));
    }

    [Test]
    public void resident_component_registration_renders_bindings_and_object_event_workflows()
    {
        var plan = CreatePlan();
        Html.InputField(plan, m => m.Name)
            .NativeTextBox(b => b.Reactive(plan, evt => evt.Changed,
                (args, p) => p.Element("care-status").SetText("changed")));

        var json = plan.Render();
        AssertSchemaValid(json);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var binding = root.GetProperty("bindings").GetProperty("Name");
        var workflow = root.GetProperty("workflows")[0];
        var expectedId = IdGenerator.For<ResidentModel, string?>(m => m.Name);

        Assert.That(binding.GetProperty("object").GetString(), Is.EqualTo($"component::{expectedId}"));
        Assert.That(binding.GetProperty("valueMember").GetString(), Is.EqualTo("value"));
        Assert.That(workflow.GetProperty("when").GetProperty("kind").GetString(), Is.EqualTo("object-event"));
        Assert.That(workflow.GetProperty("when").GetProperty("object").GetString(), Is.EqualTo($"component::{expectedId}"));
        Assert.That(workflow.GetProperty("when").GetProperty("event").GetString(), Is.EqualTo("change"));
    }

    [Test]
    public void resident_shared_native_event_contract_keeps_payload_generic_across_multiple_components()
    {
        var plan = CreatePlan();
        Html.InputField(plan, m => m.Name)
            .NativeTextBox(b => b.Reactive(plan, evt => evt.Changed,
                (args, p) => p.Element("name-status").SetText("changed")));
        Html.InputField(plan, m => m.Email)
            .NativeTextBox(b => b.Reactive(plan, evt => evt.Changed,
                (args, p) => p.Element("email-status").SetText("changed")));

        var json = plan.Render();
        AssertSchemaValid(json);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var eventContract = root.GetProperty("contracts")
            .GetProperty("native.textbox")
            .GetProperty("events")
            .GetProperty("change");
        var eventObjectContractKey = eventContract
            .GetProperty("eventObject")
            .GetProperty("contract")
            .GetString();

        Assert.That(eventObjectContractKey, Is.Not.Null.And.Not.Empty);
        Assert.That(eventContract.GetProperty("data").GetProperty("value").GetProperty("kind").GetString(), Is.EqualTo("member"));
        Assert.That(eventContract.GetProperty("data").GetProperty("value").GetProperty("object").GetString(), Is.EqualTo("$eventObject"));
        Assert.That(eventContract.GetProperty("data").GetProperty("value").GetProperty("member").GetString(), Is.EqualTo("value"));

        var eventObjectValueMember = root.GetProperty("contracts")
            .GetProperty(eventObjectContractKey!)
            .GetProperty("members")
            .GetProperty("value");

        Assert.That(eventObjectValueMember.GetProperty("path")[0].GetProperty("prop").GetString(), Is.EqualTo("currentTarget"));
        Assert.That(eventObjectValueMember.GetProperty("path")[1].GetProperty("prop").GetString(), Is.EqualTo("value"));
    }

    [Test]
    public void resident_request_plan_renders_binding_map_and_response_handlers()
    {
        var plan = CreatePlan();
        Html.InputField(plan, m => m.Name).NativeTextBox(_ => { });

        On(plan, t => t.CustomEvent("save-resident", p =>
        {
            p.Post("/api/residents", g => g.IncludeAll())
             .Response(r => r
                .OnSuccess(s => s.Element("status").SetText("saved"))
                .OnError(400, e => e.Element("status").SetText("invalid")));
        }));

        var json = plan.Render();
        AssertSchemaValid(json);

        using var doc = JsonDocument.Parse(json);
        var request = doc.RootElement.GetProperty("workflows")[0]
            .GetProperty("run")
            .GetProperty("request");

        Assert.That(request.GetProperty("method").GetString(), Is.EqualTo("POST"));
        Assert.That(request.GetProperty("url").GetString(), Is.EqualTo("/api/residents"));
        Assert.That(request.GetProperty("input").GetProperty("value").GetProperty("kind").GetString(), Is.EqualTo("binding-map"));
        Assert.That(request.GetProperty("onSuccess")[0].GetProperty("run").GetProperty("kind").GetString(), Is.EqualTo("set"));
        Assert.That(request.GetProperty("onError")[0].GetProperty("statusCode").GetInt32(), Is.EqualTo(400));
    }

    [Test]
    public void resident_live_channels_render_server_push_and_signalr_subscriptions()
    {
        var plan = CreatePlan();
        On(plan, t => t
            .ServerPush("/api/residents/stream", "resident-updated", p => p.Element("status").SetText("stream"))
            .SignalR("/hubs/residents", "ReceiveResidentAlert", p => p.Element("status").SetText("hub")));

        var json = plan.Render();
        AssertSchemaValid(json);

        using var doc = JsonDocument.Parse(json);
        var workflows = doc.RootElement.GetProperty("workflows");

        Assert.That(workflows[0].GetProperty("when").GetProperty("kind").GetString(), Is.EqualTo("server-push"));
        Assert.That(workflows[0].GetProperty("when").GetProperty("eventType").GetString(), Is.EqualTo("resident-updated"));
        Assert.That(workflows[1].GetProperty("when").GetProperty("kind").GetString(), Is.EqualTo("signalr"));
        Assert.That(workflows[1].GetProperty("when").GetProperty("method").GetString(), Is.EqualTo("ReceiveResidentAlert"));
    }
}
