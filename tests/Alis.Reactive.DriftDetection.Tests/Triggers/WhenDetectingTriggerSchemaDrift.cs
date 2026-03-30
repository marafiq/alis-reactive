using Alis.Reactive.DriftDetection.Tests.Infrastructure;
using Alis.Reactive.Native.Components;
using Alis.Reactive.Native.Extensions;

namespace Alis.Reactive.DriftDetection.Tests.Triggers;

[TestFixture]
public class WhenDetectingTriggerSchemaDrift : DriftTestBase
{
    [Test]
    public void dom_ready_trigger_conforms_to_schema()
    {
        AssertDefinitionPropertiesExactly("DomReadyTrigger", "kind");

        var plan = CreatePlan();
        On(plan, t => t.DomReady(p => p.Dispatch("init")));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertAllPropertiesPresent(json, "DomReadyTrigger", "entries[0].trigger");
    }

    [Test]
    public void custom_event_trigger_conforms_to_schema()
    {
        AssertDefinitionPropertiesExactly("CustomEventTrigger", "kind", "event");

        var plan = CreatePlan();
        On(plan, t => t.CustomEvent("resident-saved", p => p.Dispatch("ack")));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertAllPropertiesPresent(json, "CustomEventTrigger", "entries[0].trigger");
    }

    [Test]
    public void typed_custom_event_conforms_to_schema()
    {
        AssertDefinitionPropertiesExactly("CustomEventTrigger", "kind", "event");

        var plan = CreatePlan();
        On(plan, t => t.CustomEvent<ResidentModel>("resident-updated", (args, p) =>
            p.Element("name-echo").SetText(args, x => x.Name!)));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertAllPropertiesPresent(json, "CustomEventTrigger", "entries[0].trigger");
    }

    [Test]
    public void server_push_with_all_properties_conforms()
    {
        AssertDefinitionPropertiesExactly("ServerPushTrigger", "kind", "url", "eventType");

        var plan = CreatePlan();
        On(plan, t => t.ServerPush("/api/notifications/stream", "admission", p =>
            p.Dispatch("new-admission")));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertAllPropertiesPresent(json, "ServerPushTrigger", "entries[0].trigger");
    }

    [Test]
    public void signalr_trigger_conforms_to_schema()
    {
        AssertDefinitionPropertiesExactly("SignalRTrigger", "kind", "hubUrl", "methodName");

        var plan = CreatePlan();
        On(plan, t => t.SignalR("/hubs/residents", "ReceiveUpdate", p =>
            p.Dispatch("resident-updated")));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertAllPropertiesPresent(json, "SignalRTrigger", "entries[0].trigger");
    }

    [Test]
    public void component_event_with_all_properties_conforms()
    {
        AssertDefinitionPropertiesExactly("ComponentEventTrigger",
            "kind", "componentId", "jsEvent", "vendor", "bindingPath", "readExpr");

        var plan = CreatePlan();

        // InputField + NativeTextBox + Reactive produces a ComponentEventTrigger.
        Html.InputField(plan, m => m.Name)
            .NativeTextBox(b => b
                .Reactive(plan, evt => evt.Changed, (args, p) =>
                {
                    p.Element("name-echo").SetText(args, x => x.Value!);
                }));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertAllPropertiesPresent(json, "ComponentEventTrigger", "entries[0].trigger");
    }

    [Test]
    public void multiple_triggers_all_conform()
    {
        AssertDefinitionPropertiesExactly("DomReadyTrigger", "kind");
        AssertDefinitionPropertiesExactly("CustomEventTrigger", "kind", "event");

        var plan = CreatePlan();
        On(plan, t => t
            .DomReady(p => p.Dispatch("boot"))
            .CustomEvent("user-action", p => p.Element("status").SetText("done")));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertAllPropertiesPresent(json, "DomReadyTrigger", "entries[0].trigger");
        AssertAllPropertiesPresent(json, "CustomEventTrigger", "entries[1].trigger");
    }
}
