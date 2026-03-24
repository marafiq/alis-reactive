namespace Alis.Reactive.UnitTests;

/// <summary>
/// Conditions compose freely with HTTP blocks at the outer pipeline level.
/// The pipeline segments: condition → HTTP → condition → HTTP. Each segment
/// becomes a separate entry. Covers all HTTP verbs, Chained, Parallel,
/// Confirm, compound guards (And/Or/Not), and gather.
/// </summary>
[TestFixture]
public class WhenMixingConditionsWithHttp : PlanTestBase
{
    // ── Condition after HTTP ──

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

    // ── HTTP between conditions ──

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

    // ── Full interleaving — every segment type ──

    [Test]
    public Task Full_pipeline_commands_condition_http_condition_commands()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Element("header").SetText("start");
            p.When(args, x => x.Active).Truthy()
                .Then(t => t.Element("badge").Show());
            p.Element("loading").SetText("please wait");
            p.Post("/api/submit")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("result").SetText("ok");
             }));
            p.When(args, x => x.Count).Gt(0)
                .Then(t => t.Element("count").Show())
                .Else(e => e.Element("count").Hide());
            p.Element("footer").SetText("done");
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("start"));
        Assert.That(json, Does.Contain("badge"));
        Assert.That(json, Does.Contain("please wait"));
        Assert.That(json, Does.Contain("/api/submit"));
        Assert.That(json, Does.Contain("count"));
        Assert.That(json, Does.Contain("done"));
        return VerifyJson(json);
    }

    // ── ElseIf chains around HTTP ──

    [Test]
    public Task ElseIf_chain_after_http()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Get("/api/status")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("status").SetText("fetched");
             }));
            p.When(args, x => x.Count).Gt(100)
                .Then(t => t.Element("tier").SetText("gold"))
                .ElseIf(args, x => x.Count).Gt(50)
                .Then(t => t.Element("tier").SetText("silver"))
                .ElseIf(args, x => x.Count).Gt(10)
                .Then(t => t.Element("tier").SetText("bronze"))
                .Else(e => e.Element("tier").SetText("none"));
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("fetched"));
        Assert.That(json, Does.Contain("gold"));
        Assert.That(json, Does.Contain("silver"));
        Assert.That(json, Does.Contain("bronze"));
        Assert.That(json, Does.Contain("none"));
        return VerifyJson(json);
    }

    [Test]
    public Task ElseIf_chain_then_http()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.When(args, x => x.Value).Eq("admin")
                .Then(t => t.Element("role").SetText("Administrator"))
                .ElseIf(args, x => x.Value).Eq("user")
                .Then(t => t.Element("role").SetText("Standard User"))
                .Else(e => e.Element("role").SetText("Guest"));
            p.Post("/api/log")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("log-status").SetText("logged");
             }));
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("Administrator"));
        Assert.That(json, Does.Contain("Standard User"));
        Assert.That(json, Does.Contain("Guest"));
        Assert.That(json, Does.Contain("logged"));
        return VerifyJson(json);
    }

    // ── Pre-commands + HTTP + condition ──

    [Test]
    public Task Pre_commands_http_then_condition()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Element("spinner").Show();
            p.Dispatch("loading-started");
            p.Post("/api/data")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("spinner").Hide();
             }));
            p.When(args, x => x.Active).Falsy()
                .Then(t => t.Element("warning").SetText("account disabled"));
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("spinner"));
        Assert.That(json, Does.Contain("loading-started"));
        Assert.That(json, Does.Contain("/api/data"));
        Assert.That(json, Does.Contain("account disabled"));
        return VerifyJson(json);
    }

    // ── Multiple HTTP blocks separated by conditions ──

    [Test]
    public Task Two_http_blocks_with_condition_between()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Post("/api/validate")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("validation").SetText("valid");
             }));
            p.When(args, x => x.Active).Truthy()
                .Then(t => t.Element("proceed-badge").Show());
            p.Post("/api/save")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("save-result").SetText("saved");
             }));
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("/api/validate"));
        Assert.That(json, Does.Contain("valid"));
        Assert.That(json, Does.Contain("proceed-badge"));
        Assert.That(json, Does.Contain("/api/save"));
        Assert.That(json, Does.Contain("saved"));
        return VerifyJson(json);
    }

    // ── Different HTTP verbs interleaved ──

    [Test]
    public Task Get_condition_put_condition_delete()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Get("/api/resource")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("resource").SetText("loaded");
             }));
            p.When(args, x => x.Value).NotEmpty()
                .Then(t => t.Element("has-value").Show());
            p.Put("/api/resource", g => g.Static("name", "updated"))
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("update-result").SetText("updated");
             }));
            p.When(args, x => x.Count).Gte(1)
                .Then(t => t.Element("has-items").Show());
            p.Delete("/api/resource")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("delete-result").SetText("removed");
             }));
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("GET"));
        Assert.That(json, Does.Contain("has-value"));
        Assert.That(json, Does.Contain("PUT"));
        Assert.That(json, Does.Contain("updated"));
        Assert.That(json, Does.Contain("has-items"));
        Assert.That(json, Does.Contain("DELETE"));
        Assert.That(json, Does.Contain("removed"));
        return VerifyJson(json);
    }

    // ── HTTP with OnError + condition ──

    [Test]
    public Task Http_with_on_error_then_condition()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Post("/api/save")
             .Response(r => r
                 .OnSuccess(s => s.Element("status").SetText("saved"))
                 .OnError(400, e => e.Element("error").SetText("validation failed"))
                 .OnError(500, e => e.Element("error").SetText("server error")));
            p.When(args, x => x.Active).Truthy()
                .Then(t => t.Element("retry-hint").SetText("try again"))
                .Else(e => e.Element("retry-hint").Hide());
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("saved"));
        Assert.That(json, Does.Contain("validation failed"));
        Assert.That(json, Does.Contain("server error"));
        Assert.That(json, Does.Contain("retry-hint"));
        return VerifyJson(json);
    }

    // ── Dispatch interleaving ──

    [Test]
    public Task Dispatch_http_dispatch_condition_dispatch()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Dispatch("step-1");
            p.Post("/api/action")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("action-result").SetText("acted");
             }));
            p.Dispatch("step-2");
            p.When(args, x => x.Active).Truthy()
                .Then(t => t.Dispatch("step-3-active"))
                .Else(e => e.Dispatch("step-3-inactive"));
            p.Dispatch("step-4");
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("step-1"));
        Assert.That(json, Does.Contain("acted"));
        Assert.That(json, Does.Contain("step-2"));
        Assert.That(json, Does.Contain("step-3-active"));
        Assert.That(json, Does.Contain("step-3-inactive"));
        Assert.That(json, Does.Contain("step-4"));
        return VerifyJson(json);
    }

    // ── Then-only conditions around HTTP ──

    [Test]
    public Task Three_then_only_conditions_around_http()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.When(args, x => x.Active).Truthy()
                .Then(t => t.Element("flag-1").Show());
            p.Post("/api/process")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("process-result").SetText("processed");
             }));
            p.When(args, x => x.Count).Gt(10)
                .Then(t => t.Element("flag-2").Show());
            p.When(args, x => x.Value).NotNull()
                .Then(t => t.Element("flag-3").Show());
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("flag-1"));
        Assert.That(json, Does.Contain("processed"));
        Assert.That(json, Does.Contain("flag-2"));
        Assert.That(json, Does.Contain("flag-3"));
        return VerifyJson(json);
    }

    // ── Negated condition after HTTP ──

    [Test]
    public Task Negated_condition_after_http()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Get("/api/settings")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("settings").SetText("loaded");
             }));
            p.When(args, x => x.Active).Truthy().Not()
                .Then(t => t.Element("disabled-banner").Show())
                .Else(e => e.Element("disabled-banner").Hide());
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("settings"));
        Assert.That(json, Does.Contain("disabled-banner"));
        Assert.That(json, Does.Contain("not"));
        return VerifyJson(json);
    }

    // ── HTTP with gather + condition ──

    [Test]
    public Task Http_with_static_gather_then_condition()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Post("/api/submit", g =>
            {
                g.Static("action", "save");
                g.Static("version", "2");
            })
            .Response(r => r.OnSuccess(s =>
            {
                s.Element("submit-result").SetText("submitted");
            }));
            p.When(args, x => x.Category).Eq("urgent")
                .Then(t =>
                {
                    t.Element("urgency").SetText("URGENT");
                    t.Element("urgency").AddClass("text-red");
                })
                .Else(e => e.Element("urgency").SetText("normal"));
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("action"));
        Assert.That(json, Does.Contain("save"));
        Assert.That(json, Does.Contain("submitted"));
        Assert.That(json, Does.Contain("URGENT"));
        Assert.That(json, Does.Contain("normal"));
        return VerifyJson(json);
    }

    // ── Confirm guard + HTTP ──

    [Test]
    public Task Confirm_guard_then_http()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Confirm("Are you sure?")
                .Then(t => t.Element("confirmed").SetText("yes"));
            p.Post("/api/delete")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("delete-result").SetText("deleted");
             }));
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("Are you sure?"));
        Assert.That(json, Does.Contain("confirmed"));
        Assert.That(json, Does.Contain("/api/delete"));
        Assert.That(json, Does.Contain("deleted"));
        return VerifyJson(json);
    }

    [Test]
    public Task Http_then_confirm_guard()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Post("/api/check")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("check-result").SetText("checked");
             }));
            p.Confirm("Proceed with dangerous action?")
                .Then(t => t.Dispatch("danger-confirmed"));
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("/api/check"));
        Assert.That(json, Does.Contain("checked"));
        Assert.That(json, Does.Contain("Proceed with dangerous action?"));
        Assert.That(json, Does.Contain("danger-confirmed"));
        return VerifyJson(json);
    }

    // ── Compound guards (And/Or/Not) around HTTP ──

    [Test]
    public Task Compound_and_condition_after_http()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Get("/api/profile")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("profile").SetText("loaded");
             }));
            p.When(args, x => x.Active).Truthy()
                .And(args, x => x.Count).Gt(0)
                .Then(t => t.Element("active-with-items").Show())
                .Else(e => e.Element("active-with-items").Hide());
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("profile"));
        Assert.That(json, Does.Contain("active-with-items"));
        Assert.That(json, Does.Contain("truthy"));
        Assert.That(json, Does.Contain("gt"));
        return VerifyJson(json);
    }

    [Test]
    public Task Compound_or_condition_between_two_http_blocks()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Get("/api/data")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("data").SetText("fetched");
             }));
            p.When(args, x => x.Value).Eq("override")
                .Or(args, x => x.Active).Truthy()
                .Then(t => t.Element("proceed").Show());
            p.Post("/api/process")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("processed").SetText("done");
             }));
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("fetched"));
        Assert.That(json, Does.Contain("proceed"));
        Assert.That(json, Does.Contain("override"));
        Assert.That(json, Does.Contain("processed"));
        return VerifyJson(json);
    }

    [Test]
    public Task Or_guard_after_http_with_not()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Get("/api/permissions")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("perms").SetText("loaded");
             }));
            p.When(args, x => x.Value).Eq("admin")
                .Or(args, x => x.Value).Eq("superadmin")
                .Not()
                .Then(t => t.Element("restricted").Show())
                .Else(e => e.Element("restricted").Hide());
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("perms"));
        Assert.That(json, Does.Contain("restricted"));
        Assert.That(json, Does.Contain("not"), "Not guard wraps the Or");
        Assert.That(json, Does.Contain("any"), "Or guard produces 'any' composite");
        return VerifyJson(json);
    }

    [Test]
    public Task And_guard_between_http_and_condition()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Post("/api/auth")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("auth-status").SetText("authenticated");
             }));
            p.When(args, x => x.Active).Truthy()
                .And(args, x => x.Value).NotEmpty()
                .And(args, x => x.Count).Gte(1)
                .Then(t =>
                {
                    t.Element("full-access").Show();
                    t.Element("full-access").SetText("all checks passed");
                })
                .Else(e => e.Element("full-access").Hide());
            p.When(args, x => x.Category).In("premium", "enterprise")
                .Then(t => t.Element("tier-badge").Show());
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("authenticated"));
        Assert.That(json, Does.Contain("full-access"));
        Assert.That(json, Does.Contain("all checks passed"));
        Assert.That(json, Does.Contain("tier-badge"));
        Assert.That(json, Does.Contain("all"), "Triple-And produces flat AllGuard");
        Assert.That(json, Does.Contain("in"));
        return VerifyJson(json);
    }

    // ── Chained HTTP + conditions ──

    [Test]
    public Task Chained_http_then_condition()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Post("/api/validate")
             .Response(r => r
                 .OnSuccess(s => s.Element("validate-result").SetText("valid"))
                 .Chained(chain => chain.Post("/api/save")
                     .Response(cr => cr.OnSuccess(cs =>
                     {
                         cs.Element("save-result").SetText("saved");
                     }))));
            p.When(args, x => x.Active).Truthy()
                .Then(t => t.Element("post-chain-badge").Show())
                .Else(e => e.Element("post-chain-badge").Hide());
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("/api/validate"));
        Assert.That(json, Does.Contain("valid"));
        Assert.That(json, Does.Contain("/api/save"));
        Assert.That(json, Does.Contain("saved"));
        Assert.That(json, Does.Contain("post-chain-badge"));
        return VerifyJson(json);
    }

    [Test]
    public Task Condition_then_chained_http()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.When(args, x => x.Value).NotEmpty()
                .Then(t => t.Element("pre-check").SetText("has value"))
                .Else(e => e.Element("pre-check").SetText("empty"));
            p.Get("/api/lookup")
             .Response(r => r
                 .OnSuccess(s => s.Element("lookup-result").SetText("found"))
                 .Chained(chain => chain.Post("/api/enrich")
                     .Response(cr => cr.OnSuccess(cs =>
                     {
                         cs.Element("enrich-result").SetText("enriched");
                     }))));
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("has value"));
        Assert.That(json, Does.Contain("/api/lookup"));
        Assert.That(json, Does.Contain("found"));
        Assert.That(json, Does.Contain("/api/enrich"));
        Assert.That(json, Does.Contain("enriched"));
        return VerifyJson(json);
    }

    [Test]
    public Task Condition_chained_http_condition()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.When(args, x => x.Active).Truthy()
                .Then(t => t.Element("auth-status").SetText("authorized"));
            p.Post("/api/create")
             .Response(r => r
                 .OnSuccess(s => s.Element("create-result").SetText("created"))
                 .Chained(chain => chain.Get("/api/refresh")
                     .Response(cr => cr.OnSuccess(cs =>
                     {
                         cs.Element("refresh-result").SetText("refreshed");
                     }))));
            p.When(args, x => x.Count).Gt(0)
                .Then(t => t.Element("item-count").Show())
                .Else(e => e.Element("item-count").Hide());
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("authorized"));
        Assert.That(json, Does.Contain("/api/create"));
        Assert.That(json, Does.Contain("created"));
        Assert.That(json, Does.Contain("/api/refresh"));
        Assert.That(json, Does.Contain("refreshed"));
        Assert.That(json, Does.Contain("item-count"));
        return VerifyJson(json);
    }

    // ── Parallel HTTP + conditions ──

    [Test]
    public Task Parallel_http_with_different_verbs()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Parallel(
                post => post.SetVerb("POST").SetUrl("/api/save")
                    .Response(r => r.OnSuccess(s =>
                    {
                        s.Element("save-result").SetText("saved");
                    })),
                del => del.SetVerb("DELETE").SetUrl("/api/cleanup")
                    .Response(r => r.OnSuccess(s =>
                    {
                        s.Element("cleanup-result").SetText("cleaned");
                    }))
            );
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("/api/save"));
        Assert.That(json, Does.Contain("saved"));
        Assert.That(json, Does.Contain("/api/cleanup"));
        Assert.That(json, Does.Contain("cleaned"));
        Assert.That(json, Does.Contain("parallel"));
        return VerifyJson(json);
    }

    [Test]
    public Task Parallel_http_then_condition()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Parallel(
                get1 => get1.SetVerb("GET").SetUrl("/api/users"),
                get2 => get2.SetVerb("GET").SetUrl("/api/roles")
            );
            p.When(args, x => x.Active).Truthy()
                .Then(t => t.Element("access-granted").Show())
                .Else(e => e.Element("access-granted").Hide());
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("/api/users"));
        Assert.That(json, Does.Contain("/api/roles"));
        Assert.That(json, Does.Contain("access-granted"));
        return VerifyJson(json);
    }

    [Test]
    public Task Condition_then_parallel_http()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.When(args, x => x.Active).Truthy()
                .Then(t => t.Element("status").SetText("active"))
                .Else(e => e.Element("status").SetText("inactive"));
            p.Parallel(
                req1 => req1.SetVerb("POST").SetUrl("/api/audit")
                    .Response(r => r.OnSuccess(s =>
                    {
                        s.Element("audit").SetText("logged");
                    })),
                req2 => req2.SetVerb("POST").SetUrl("/api/notify")
                    .Response(r => r.OnSuccess(s =>
                    {
                        s.Element("notify").SetText("sent");
                    }))
            );
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("active"));
        Assert.That(json, Does.Contain("inactive"));
        Assert.That(json, Does.Contain("/api/audit"));
        Assert.That(json, Does.Contain("/api/notify"));
        Assert.That(json, Does.Contain("parallel"));
        return VerifyJson(json);
    }

    [Test]
    public Task Condition_parallel_condition()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.When(args, x => x.Value).Eq("go")
                .Then(t => t.Element("go-badge").Show());
            p.Parallel(
                r1 => r1.SetVerb("GET").SetUrl("/api/first"),
                r2 => r2.SetVerb("GET").SetUrl("/api/second")
            );
            p.When(args, x => x.Count).Gt(0)
                .Then(t => t.Element("has-results").Show())
                .Else(e => e.Element("has-results").Hide());
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("go-badge"));
        Assert.That(json, Does.Contain("/api/first"));
        Assert.That(json, Does.Contain("/api/second"));
        Assert.That(json, Does.Contain("has-results"));
        return VerifyJson(json);
    }

    // ── Stress test — maximum segment count ──

    [Test]
    public Task Seven_segments_alternating_types()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Element("s1").SetText("one");
            p.Dispatch("event-1");

            p.When(args, x => x.Active).Truthy()
                .Then(t => t.Element("s2").Show());

            p.Element("s3").SetText("three");

            p.Post("/api/s4")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("s4-result").SetText("four");
             }));

            p.When(args, x => x.Count).Gt(100)
                .Then(t => t.Element("s5").SetText("high"))
                .ElseIf(args, x => x.Count).Gt(0)
                .Then(t => t.Element("s5").SetText("low"))
                .Else(e => e.Element("s5").SetText("zero"));

            p.Delete("/api/s6")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("s6-result").SetText("six");
             }));

            p.When(args, x => x.Value).Contains("done")
                .Then(t => t.Element("s7").Show());
            p.Element("s8-final").SetText("complete");
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("one"));
        Assert.That(json, Does.Contain("event-1"));
        Assert.That(json, Does.Contain("s2"));
        Assert.That(json, Does.Contain("three"));
        Assert.That(json, Does.Contain("/api/s4"));
        Assert.That(json, Does.Contain("s5"));
        Assert.That(json, Does.Contain("high"));
        Assert.That(json, Does.Contain("/api/s6"));
        Assert.That(json, Does.Contain("s7"));
        Assert.That(json, Does.Contain("complete"));
        return VerifyJson(json);
    }

    // ── Realistic end-to-end workflow ──

    [Test]
    public Task Realistic_form_submit_workflow()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("submit-form", (args, p) =>
        {
            p.Element("submit-btn").AddClass("disabled");
            p.Element("spinner").Show();

            p.When(args, x => x.Value).IsEmpty()
                .Then(t =>
                {
                    t.Element("name-error").SetText("Name is required");
                    t.Element("name-error").Show();
                })
                .Else(e => e.Element("name-error").Hide());

            p.Post("/api/residents", g =>
            {
                g.Static("source", "form");
            })
            .Response(r => r
                .OnSuccess(s =>
                {
                    s.When(args, x => x.Count).Gt(100)
                        .Then(t => t.Element("capacity").SetText("at capacity"))
                        .ElseIf(args, x => x.Count).Gt(50)
                        .Then(t => t.Element("capacity").SetText("filling up"))
                        .Else(e => e.Element("capacity").SetText("available"));
                    s.Element("status").SetText("saved successfully");
                })
                .OnError(400, e =>
                {
                    e.Element("status").SetText("validation failed");
                    e.When(args, x => x.Category).Eq("duplicate")
                        .Then(t => t.Element("dup-warning").Show());
                })
                .OnError(500, e => e.Element("status").SetText("server error")));

            p.When(args, x => x.Active).Truthy()
                .And(args, x => x.Value).NotEmpty()
                .Then(t => t.Dispatch("resident-created"))
                .Else(e => e.Dispatch("resident-draft-saved"));

            p.Element("spinner").Hide();
            p.Element("submit-btn").RemoveClass("disabled");
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("disabled"));
        Assert.That(json, Does.Contain("spinner"));
        Assert.That(json, Does.Contain("Name is required"));
        Assert.That(json, Does.Contain("/api/residents"));
        Assert.That(json, Does.Contain("source"));
        Assert.That(json, Does.Contain("at capacity"));
        Assert.That(json, Does.Contain("filling up"));
        Assert.That(json, Does.Contain("available"));
        Assert.That(json, Does.Contain("saved successfully"));
        Assert.That(json, Does.Contain("validation failed"));
        Assert.That(json, Does.Contain("dup-warning"));
        Assert.That(json, Does.Contain("server error"));
        Assert.That(json, Does.Contain("resident-created"));
        Assert.That(json, Does.Contain("resident-draft-saved"));
        Assert.That(json, Does.Contain("submit-btn"));
        return VerifyJson(json);
    }
}
