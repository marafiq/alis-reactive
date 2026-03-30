namespace Alis.Reactive.UnitTests;

[TestFixture]
public class WhenRequestingFromServer : PlanTestBase
{
    [Test]
    public Task Get_request_flows_to_plan() =>
        VerifyJson(Build(p =>
            p.Get("/api/test")
             .Response(r => r
                .OnSuccess(s => s.Element("result").SetText("loaded"))
             )
        ).Render());

    [Test]
    public Task Post_body_includes_gathered_values() =>
        VerifyJson(Build(p =>
            p.Post("/api/test", g => g.Static("key", "val"))
             .Response(r => r
                .OnSuccess(s => s.Element("result").SetText("saved"))
             )
        ).Render());

    [Test]
    public Task Loading_feedback_runs_during_request() =>
        VerifyJson(Build(p =>
            p.Get("/api/test")
             .WhileLoading(l => l.Element("spinner").Show())
             .Response(r => r
                .OnSuccess(s => s.Element("spinner").Hide())
             )
        ).Render());

    [Test]
    public Task Error_routes_by_status_code() =>
        VerifyJson(Build(p =>
            p.Post("/api/save", g => g.Static("name", "test"))
             .Response(r => r
                .OnSuccess(s => s.Element("result").SetText("ok"))
                .OnError(400, e => e.Element("error").SetText("bad request"))
                .OnError(500, e => e.Element("error").SetText("server error"))
             )
        ).Render());

    [Test]
    public Task Chained_request_fires_after_success() =>
        VerifyJson(Build(p =>
            p.Get("/api/residents")
             .Response(r => r
                .OnSuccess(s => s.Element("residents").SetText("loaded"))
                .Chained(c => c
                    .Get("/api/facilities")
                    .Response(r2 => r2
                        .OnSuccess(s2 => s2.Element("facilities").SetText("loaded"))
                    )
                )
             )
        ).Render());

    [Test]
    public Task Parallel_requests_fire_concurrently() =>
        VerifyJson(Build(p =>
            p.Parallel(
                a => a.Get("/api/residents"),
                b => b.Get("/api/facilities")
            ).OnAllSettled(s => s.Element("result").SetText("all loaded"))
        ).Render());

    [Test]
    public Task Commands_before_request_become_preFetch() =>
        VerifyJson(Build(p =>
        {
            p.Element("spinner").Show();
            p.Get("/api/test")
             .Response(r => r
                .OnSuccess(s => s.Element("spinner").Hide())
             );
        }).Render());

    // ── Schema validation for all HTTP patterns ──

    [Test]
    public void Get_request_conforms_to_schema()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Get("/api/test")
             .Response(r => r.OnSuccess(s => s.Element("x").SetText("ok"))));
        AssertSchemaValid(plan.Render());
    }

    [Test]
    public void Post_with_gather_conforms_to_schema()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Post("/api/save", g => g.Static("key", "val"))
             .Response(r => r
                .OnSuccess(s => s.Element("x").SetText("ok"))
                .OnError(400, e => e.Element("x").SetText("err"))));
        AssertSchemaValid(plan.Render());
    }

    [Test]
    public void Chained_conforms_to_schema()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Get("/api/a")
             .Response(r => r
                .OnSuccess(s => s.Dispatch("a-done"))
                .Chained(c => c
                    .Get("/api/b")
                    .Response(r2 => r2.OnSuccess(s2 => s2.Dispatch("b-done"))))));
        AssertSchemaValid(plan.Render());
    }

    [Test]
    public void Parallel_conforms_to_schema()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Parallel(
                a => a.Get("/api/a"),
                b => b.Get("/api/b")
            ).OnAllSettled(s => s.Dispatch("all-done")));
        AssertSchemaValid(plan.Render());
    }

    [Test]
    public void PreFetch_conforms_to_schema()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Element("spinner").Show();
            p.Get("/api/test").Response(r => r.OnSuccess(s => s.Element("spinner").Hide()));
        });
        AssertSchemaValid(plan.Render());
    }

    // ── Validation descriptor in plan ──

    [Test]
    public Task Post_with_validation_includes_descriptor() =>
        VerifyJson(Build(p =>
            p.Post("/api/save", g => g.Static("name", "test"))
             .Validate(new Validation.ValidationDescriptor("testForm", new List<Validation.ValidationField>
             {
                 new("Name", new List<Validation.ValidationRule>
                 {
                     new("required", "Name is required"),
                 }),
                 new("Email", new List<Validation.ValidationRule>
                 {
                     new("required", "Email is required"),
                     new("email", "Must be a valid email"),
                 }),
             }))
             .Response(r => r
                .OnSuccess(s => s.Element("result").SetText("saved"))
                .OnError(400, e => e.ValidationErrors("testForm")))
        ).Render());

    [Test]
    public void Post_with_validation_conforms_to_schema()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Post("/api/save", g => g.Static("name", "test"))
             .Validate(new Validation.ValidationDescriptor("testForm", new List<Validation.ValidationField>
             {
                 new("Name", new List<Validation.ValidationRule>
                 {
                     new("required", "Name is required"),
                 }),
             }))
             .Response(r => r
                .OnSuccess(s => s.Element("x").SetText("ok"))
                .OnError(400, e => e.ValidationErrors("testForm"))));
        AssertSchemaValid(plan.Render());
    }

    private static ReactivePlan<TestModel> Build(Action<Builders.PipelineBuilder<TestModel>> pipeline)
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(pipeline);
        return plan;
    }
}
