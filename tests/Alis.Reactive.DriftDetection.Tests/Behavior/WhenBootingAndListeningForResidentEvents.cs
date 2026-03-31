using Alis.Reactive.DriftDetection.Tests.Infrastructure;
using Alis.Reactive.Native.Components;
using Alis.Reactive.Native.Extensions;

namespace Alis.Reactive.DriftDetection.Tests.Behavior;

[TestFixture]
public class WhenBootingAndListeningForResidentEvents : DriftTestBase
{
    [Test]
    public void resident_boot_reactions_cover_dom_ready_dispatch_and_sequential_shape()
    {
        AssertDefinitionPropertiesExactly("DomReadyTrigger", "kind");
        AssertDefinitionPropertiesExactly("SequentialReaction", "kind", "commands");
        AssertDefinitionPropertiesExactly("DispatchCommand", "kind", "event", "payload");

        var plan = CreatePlan();
        On(plan, t => t.DomReady(p =>
        {
            p.Element("step-1").AddClass("complete");
            p.Dispatch("booted");
        }));

        var json = plan.Render();
        AssertSchemaValid(json);

        AssertPropertiesExactly(json, "entries[0].trigger", "kind");
        AssertPropertiesExactly(json, "entries[0].reaction", "kind", "commands");
        AssertPropertiesExactly(json, "entries[0].reaction.commands[1]", "kind", "event");
    }

    [Test]
    public void resident_events_cover_custom_triggers_with_and_without_typed_payloads()
    {
        AssertDefinitionPropertiesExactly("CustomEventTrigger", "kind", "event");

        var plan = CreatePlan();
        On(plan, t => t.CustomEvent("resident-saved", p => p.Dispatch("ack")));
        On(plan, t => t.CustomEvent<ResidentModel>("resident-updated", (args, p) =>
            p.Element("name-echo").SetText(args, x => x.Name!)));

        var json = plan.Render();
        AssertSchemaValid(json);

        AssertPropertiesExactly(json, "entries[0].trigger", "kind", "event");
        AssertPropertiesExactly(json, "entries[1].trigger", "kind", "event");
    }

    [Test]
    public void resident_live_channels_cover_server_push_and_signalr_triggers()
    {
        AssertDefinitionPropertiesExactly("ServerPushTrigger", "kind", "url", "eventType");
        AssertDefinitionPropertiesExactly("SignalRTrigger", "kind", "hubUrl", "methodName");

        var plan = CreatePlan();
        On(plan, t => t.ServerPush("/api/notifications/stream", "admission", p =>
            p.Dispatch("new-admission")));
        On(plan, t => t.SignalR("/hubs/residents", "ReceiveUpdate", p =>
            p.Dispatch("resident-updated")));

        var json = plan.Render();
        AssertSchemaValid(json);

        AssertPropertiesExactly(json, "entries[0].trigger", "kind", "url", "eventType");
        AssertPropertiesExactly(json, "entries[1].trigger", "kind", "hubUrl", "methodName");
    }

    [Test]
    public void resident_form_components_cover_component_event_triggers_and_native_registration()
    {
        AssertDefinitionPropertiesExactly("ComponentEventTrigger",
            "kind", "componentId", "jsEvent", "vendor", "bindingPath", "readExpr");
        AssertDefinitionPropertiesExactly("ComponentEntry",
            "id", "vendor", "readExpr", "componentType", "coerceAs");

        var plan = CreatePlan();

        Html.InputField(plan, m => m.Name)
            .NativeTextBox(b => b
                .Reactive(plan, evt => evt.Changed, (args, p) =>
                {
                    p.Element("name-echo").SetText(args, x => x.Value!);
                }));

        var json = plan.Render();
        AssertSchemaValid(json);

        AssertPropertiesExactly(json, "entries[0].trigger",
            "kind", "componentId", "jsEvent", "vendor", "bindingPath", "readExpr");
        AssertPropertiesExactly(json, "components.Name",
            "id", "vendor", "readExpr", "componentType", "coerceAs");
    }
}
