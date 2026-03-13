using static VerifyNUnit.Verifier;

namespace Alis.Reactive.UnitTests;

/// <summary>
/// Verifies that conditions work in EVERY DSL surface that accepts a PipelineBuilder,
/// not just triggers and branch bodies. If the DSL allows writing conditions there,
/// the framework must support it.
/// </summary>
[TestFixture]
public class WhenUsingConditionsInEveryDslSurface : PlanTestBase
{
    // ── OnSuccess response handler ──

    [Test]
    public Task Conditions_inside_OnSuccess()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<WorkflowPayload>("save", (args, p) =>
            p.Post("/api/save")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("status").SetText("Saved");
                 s.When(args, x => x.Role).Eq("admin")
                     .Then(t => t.Element("admin-notice").Show())
                     .Else(e => e.Element("admin-notice").Hide());
             }))
        );
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    [Test]
    public Task IfElseIfElse_inside_OnSuccess()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<WorkflowPayload>("process", (args, p) =>
            p.Post("/api/process")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("status").SetText("Done");
                 s.When(args, x => x.Total).Gte(1000m)
                     .Then(t => t.Element("tier").SetText("Gold"))
                     .ElseIf(args, x => x.Total).Gte(500m)
                     .Then(t => t.Element("tier").SetText("Silver"))
                     .Else(e => e.Element("tier").SetText("Bronze"));
             }))
        );
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    [Test]
    public Task Http_inside_OnSuccess_branch()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<WorkflowPayload>("check", (args, p) =>
            p.Post("/api/check")
             .Response(r => r.OnSuccess(s =>
                 s.When(args, x => x.IsActive).Truthy()
                     .Then(t =>
                         t.Post("/api/activate")
                          .Response(r2 => r2.OnSuccess(s2 => s2.Dispatch("activated"))))
                     .Else(e => e.Element("error").SetText("Inactive"))
             ))
        );
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    // ── OnError response handler ──

    [Test]
    public Task Conditions_inside_OnError()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<WorkflowPayload>("save", (args, p) =>
            p.Post("/api/save")
             .Response(r => r.OnError(500, e =>
             {
                 e.When(args, x => x.Role).Eq("admin")
                     .Then(t => t.Element("error").SetText("Admin: check server logs"))
                     .Else(el => el.Element("error").SetText("Please try again"));
             }))
        );
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    // ── Compound conditions in OnSuccess ──

    [Test]
    public Task Compound_And_inside_OnSuccess()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<WorkflowPayload>("auth", (args, p) =>
            p.Post("/api/auth")
             .Response(r => r.OnSuccess(s =>
                 s.When(args, x => x.IsActive).Truthy()
                     .And(args, x => x.IsAdmin).Truthy()
                     .Then(t => t.Element("admin-panel").Show())
                     .Else(e => e.Element("admin-panel").Hide())
             ))
        );
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    // ── Confirm in OnSuccess ──

    [Test]
    public Task Confirm_inside_OnSuccess()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<WorkflowPayload>("delete-check", (args, p) =>
            p.Post("/api/can-delete")
             .Response(r => r.OnSuccess(s =>
                 s.Confirm("Permanently delete this record?")
                     .Then(t =>
                         t.Post("/api/delete")
                          .Response(r2 => r2.OnSuccess(s2 => s2.Dispatch("deleted"))))
                     .Else(e => e.Element("status").SetText("Cancelled"))
             ))
        );
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    // ── Multiple actions + condition in OnSuccess ──

    [Test]
    public Task Unconditional_actions_plus_condition_inside_OnSuccess()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<WorkflowPayload>("load", (args, p) =>
            p.Post("/api/load")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("spinner").Hide();
                 s.Element("content").Show();
                 s.Dispatch("data-loaded");
                 s.When(args, x => x.Role).Eq("admin")
                     .Then(t => t.Element("admin-tools").Show())
                     .Else(e => e.Element("admin-tools").Hide());
             }))
        );
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }
}
