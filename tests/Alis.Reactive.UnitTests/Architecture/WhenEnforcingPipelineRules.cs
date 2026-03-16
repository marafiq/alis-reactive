namespace Alis.Reactive.UnitTests;

public class OrderPayload
{
    public decimal Total { get; set; }
    public string Status { get; set; } = "";
    public string Role { get; set; } = "";
}

[TestFixture]
public class WhenEnforcingPipelineRules : PlanTestBase
{
    // ── Commands + Conditional (no HTTP) ──

    [Test]
    public Task Single_command_before_condition()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<OrderPayload>("order-submitted", (args, p) =>
        {
            p.Dispatch("audit-log");
            p.When(args, x => x.Total).Gte(100m)
                .Then(t => t.Element("express").Show());
        });
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    [Test]
    public Task Multiple_commands_before_condition()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<OrderPayload>("order-submitted", (args, p) =>
        {
            p.Element("confirmation").Show();
            p.Dispatch("audit-log");
            p.Element("spinner").Hide();
            p.When(args, x => x.Total).Gte(100m)
                .Then(t => t.Element("express").Show())
                .Else(e => e.Element("standard").Show());
        });
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    [Test]
    public Task Commands_before_multi_branch_condition()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<OrderPayload>("order-submitted", (args, p) =>
        {
            p.Element("status").SetText("Processing...");
            p.Dispatch("order-received");
            p.When(args, x => x.Total).Gte(1000m)
                .Then(t => t.Element("tier").SetText("Gold"))
                .ElseIf(args, x => x.Total).Gte(500m)
                .Then(t => t.Element("tier").SetText("Silver"))
                .Else(e => e.Element("tier").SetText("Bronze"));
        });
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    [Test]
    public Task Multiple_commands_in_then_branch()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<OrderPayload>("role-check", (args, p) =>
        {
            p.Element("loading").Hide();
            p.When(args, x => x.Role).Eq("admin")
                .Then(t =>
                {
                    t.Element("admin-panel").Show();
                    t.Element("delete-btn").Show();
                    t.Dispatch("admin-loaded");
                })
                .Else(e =>
                {
                    e.Element("admin-panel").Hide();
                    e.Element("delete-btn").Hide();
                });
        });
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    // ── Condition-only (no pre-commands) still works ──

    [Test]
    public Task Condition_without_pre_commands()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<OrderPayload>("status-check", (args, p) =>
            p.When(args, x => x.Status).Eq("active")
                .Then(t => t.Element("badge").Show())
                .Else(e => e.Element("badge").Hide())
        );
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    // ── Commands before HTTP (preFetch — existing pattern) ──

    [Test]
    public void Commands_before_http_as_preFetch()
    {
        var plan = CreatePlan();
        Assert.DoesNotThrow(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Element("spinner").Show();
                p.Get("/api/test")
                 .Response(r => r.OnSuccess(s => s.Element("spinner").Hide()));
            });
        });
    }
}
