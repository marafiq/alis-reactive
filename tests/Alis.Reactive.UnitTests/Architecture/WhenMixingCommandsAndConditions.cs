using Alis.Reactive.Builders.Conditions;

namespace Alis.Reactive.UnitTests;

public class MixedPayload
{
    public string? Value { get; set; }
    public int Count { get; set; }
    public bool Active { get; set; }
    public string? Category { get; set; }
}

/// <summary>
/// BDD tests for the pipeline's ability to mix sequential commands and conditional
/// blocks in any order. Each When().Then().Else() is an independent block — they
/// do not interfere with each other (no first-match-wins across blocks).
///
/// The pipeline builder segments content: commands → condition → commands → condition.
/// Each segment becomes a separate entry on the same trigger. The runtime fires all
/// entries for a trigger, so all blocks execute independently.
/// </summary>
[TestFixture]
public class WhenMixingCommandsAndConditions : PlanTestBase
{
    // ═══════════════════════════════════════════════════════════
    // Two independent When blocks — the core fix
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task Two_independent_when_blocks_both_produce_entries()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.When(args, x => x.Value).Eq("hello")
                .Then(t => t.Element("r1").SetText("yes"))
                .Else(e => e.Element("r1").SetText("no"));
            p.When(args, x => x.Count).Gt(5)
                .Then(t => t.Element("r2").SetText(">5"))
                .Else(e => e.Element("r2").SetText("<=5"));
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("r1"), "First condition (r1) must not be overwritten");
        Assert.That(json, Does.Contain("r2"), "Second condition (r2) must be present");
        Assert.That(json, Does.Contain("hello"), "First guard operand must be in plan");
        return VerifyJson(json);
    }

    [Test]
    public Task Three_independent_when_blocks()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.When(args, x => x.Value).Eq("a")
                .Then(t => t.Element("r1").SetText("match-a"));
            p.When(args, x => x.Count).Gt(10)
                .Then(t => t.Element("r2").SetText("high"));
            p.When(args, x => x.Active).Truthy()
                .Then(t => t.Element("r3").SetText("active"));
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("r1"));
        Assert.That(json, Does.Contain("r2"));
        Assert.That(json, Does.Contain("r3"));
        return VerifyJson(json);
    }

    // ═══════════════════════════════════════════════════════════
    // Commands before / between / after conditions
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task Commands_before_first_when()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Element("echo").SetText("before");
            p.When(args, x => x.Value).Eq("hello")
                .Then(t => t.Element("r1").SetText("matched"))
                .Else(e => e.Element("r1").SetText("nope"));
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("before"));
        Assert.That(json, Does.Contain("r1"));
        return VerifyJson(json);
    }

    [Test]
    public Task Commands_after_last_when()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.When(args, x => x.Value).Eq("hello")
                .Then(t => t.Element("r1").SetText("matched"))
                .Else(e => e.Element("r1").SetText("nope"));
            p.Element("footer").SetText("after");
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("r1"));
        Assert.That(json, Does.Contain("after"));
        return VerifyJson(json);
    }

    [Test]
    public Task Commands_between_two_conditions()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.When(args, x => x.Value).Eq("a")
                .Then(t => t.Element("r1").SetText("first"));
            p.Element("mid").SetText("between");
            p.When(args, x => x.Count).Gt(0)
                .Then(t => t.Element("r2").SetText("second"));
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("r1"));
        Assert.That(json, Does.Contain("between"));
        Assert.That(json, Does.Contain("r2"));
        return VerifyJson(json);
    }

    [Test]
    public Task Full_mix_commands_and_conditions()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Element("echo").SetText("before");
            p.When(args, x => x.Value).Eq("hello")
                .Then(t => t.Element("r1").SetText("yes"))
                .Else(e => e.Element("r1").SetText("no"));
            p.When(args, x => x.Count).Gt(5)
                .Then(t => t.Element("r2").SetText(">5"))
                .Else(e => e.Element("r2").SetText("<=5"));
            p.Element("footer").SetText("after");
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("before"), "Pre-command present");
        Assert.That(json, Does.Contain("r1"), "First condition present");
        Assert.That(json, Does.Contain("r2"), "Second condition present");
        Assert.That(json, Does.Contain("after"), "Post-command present");
        return VerifyJson(json);
    }

    [Test]
    public Task Dispatch_between_conditions()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.When(args, x => x.Active).Truthy()
                .Then(t => t.Element("badge").Show());
            p.Dispatch("audit-log");
            p.When(args, x => x.Value).NotEmpty()
                .Then(t => t.Element("status").SetText("has value"));
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("audit-log"));
        Assert.That(json, Does.Contain("badge"));
        Assert.That(json, Does.Contain("status"));
        return VerifyJson(json);
    }

    // ═══════════════════════════════════════════════════════════
    // ElseIf chains — single block, not split
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task ElseIf_chain_is_one_block_not_multiple()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.When(args, x => x.Count).Gt(100)
                .Then(t => t.Element("tier").SetText("gold"))
                .ElseIf(args, x => x.Count).Gt(50)
                .Then(t => t.Element("tier").SetText("silver"))
                .Else(e => e.Element("tier").SetText("bronze"));
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("gold"));
        Assert.That(json, Does.Contain("silver"));
        Assert.That(json, Does.Contain("bronze"));
        return VerifyJson(json);
    }

    [Test]
    public Task ElseIf_followed_by_independent_when()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.When(args, x => x.Count).Gt(100)
                .Then(t => t.Element("tier").SetText("gold"))
                .ElseIf(args, x => x.Count).Gt(50)
                .Then(t => t.Element("tier").SetText("silver"))
                .Else(e => e.Element("tier").SetText("bronze"));
            p.When(args, x => x.Active).Truthy()
                .Then(t => t.Element("badge").Show());
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("gold"));
        Assert.That(json, Does.Contain("silver"));
        Assert.That(json, Does.Contain("bronze"));
        Assert.That(json, Does.Contain("badge"));
        return VerifyJson(json);
    }

    // ═══════════════════════════════════════════════════════════
    // Then-only (no Else) conditions
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task Then_only_without_else()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.When(args, x => x.Active).Truthy()
                .Then(t => t.Element("badge").Show());
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("badge"));
        Assert.That(json, Does.Not.Contain("\"guard\":null"), "No else branch");
        return VerifyJson(json);
    }

    [Test]
    public Task Two_then_only_blocks()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.When(args, x => x.Active).Truthy()
                .Then(t => t.Element("badge").Show());
            p.When(args, x => x.Count).Gt(0)
                .Then(t => t.Element("counter").Show());
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("badge"));
        Assert.That(json, Does.Contain("counter"));
        return VerifyJson(json);
    }

    // ═══════════════════════════════════════════════════════════
    // Multiple commands in Then/Else branches
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task Multiple_commands_in_then_and_else()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.When(args, x => x.Active).Truthy()
                .Then(t =>
                {
                    t.Element("panel").Show();
                    t.Element("panel").AddClass("active");
                    t.Element("status").SetText("enabled");
                })
                .Else(e =>
                {
                    e.Element("panel").Hide();
                    e.Element("panel").RemoveClass("active");
                    e.Element("status").SetText("disabled");
                });
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("enabled"));
        Assert.That(json, Does.Contain("disabled"));
        return VerifyJson(json);
    }

    // ═══════════════════════════════════════════════════════════
    // Regression — existing patterns must not break
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task Single_when_still_works()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Element("echo").SetText("before");
            p.When(args, x => x.Value).Eq("hello")
                .Then(t => t.Element("r1").SetText("yes"))
                .Else(e => e.Element("r1").SetText("no"));
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("before"));
        Assert.That(json, Does.Contain("r1"));
        return VerifyJson(json);
    }

    [Test]
    public Task Commands_only_still_produces_sequential()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Element("a").SetText("one");
            p.Element("b").SetText("two");
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("sequential"));
        Assert.That(json, Does.Not.Contain("conditional"));
        return VerifyJson(json);
    }

    [Test]
    public Task DomReady_with_mixed_commands_and_condition()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Element("loader").Hide();
            p.Dispatch("page-ready");
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("dom-ready"));
        Assert.That(json, Does.Contain("loader"));
        Assert.That(json, Does.Contain("page-ready"));
        return VerifyJson(json);
    }

    // ═══════════════════════════════════════════════════════════
    // Condition with Dispatch inside Then
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task Condition_then_dispatches_event()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.When(args, x => x.Active).Truthy()
                .Then(t =>
                {
                    t.Element("status").SetText("active");
                    t.Dispatch("user-activated");
                })
                .Else(e =>
                {
                    e.Element("status").SetText("inactive");
                    e.Dispatch("user-deactivated");
                });
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("user-activated"));
        Assert.That(json, Does.Contain("user-deactivated"));
        return VerifyJson(json);
    }

    // ═══════════════════════════════════════════════════════════
    // Mixed event-arg and component source conditions
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task Event_arg_condition_followed_by_component_condition()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Element("echo").SetText(args, x => x.Value);
            p.When(args, x => x.Value).Eq("special")
                .Then(t => t.Element("highlight").Show())
                .Else(e => e.Element("highlight").Hide());
            p.When(args, x => x.Count).Gte(10)
                .Then(t => t.Element("badge").SetText("many"))
                .Else(e => e.Element("badge").SetText("few"));
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("echo"));
        Assert.That(json, Does.Contain("highlight"));
        Assert.That(json, Does.Contain("badge"));
        Assert.That(json, Does.Contain("special"));
        return VerifyJson(json);
    }

    // ═══════════════════════════════════════════════════════════
    // Conditions inside HTTP OnSuccess
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task Condition_inside_http_on_success_with_surrounding_commands()
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

    [Test]
    public Task ElseIf_chain_inside_http_on_success()
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

    // ═══════════════════════════════════════════════════════════
    // Conditions after HTTP
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task Condition_after_http_response_block()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Post("/api/save")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("status").SetText("saved");
             }));
            p.When(args, x => x.Active).Truthy()
                .Then(t => t.Element("active-badge").Show())
                .Else(e => e.Element("active-badge").Hide());
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("saved"), "OnSuccess handler present");
        Assert.That(json, Does.Contain("active-badge"), "Condition after HTTP present");
        return VerifyJson(json);
    }

    [Test]
    public Task Multiple_conditions_after_http()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Post("/api/save")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("status").SetText("saved");
             }));
            p.When(args, x => x.Active).Truthy()
                .Then(t => t.Element("badge").Show());
            p.When(args, x => x.Category).Eq("premium")
                .Then(t => t.Element("premium-label").Show())
                .Else(e => e.Element("premium-label").Hide());
            p.Element("footer").SetText("complete");
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("saved"));
        Assert.That(json, Does.Contain("badge"));
        Assert.That(json, Does.Contain("premium-label"));
        Assert.That(json, Does.Contain("complete"));
        return VerifyJson(json);
    }

    // ═══════════════════════════════════════════════════════════
    // Multiple ElseIf chains with commands between them
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task Two_elseif_chains_with_commands_between()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.When(args, x => x.Count).Gt(100)
                .Then(t => t.Element("tier").SetText("gold"))
                .ElseIf(args, x => x.Count).Gt(50)
                .Then(t => t.Element("tier").SetText("silver"))
                .Else(e => e.Element("tier").SetText("bronze"));
            p.Element("divider").Show();
            p.Dispatch("tier-evaluated");
            p.When(args, x => x.Category).Eq("vip")
                .Then(t => t.Element("vip-badge").Show())
                .ElseIf(args, x => x.Category).Eq("member")
                .Then(t => t.Element("member-badge").Show())
                .Else(e => e.Element("guest-badge").Show());
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("gold"));
        Assert.That(json, Does.Contain("silver"));
        Assert.That(json, Does.Contain("bronze"));
        Assert.That(json, Does.Contain("divider"));
        Assert.That(json, Does.Contain("tier-evaluated"));
        Assert.That(json, Does.Contain("vip-badge"));
        Assert.That(json, Does.Contain("member-badge"));
        Assert.That(json, Does.Contain("guest-badge"));
        return VerifyJson(json);
    }

    [Test]
    public Task Three_elseif_chains_interleaved_with_commands()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Element("header").SetText("evaluating");
            p.When(args, x => x.Count).Gt(100)
                .Then(t => t.Element("count-tier").SetText("high"))
                .ElseIf(args, x => x.Count).Gt(10)
                .Then(t => t.Element("count-tier").SetText("medium"))
                .Else(e => e.Element("count-tier").SetText("low"));
            p.Dispatch("count-done");
            p.When(args, x => x.Active).Truthy()
                .Then(t => t.Element("active-ind").Show())
                .Else(e => e.Element("active-ind").Hide());
            p.Element("mid").SetText("between");
            p.When(args, x => x.Value).Eq("urgent")
                .Then(t =>
                {
                    t.Element("priority").SetText("URGENT");
                    t.Element("priority").AddClass("text-red");
                })
                .ElseIf(args, x => x.Value).Eq("normal")
                .Then(t => t.Element("priority").SetText("normal"))
                .Else(e => e.Element("priority").SetText("low"));
            p.Element("footer").SetText("complete");
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("evaluating"));
        Assert.That(json, Does.Contain("count-tier"));
        Assert.That(json, Does.Contain("count-done"));
        Assert.That(json, Does.Contain("active-ind"));
        Assert.That(json, Does.Contain("between"));
        Assert.That(json, Does.Contain("URGENT"));
        Assert.That(json, Does.Contain("priority"));
        Assert.That(json, Does.Contain("complete"));
        return VerifyJson(json);
    }

    // ═══════════════════════════════════════════════════════════
    // HTTP between conditions
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task Http_between_two_condition_blocks()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.When(args, x => x.Active).Truthy()
                .Then(t => t.Element("status").SetText("active"))
                .Else(e => e.Element("status").SetText("inactive"));
            p.Post("/api/audit")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("audit-result").SetText("logged");
             }));
            p.When(args, x => x.Count).Gt(0)
                .Then(t => t.Element("count-badge").Show())
                .Else(e => e.Element("count-badge").Hide());
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("active"));
        Assert.That(json, Does.Contain("inactive"));
        Assert.That(json, Does.Contain("audit-result"));
        Assert.That(json, Does.Contain("count-badge"));
        return VerifyJson(json);
    }
}
