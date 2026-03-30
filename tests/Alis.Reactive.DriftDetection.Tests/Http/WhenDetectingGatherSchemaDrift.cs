using Alis.Reactive.DriftDetection.Tests.Infrastructure;
using Alis.Reactive.Native.Components;
using Alis.Reactive.Native.Extensions;

namespace Alis.Reactive.DriftDetection.Tests.Http;

[TestFixture]
public class WhenDetectingGatherSchemaDrift : DriftTestBase
{
    [Test]
    public void include_all_conforms()
    {
        // AllGather: kind
        AssertDefinitionPropertiesExactly("AllGather", "kind");

        var plan = CreatePlan();
        On(plan, t => t.CustomEvent("submit", p =>
            p.Post("/api/residents", g => g.IncludeAll())));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertAllPropertiesPresent(json, "AllGather",
            "entries[0].reaction.request.gather[0]");
    }

    [Test]
    public void static_gather_conforms()
    {
        // StaticGather: kind, param, value
        AssertDefinitionPropertiesExactly("StaticGather", "kind", "param", "value");

        var plan = CreatePlan();
        On(plan, t => t.CustomEvent("submit", p =>
            p.Post("/api/residents", g => g.Static("facilityId", "FAC-001"))));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertAllPropertiesPresent(json, "StaticGather",
            "entries[0].reaction.request.gather[0]");
    }

    [Test]
    public void from_event_gather_conforms()
    {
        // EventGather: kind, param, path
        AssertDefinitionPropertiesExactly("EventGather", "kind", "param", "path");

        var plan = CreatePlan();
        On(plan, t => t.CustomEvent<ResidentModel>("save", (args, p) =>
            p.Post("/api/residents", g =>
                g.FromEvent(args, x => x.Name!, "residentName"))));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertAllPropertiesPresent(json, "EventGather",
            "entries[0].reaction.request.gather[0]");
    }

    [Test]
    public void component_gather_conforms()
    {
        // ComponentGather: kind, componentId, vendor, name, readExpr
        // Requires a component registered in the plan via InputField.
        // Native Include extension produces a ComponentGather.
        AssertDefinitionPropertiesExactly("ComponentGather",
            "kind", "componentId", "vendor", "name", "readExpr");

        var plan = CreatePlan();

        Html.InputField(plan, m => m.Name)
            .NativeTextBox(b => b.Placeholder("Name"));

        On(plan, t => t.CustomEvent("submit", p =>
            p.Post("/api/residents", g => g.Include(m => m.Name))));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertAllPropertiesPresent(json, "ComponentGather",
            "entries[0].reaction.request.gather[0]");
    }
}
