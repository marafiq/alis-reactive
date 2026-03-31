using Alis.Reactive.DriftDetection.Tests.Infrastructure;
using Alis.Reactive.Native.Components;

namespace Alis.Reactive.DriftDetection.Tests.Behavior;

[TestFixture]
public class WhenRoutingResidentGuardDecisions : DriftTestBase
{
    [Test]
    public void resident_triage_ladders_cover_branch_level_condition_syntax()
    {
        AssertDefinitionPropertiesExactly("ValueGuard",
            "kind", "source", "coerceAs", "op", "operand", "rightSource", "elementCoerceAs");
        AssertDefinitionPropertiesExactly("ConditionalReaction", "kind", "commands", "branches");
        AssertDefinitionPropertiesExactly("Branch", "guard", "reaction");

        var plan = CreatePlan();
        On(plan, t => t.CustomEvent<ResidentModel>("assess", (args, p) =>
        {
            p.Element("status").SetText("Assessing...");

            p.When(args, x => x.CareLevel!).Eq("Memory Care")
             .Then(tp => tp.Element("rate").SetText("$5,200"))
             .ElseIf(args, x => x.CareLevel!).Eq("Assisted Living")
             .Then(tp => tp.Element("rate").SetText("$3,800"))
             .Else(ep => ep.Element("rate").SetText("$2,400"));
        }));

        var json = plan.Render();
        AssertSchemaValid(json);

        AssertPropertiesExactly(json, "entries[0].reaction", "kind", "commands", "branches");
        AssertPropertiesExactly(json, "entries[0].reaction.branches[0]", "guard", "reaction");
        AssertPropertiesExactly(json, "entries[0].reaction.branches[0].guard",
            "kind", "source", "coerceAs", "op", "operand");
        AssertPropertiesExactly(json, "entries[0].reaction.branches[2]", "reaction");
    }

    [Test]
    public void resident_comparisons_cover_source_to_source_and_array_contains_variants()
    {
        AssertDefinitionPropertiesExactly("ValueGuard",
            "kind", "source", "coerceAs", "op", "operand", "rightSource", "elementCoerceAs");

        var plan = CreatePlan();
        On(plan, t => t.DomReady(p =>
        {
            var nameComp = p.Component<NativeTextBox>(m => m.Name);
            var emailComp = p.Component<NativeTextBox>(m => m.Email);

            p.When(nameComp.Value()).Eq(emailComp.Value())
             .Then(tp => tp.Element("match").Show());
        }));
        On(plan, t => t.CustomEvent<ResidentModel>("check-tags", (args, p) =>
        {
            p.When(args, x => x.CareTags!).ArrayContains("Memory Care")
             .Then(tp => tp.Element("found").Show());
        }));

        var json = plan.Render();
        AssertSchemaValid(json);

        AssertPropertiesExactly(json, "entries[0].reaction.branches[0].guard",
            "kind", "source", "coerceAs", "op", "rightSource");
        AssertPropertiesExactly(json, "entries[1].reaction.branches[0].guard",
            "kind", "source", "coerceAs", "op", "operand", "elementCoerceAs");
    }

    [Test]
    public void resident_compound_guard_trees_cover_all_any_and_invert_shapes()
    {
        AssertDefinitionPropertiesExactly("AllGuard", "kind", "guards");
        AssertDefinitionPropertiesExactly("AnyGuard", "kind", "guards");
        AssertDefinitionPropertiesExactly("InvertGuard", "kind", "inner");

        var plan = CreatePlan();
        On(plan, t => t.CustomEvent<ResidentModel>("check-all", (args, p) =>
        {
            p.When(args, x => x.CareLevel!).NotEmpty()
             .And(args, x => x.Name!).NotEmpty()
             .Then(tp => tp.Element("both-filled").Show());
        }));
        On(plan, t => t.CustomEvent<ResidentModel>("check-any", (args, p) =>
        {
            p.When(args, x => x.CareLevel!).Eq("Memory Care")
             .Or(args, x => x.CareLevel!).Eq("Assisted Living")
             .Then(tp => tp.Element("special-care").Show());
        }));
        On(plan, t => t.CustomEvent<ResidentModel>("check-not", (args, p) =>
        {
            p.When(args, x => x.CareLevel!).Eq("Discharged").Not()
             .Then(tp => tp.Element("active-badge").Show());
        }));

        var json = plan.Render();
        AssertSchemaValid(json);

        AssertPropertiesExactly(json, "entries[0].reaction.branches[0].guard", "kind", "guards");
        AssertPropertiesExactly(json, "entries[1].reaction.branches[0].guard", "kind", "guards");
        AssertPropertiesExactly(json, "entries[2].reaction.branches[0].guard", "kind", "inner");
    }

    [Test]
    public void resident_confirmation_guards_remain_supported_without_expanding_syntax()
    {
        AssertDefinitionPropertiesExactly("ConfirmGuard", "kind", "message");

        var plan = CreatePlan();
        On(plan, t => t.CustomEvent("delete-resident", p =>
        {
            p.Confirm("Are you sure you want to discharge this resident?")
             .Then(tp => tp.Dispatch("confirmed-discharge"));
        }));

        var json = plan.Render();
        AssertSchemaValid(json);

        AssertPropertiesExactly(json, "entries[0].reaction.branches[0].guard", "kind", "message");
    }

    [Test]
    public void resident_component_values_can_drive_guard_sources_without_route_expansion()
    {
        AssertDefinitionPropertiesExactly("ComponentSource",
            "kind", "componentId", "vendor", "readExpr");

        var plan = CreatePlan();
        On(plan, t => t.DomReady(p =>
        {
            var nameComp = p.Component<NativeTextBox>(m => m.Name);
            p.When(nameComp.Value()).NotEmpty()
             .Then(tp => tp.Element("name-filled").Show())
             .Else(ep => ep.Element("name-filled").Hide());
        }));

        var json = plan.Render();
        AssertSchemaValid(json);

        AssertPropertiesExactly(json, "entries[0].reaction.branches[0].guard.source",
            "kind", "componentId", "vendor", "readExpr");
    }
}
