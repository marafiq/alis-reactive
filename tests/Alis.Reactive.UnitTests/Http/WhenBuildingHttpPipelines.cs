namespace Alis.Reactive.UnitTests;

public class ApiResponse
{
    public string? Message { get; set; }
    public int Count { get; set; }
}

public class SaveResponse
{
    public string? Id { get; set; }
    public bool Success { get; set; }
}

public class HttpPayload
{
    public string? Name { get; set; }
    public int Amount { get; set; }
}

/// <summary>
/// BDD tests for the HTTP pipeline — every verb, gather pattern, response handler,
/// chained request, parallel request, validation, content type, and pre-fetch command.
/// Each test verifies the plan JSON shape + schema validity.
/// </summary>
[TestFixture]
public class WhenBuildingHttpPipelines : PlanTestBase
{
    // ═══════════════════════════════════════════════════════════
    // HTTP Verbs — GET, POST, PUT, DELETE
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task Get_produces_http_reaction()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Get("/api/residents")
             .Response(r => r.OnSuccess(s => s.Element("result").SetText("loaded"))));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Post_with_static_gather()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Post("/api/save", g => g.Static("name", "John"))
             .Response(r => r.OnSuccess(s => s.Element("result").SetText("saved"))));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Put_with_static_gather()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Put("/api/update", g => g.Static("id", "42").Static("name", "Jane"))
             .Response(r => r.OnSuccess(s => s.Element("result").SetText("updated"))));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Delete_produces_http_reaction()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Delete("/api/residents/42")
             .Response(r => r.OnSuccess(s => s.Element("result").SetText("deleted"))));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ═══════════════════════════════════════════════════════════
    // Response handlers — OnSuccess, OnError, typed
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task OnSuccess_untyped()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Get("/api/data")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("status").SetText("ok");
                 s.Element("panel").Show();
             })));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task OnSuccess_typed_response()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Get("/api/data")
             .Response(r => r.OnSuccess<ApiResponse>((json, s) =>
             {
                 s.Element("message").SetText(json, x => x.Message);
                 s.Element("count").SetText(json, x => x.Count);
             })));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task OnError_with_status_code()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Post("/api/save", g => g.Static("name", ""))
             .Response(r => r
                .OnSuccess(s => s.Element("result").SetText("saved"))
                .OnError(400, e => e.Element("error").SetText("validation failed"))
                .OnError(500, e => e.Element("error").SetText("server error"))));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task OnSuccess_and_OnError_combined()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<HttpPayload>("submit", (args, p) =>
            p.Post("/api/submit", g => g.Static("data", "test"))
             .Response(r => r
                .OnSuccess<SaveResponse>((json, s) =>
                {
                    s.Element("id").SetText(json, x => x.Id);
                    s.Element("status").SetText("success");
                })
                .OnError(422, e =>
                {
                    e.Element("status").SetText("invalid");
                    e.ValidationErrors("form");
                })));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ═══════════════════════════════════════════════════════════
    // Gather — Static, IncludeAll
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task Gather_with_include_all()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Post("/api/save", g => g.IncludeAll())
             .Response(r => r.OnSuccess(s => s.Element("result").SetText("saved"))));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Gather_with_multiple_statics()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Post("/api/save", g => g
                .Static("firstName", "John")
                .Static("lastName", "Doe")
                .Static("age", 82))
             .Response(r => r.OnSuccess(s => s.Element("result").SetText("saved"))));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ═══════════════════════════════════════════════════════════
    // WhileLoading
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task WhileLoading_shows_spinner()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Get("/api/slow")
             .WhileLoading(l =>
             {
                 l.Element("spinner").Show();
                 l.Element("content").Hide();
             })
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("spinner").Hide();
                 s.Element("content").Show();
             })));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ═══════════════════════════════════════════════════════════
    // AsFormData
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task AsFormData_sets_content_type()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Post("/api/upload", g => g.IncludeAll())
             .AsFormData()
             .Response(r => r.OnSuccess(s => s.Element("result").SetText("uploaded"))));
        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("form-data"));
        return VerifyJson(json);
    }

    // ═══════════════════════════════════════════════════════════
    // Chained requests
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task Chained_get_after_post()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Post("/api/save", g => g.Static("name", "John"))
             .Response(r => r
                .OnSuccess(s => s.Element("save-status").SetText("saved"))
                .Chained(c => c.Get("/api/list")
                    .Response(cr => cr.OnSuccess(s => s.Element("list").SetText("refreshed"))))));
        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("chained"));
        return VerifyJson(json);
    }

    // ═══════════════════════════════════════════════════════════
    // Parallel requests
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task Parallel_two_gets()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Parallel(
                a => a.Get("/api/residents").Response(r => r.OnSuccess(s => s.Element("r1").SetText("loaded"))),
                b => b.Get("/api/facilities").Response(r => r.OnSuccess(s => s.Element("r2").SetText("loaded")))
            ).OnAllSettled(s => s.Element("spinner").Hide()));
        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("parallel-http"));
        return VerifyJson(json);
    }

    [Test]
    public Task Parallel_with_pre_fetch_commands()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Element("spinner").Show();
            p.Parallel(
                a => a.Get("/api/a").Response(r => r.OnSuccess(s => s.Element("a").SetText("done"))),
                b => b.Get("/api/b").Response(r => r.OnSuccess(s => s.Element("b").SetText("done")))
            );
        });
        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("spinner"));
        Assert.That(json, Does.Contain("parallel-http"));
        return VerifyJson(json);
    }

    // ═══════════════════════════════════════════════════════════
    // Pre-fetch commands (commands before HTTP)
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task Commands_before_http_become_preFetch()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Element("spinner").Show();
            p.Element("status").SetText("loading...");
            p.Get("/api/data")
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element("spinner").Hide();
                 s.Element("status").SetText("loaded");
             }));
        });
        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("spinner"));
        Assert.That(json, Does.Contain("loading..."));
        return VerifyJson(json);
    }

    // ═══════════════════════════════════════════════════════════
    // Into (HTML content injection)
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task Into_injects_html_response()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Get("/api/partial")
             .Response(r => r.OnSuccess(s => s.Into("container"))));
        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("into"));
        Assert.That(json, Does.Contain("container"));
        return VerifyJson(json);
    }

    // ═══════════════════════════════════════════════════════════
    // Conditions mixed with HTTP
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task Typed_response_with_multiple_field_reads()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Get("/api/data")
             .Response(r => r.OnSuccess<ApiResponse>((json, s) =>
             {
                 s.Element("message").SetText(json, x => x.Message);
                 s.Element("count").SetText(json, x => x.Count);
             })));
        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("message"));
        Assert.That(json, Does.Contain("count"));
        Assert.That(json, Does.Contain("responseBody"));
        return VerifyJson(json);
    }

    [Test]
    public Task Condition_in_OnError_handler()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<HttpPayload>("submit", (args, p) =>
            p.Post("/api/save", g => g.Static("data", "x"))
             .Response(r => r
                .OnSuccess(s => s.Element("ok").Show())
                .OnError(400, e =>
                {
                    e.Element("error").Show();
                    e.ValidationErrors("form");
                })));
        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("validation-errors"));
        return VerifyJson(json);
    }

    // ═══════════════════════════════════════════════════════════
    // Full real-world workflow
    // ═══════════════════════════════════════════════════════════

    [Test]
    public Task Full_workflow_post_with_gather_validation_loading_success_error()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<HttpPayload>("save-resident", (args, p) =>
        {
            p.Post("/api/residents", g => g
                .Static("name", "Margaret")
                .IncludeAll())
             .WhileLoading(l =>
             {
                 l.Element("spinner").Show();
                 l.Element("save-btn").Hide();
             })
             .Response(r => r
                .OnSuccess<SaveResponse>((json, s) =>
                {
                    s.Element("spinner").Hide();
                    s.Element("save-btn").Show();
                    s.Element("result").SetText(json, x => x.Id);
                })
                .OnError(400, e =>
                {
                    e.Element("spinner").Hide();
                    e.Element("save-btn").Show();
                    e.ValidationErrors("resident-form");
                }));
        });
        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("Margaret"));
        Assert.That(json, Does.Contain("spinner"));
        Assert.That(json, Does.Contain("resident-form"));
        return VerifyJson(json);
    }

    [Test]
    public Task Full_workflow_domready_get_into_partial()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Element("loader").Show();
            p.Get("/api/dashboard")
             .WhileLoading(l => l.Element("shimmer").Show())
             .Response(r => r
                .OnSuccess(s =>
                {
                    s.Element("loader").Hide();
                    s.Element("shimmer").Hide();
                    s.Into("dashboard-content");
                }));
        });
        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("dashboard-content"));
        return VerifyJson(json);
    }
}
