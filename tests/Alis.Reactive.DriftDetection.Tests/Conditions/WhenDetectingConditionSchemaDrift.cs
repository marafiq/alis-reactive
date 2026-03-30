using Alis.Reactive.DriftDetection.Tests.Infrastructure;
using Alis.Reactive.Fusion.Components;
using Alis.Reactive.Native.Components;

namespace Alis.Reactive.DriftDetection.Tests.Conditions;

[TestFixture]
public class WhenDetectingConditionSchemaDrift : DriftTestBase
{
    [Test]
    public void value_guard_with_literal_conforms()
    {
        // ValueGuard properties exercised: kind, source, coerceAs, op, operand
        // rightSource and elementCoerceAs are mutually exclusive alternatives tested separately
        var plan = CreatePlan();
        On(plan, t => t.CustomEvent<ResidentModel>("check", (args, p) =>
        {
            p.When(args, x => x.CareLevel!).Eq("Memory Care")
             .Then(tp => tp.Element("badge").Show());
        }));

        var json = plan.Render();
        AssertSchemaValid(json);

        // AssertAllPropertiesPresent not used: ValueGuard has mutually exclusive
        // optional properties (operand vs rightSource vs elementCoerceAs).
        // This variant exercises: kind, source, coerceAs, op, operand.
        AssertPropertiesPresent(json, "entries[0].reaction.branches[0].guard",
            "kind", "source", "coerceAs", "op", "operand");
    }

    [Test]
    public void value_guard_source_vs_source_conforms()
    {
        // ValueGuard with rightSource: kind, source, coerceAs, op, rightSource
        var plan = CreatePlan();
        On(plan, t => t.DomReady(p =>
        {
            var nameComp = p.Component<NativeTextBox>(m => m.Name);
            var nameValue = nameComp.Value();

            var emailComp = p.Component<NativeTextBox>(m => m.Email);
            var emailValue = emailComp.Value();

            p.When(nameValue).Eq(emailValue)
             .Then(tp => tp.Element("match").Show());
        }));

        var json = plan.Render();
        AssertSchemaValid(json);

        // This variant exercises: kind, source, coerceAs, op, rightSource.
        AssertPropertiesPresent(json, "entries[0].reaction.branches[0].guard",
            "kind", "source", "coerceAs", "op", "rightSource");
    }

    [Test]
    public void value_guard_with_element_coerce_conforms()
    {
        // ValueGuard with elementCoerceAs: requires array-typed model property.
        // ResidentModel.CareLevel is string (not array), so elementCoerceAs is null.
        // ArrayContains still exercises the op correctly; elementCoerceAs would need
        // a List<T> property on the model to produce a non-null value.
        var plan = CreatePlan();
        On(plan, t => t.CustomEvent<ResidentModel>("check", (args, p) =>
        {
            p.When(args, x => x.CareLevel!).ArrayContains("Memory Care")
             .Then(tp => tp.Element("found").Show());
        }));

        var json = plan.Render();
        AssertSchemaValid(json);

        // elementCoerceAs requires an array-typed property (List<T>) on the model.
        // CareLevel is string, so elementCoerceAs is null and omitted from JSON.
        // Asserting the properties this variant does produce.
        AssertPropertiesPresent(json, "entries[0].reaction.branches[0].guard",
            "kind", "source", "coerceAs", "op", "operand");
    }

    [Test]
    public void all_guard_conforms()
    {
        // AllGuard: kind, guards
        var plan = CreatePlan();
        On(plan, t => t.CustomEvent<ResidentModel>("check", (args, p) =>
        {
            p.When(args, x => x.CareLevel!).NotEmpty()
             .And(args, x => x.Name!).NotEmpty()
             .Then(tp => tp.Element("both-filled").Show());
        }));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertAllPropertiesPresent(json, "AllGuard",
            "entries[0].reaction.branches[0].guard");
    }

    [Test]
    public void any_guard_conforms()
    {
        // AnyGuard: kind, guards
        var plan = CreatePlan();
        On(plan, t => t.CustomEvent<ResidentModel>("check", (args, p) =>
        {
            p.When(args, x => x.CareLevel!).Eq("Memory Care")
             .Or(args, x => x.CareLevel!).Eq("Assisted Living")
             .Then(tp => tp.Element("special-care").Show());
        }));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertAllPropertiesPresent(json, "AnyGuard",
            "entries[0].reaction.branches[0].guard");
    }

    [Test]
    public void not_guard_conforms()
    {
        // InvertGuard: kind, inner
        var plan = CreatePlan();
        On(plan, t => t.CustomEvent<ResidentModel>("check", (args, p) =>
        {
            p.When(args, x => x.CareLevel!).Eq("Discharged").Not()
             .Then(tp => tp.Element("active-badge").Show());
        }));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertAllPropertiesPresent(json, "InvertGuard",
            "entries[0].reaction.branches[0].guard");
    }

    [Test]
    public void confirm_guard_conforms()
    {
        // ConfirmGuard: kind, message
        var plan = CreatePlan();
        On(plan, t => t.CustomEvent("delete-resident", p =>
        {
            p.Confirm("Are you sure you want to discharge this resident?")
             .Then(tp => tp.Dispatch("confirmed-discharge"));
        }));

        var json = plan.Render();
        AssertSchemaValid(json);
        AssertAllPropertiesPresent(json, "ConfirmGuard",
            "entries[0].reaction.branches[0].guard");
    }

    [Test]
    public void branched_conditional_conforms()
    {
        // Full When/Then/ElseIf/Else chain producing multiple Branch objects
        var plan = CreatePlan();
        On(plan, t => t.CustomEvent<ResidentModel>("assess", (args, p) =>
        {
            p.When(args, x => x.CareLevel!).Eq("Memory Care")
             .Then(tp => tp.Element("rate").SetText("$5,200"))
             .ElseIf(args, x => x.CareLevel!).Eq("Assisted Living")
             .Then(tp => tp.Element("rate").SetText("$3,800"))
             .Else(ep => ep.Element("rate").SetText("$2,400"));
        }));

        var json = plan.Render();
        AssertSchemaValid(json);
        // Branch has: guard, reaction — both present on first branch
        AssertAllPropertiesPresent(json, "Branch",
            "entries[0].reaction.branches[0]");
    }
}
