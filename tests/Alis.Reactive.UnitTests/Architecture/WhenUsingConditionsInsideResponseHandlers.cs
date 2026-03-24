namespace Alis.Reactive.UnitTests;

/// <summary>
/// Conditions inside OnSuccess/OnError response handlers use a nested PipelineBuilder.
/// They produce conditional reactions within the StatusHandler, independent of the
/// outer pipeline's segmentation. Covers ElseIf chains, compound guards (And/Or),
/// conditions in both OnSuccess and OnError, and chained HTTP stages.
/// </summary>
[TestFixture]
public class WhenUsingConditionsInsideResponseHandlers : PlanTestBase
{
    // ── Condition inside OnSuccess ──

    [Test]
    public Task Condition_inside_on_success_with_surrounding_commands()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Element("status").SetText("loading");
            p.Post("/api/save")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("status").SetText("saved");
                 s.When(args, x => x.Active).Truthy()
                     .Then(t => t.Element("badge").Show())
                     .Else(e => e.Element("badge").Hide());
                 s.Element("timestamp").SetText("now");
             }));
            p.Element("footer").SetText("done");
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("loading"), "Pre-http command");
        Assert.That(json, Does.Contain("saved"), "OnSuccess command");
        Assert.That(json, Does.Contain("badge"), "Condition inside OnSuccess");
        Assert.That(json, Does.Contain("timestamp"), "Post-condition command inside OnSuccess");
        Assert.That(json, Does.Contain("footer"), "Post-http command");
        return VerifyJson(json);
    }

    // ── ElseIf chain inside OnSuccess ──

    [Test]
    public Task ElseIf_chain_inside_on_success()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Post("/api/process")
             .Response(r => r.OnSuccess(s =>
             {
                 s.When(args, x => x.Count).Gt(100)
                     .Then(t => t.Element("tier").SetText("gold"))
                     .ElseIf(args, x => x.Count).Gt(50)
                     .Then(t => t.Element("tier").SetText("silver"))
                     .ElseIf(args, x => x.Count).Gt(10)
                     .Then(t => t.Element("tier").SetText("bronze"))
                     .Else(e => e.Element("tier").SetText("none"));
             }));
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("gold"));
        Assert.That(json, Does.Contain("silver"));
        Assert.That(json, Does.Contain("bronze"));
        Assert.That(json, Does.Contain("none"));
        return VerifyJson(json);
    }

    [Test]
    public Task ElseIf_chain_inside_on_success_then_outer_condition()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Post("/api/classify")
             .Response(r => r.OnSuccess(s =>
             {
                 s.When(args, x => x.Count).Gt(1000)
                     .Then(t => t.Element("class").SetText("enterprise"))
                     .ElseIf(args, x => x.Count).Gt(100)
                     .Then(t => t.Element("class").SetText("business"))
                     .ElseIf(args, x => x.Count).Gt(10)
                     .Then(t => t.Element("class").SetText("team"))
                     .Else(e => e.Element("class").SetText("individual"));
             }));
            p.When(args, x => x.Active).Truthy()
                .Then(t => t.Element("trial-badge").Hide())
                .Else(e => e.Element("trial-badge").Show());
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("enterprise"));
        Assert.That(json, Does.Contain("business"));
        Assert.That(json, Does.Contain("team"));
        Assert.That(json, Does.Contain("individual"));
        Assert.That(json, Does.Contain("trial-badge"));
        return VerifyJson(json);
    }

    // ── Inner + outer conditions ──

    [Test]
    public Task Conditions_inside_and_outside_http_response()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Post("/api/evaluate")
             .Response(r => r.OnSuccess(s =>
             {
                 s.When(args, x => x.Count).Gt(50)
                     .Then(t => t.Element("inner-tier").SetText("premium"))
                     .Else(e => e.Element("inner-tier").SetText("standard"));
             }));
            p.When(args, x => x.Active).Truthy()
                .Then(t => t.Element("outer-active").Show())
                .Else(e => e.Element("outer-active").Hide());
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("inner-tier"));
        Assert.That(json, Does.Contain("premium"));
        Assert.That(json, Does.Contain("standard"));
        Assert.That(json, Does.Contain("outer-active"));
        return VerifyJson(json);
    }

    // ── Condition inside OnError ──

    [Test]
    public Task Condition_inside_on_error_handler()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Post("/api/submit")
             .Response(r => r
                 .OnSuccess(s => s.Element("status").SetText("ok"))
                 .OnError(400, e =>
                 {
                     e.When(args, x => x.Category).Eq("required")
                         .Then(t => t.Element("error-msg").SetText("missing required fields"))
                         .Else(el => el.Element("error-msg").SetText("validation error"));
                 }));
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("ok"));
        Assert.That(json, Does.Contain("missing required fields"));
        Assert.That(json, Does.Contain("validation error"));
        Assert.That(json, Does.Contain("400"));
        return VerifyJson(json);
    }

    [Test]
    public Task Conditions_in_both_on_success_and_on_error()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Post("/api/transfer")
             .Response(r => r
                 .OnSuccess(s =>
                 {
                     s.When(args, x => x.Count).Gt(0)
                         .Then(t => t.Element("transfer-detail").SetText("items transferred"))
                         .Else(e => e.Element("transfer-detail").SetText("nothing to transfer"));
                 })
                 .OnError(409, e =>
                 {
                     e.When(args, x => x.Category).Eq("locked")
                         .Then(t => t.Element("conflict-msg").SetText("resource locked"))
                         .Else(el => el.Element("conflict-msg").SetText("conflict detected"));
                 }));
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("items transferred"));
        Assert.That(json, Does.Contain("nothing to transfer"));
        Assert.That(json, Does.Contain("resource locked"));
        Assert.That(json, Does.Contain("conflict detected"));
        Assert.That(json, Does.Contain("409"));
        return VerifyJson(json);
    }

    // ── Compound guard inside OnSuccess ──

    [Test]
    public Task And_guard_inside_on_success()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Post("/api/evaluate")
             .Response(r => r.OnSuccess(s =>
             {
                 s.When(args, x => x.Active).Truthy()
                     .And(args, x => x.Count).Gt(5)
                     .Then(t => t.Element("qualified").Show())
                     .Else(e => e.Element("qualified").Hide());
             }));
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("qualified"));
        Assert.That(json, Does.Contain("all"), "And guard produces 'all' composite");
        Assert.That(json, Does.Contain("truthy"));
        Assert.That(json, Does.Contain("gt"));
        return VerifyJson(json);
    }

    // ── Conditions in chained HTTP response stages ──

    [Test]
    public Task Chained_http_with_conditions_in_each_stage()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Post("/api/step1")
             .Response(r => r
                 .OnSuccess(s =>
                 {
                     s.When(args, x => x.Active).Truthy()
                         .Then(t => t.Element("step1-status").SetText("active user"))
                         .Else(e => e.Element("step1-status").SetText("inactive user"));
                 })
                 .Chained(chain => chain.Post("/api/step2")
                     .Response(cr => cr.OnSuccess(cs =>
                     {
                         cs.When(args, x => x.Count).Gt(0)
                             .Then(t => t.Element("step2-count").SetText("has items"))
                             .Else(e => e.Element("step2-count").SetText("empty"));
                     }))));
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("/api/step1"));
        Assert.That(json, Does.Contain("active user"));
        Assert.That(json, Does.Contain("inactive user"));
        Assert.That(json, Does.Contain("/api/step2"));
        Assert.That(json, Does.Contain("has items"));
        Assert.That(json, Does.Contain("empty"));
        return VerifyJson(json);
    }
}
