using System.Text.Json;

namespace Alis.Reactive.UnitTests.Http;

/// <summary>
/// Verifies that the Finally stage on HTTP requests serializes to the
/// plan's "complete" field and validates against the reactive plan schema.
/// </summary>
[TestFixture]
public class WhenUsingFinally : PlanTestBase
{
    [Test]
    public void finally_produces_complete_in_plan_json()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/save")
             .WhileLoading(l => l.Element("spinner").Show())
             .Response(r => r.OnSuccess(s => s.Element("result").Show()))
             .Finally(f => f.Element("spinner").Hide());
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        Assert.That(planJson, Does.Contain("\"complete\""));
    }

    [Test]
    public void finally_with_multiple_commands_serializes_all()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/save")
             .Finally(f =>
             {
                 f.Element("spinner").Hide();
                 f.Element("overlay").Hide();
                 f.Element("form").RemoveClass("disabled");
             });
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        using var doc = JsonDocument.Parse(planJson);
        var request = doc.RootElement
            .GetProperty("behaviors")[0]
            .GetProperty("reaction")
            .GetProperty("request");

        Assert.That(request.TryGetProperty("complete", out var complete), Is.True);
        Assert.That(complete.GetArrayLength(), Is.EqualTo(3));
    }

    [Test]
    public void plan_without_finally_emits_empty_complete_array()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Get("/api/data")
             .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        // Null-design-smell elimination: Request.Complete is always present as a domain default.
        // Plans without Finally() emit "complete": [] — never omit the field.
        Assert.That(planJson, Does.Contain("\"complete\": []"));
    }

    [Test]
    public void finally_works_without_response_handlers()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/fire-and-forget")
             .WhileLoading(l => l.Element("spinner").Show())
             .Finally(f => f.Element("spinner").Hide());
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        // Null-design-smell elimination: before/success/error/complete are always present
        // as domain defaults (empty arrays when not populated). Finally() populates complete;
        // the other handlers remain empty but still appear in the JSON.
        Assert.That(planJson, Does.Contain("\"before\""));
        Assert.That(planJson, Does.Contain("\"complete\""));
        Assert.That(planJson, Does.Contain("\"success\": []"));
        Assert.That(planJson, Does.Contain("\"error\": []"));
    }

    [Test]
    public void finally_coexists_with_while_loading_and_all_response_handlers()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/save")
             .WhileLoading(l => l.Element("spinner").Show())
             .Response(r => r
                .OnSuccess(s => s.Element("result").SetText("OK"))
                .OnError(400, e => e.Element("error").SetText("Bad Request"))
                .OnError(500, e => e.Element("error").SetText("Server Error"))
             )
             .Finally(f => f.Element("spinner").Hide());
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        Assert.That(planJson, Does.Contain("\"before\""));
        Assert.That(planJson, Does.Contain("\"success\""));
        Assert.That(planJson, Does.Contain("\"error\""));
        Assert.That(planJson, Does.Contain("\"complete\""));
    }

    [Test]
    public void finally_with_conditions_serializes_correctly()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/save")
             .Finally(f =>
             {
                 f.When(f.FromUrl("status")).Eq("error")
                  .Then(t => t.Element("error-icon").Hide())
                  .Else(e => e.Element("success-icon").Show());
                 f.Element("spinner").Hide();
             });
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        Assert.That(planJson, Does.Contain("\"complete\""));
        Assert.That(planJson, Does.Contain("\"kind\": \"branch\""));
    }

    [Test]
    public void finally_on_chained_request_serializes_independently()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Get("/api/first")
             .Finally(f => f.Element("first-spinner").Hide())
             .Response(r => r
                .OnSuccess(s => s.Element("first-result").Show())
                .Chained(c => c
                    .Get("/api/second")
                    .Finally(f2 => f2.Element("second-spinner").Hide())
                    .Response(r2 => r2.OnSuccess(s2 => s2.Element("second-result").Show()))
                )
             );
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        using var doc = JsonDocument.Parse(planJson);
        var outerRequest = doc.RootElement
            .GetProperty("behaviors")[0]
            .GetProperty("reaction")
            .GetProperty("request");

        Assert.That(outerRequest.TryGetProperty("complete", out _), Is.True,
            "Outer request must have 'complete'");
        Assert.That(outerRequest.TryGetProperty("next", out var next), Is.True,
            "Outer request must have 'next' (chained)");
        Assert.That(next.TryGetProperty("complete", out _), Is.True,
            "Chained request must have its own 'complete'");
    }
}
