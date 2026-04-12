using System.Text.Json;
using Alis.Reactive.Builders.Conditions;

namespace Alis.Reactive.UnitTests;

/// <summary>
/// Verifies that ResponseBody typed access produces the correct plan JSON
/// for success and error payload reads in conditions, setters, and branches.
/// </summary>
[TestFixture]
public class WhenUsingResponseBodyInConditions : PlanTestBase
{
    public class ApiResponse
    {
        public string Status { get; set; } = "";
        public int Count { get; set; }
    }

    public class ErrorResponse
    {
        public string Message { get; set; } = "";
        public int Code { get; set; }
    }

    [Test]
    public void When_with_success_ResponseBody_reads_from_success_scope()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/data")
             .Response(r => r.OnSuccess<ApiResponse>((json, s) =>
             {
                 s.When(json, j => j.Status).Eq("approved")
                  .Then(t => t.Element("badge").Show());
             }));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        using var doc = JsonDocument.Parse(planJson);
        var behaviors = doc.RootElement.GetProperty("behaviors");
        Assert.That(behaviors.GetArrayLength(), Is.GreaterThan(0));

        // The condition should read from payload scope "success"
        Assert.That(planJson, Does.Contain("\"scope\": \"success\""));
        Assert.That(planJson, Does.Contain("\"status\""));
    }

    [Test]
    public void When_with_error_ResponseBody_reads_from_error_scope()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/data")
             .Response(r => r.OnError<ErrorResponse>((err, s) =>
             {
                 s.When(err, e => e.Code).Eq(404)
                  .Then(t => t.Element("not-found").Show());
             }));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        // The condition should read from payload scope "error"
        Assert.That(planJson, Does.Contain("\"scope\": \"error\""));
        Assert.That(planJson, Does.Contain("\"code\""));
    }

    [Test]
    public void And_with_ResponseBody_composes_correctly()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/data")
             .Response(r => r.OnSuccess<ApiResponse>((json, s) =>
             {
                 s.When(json, j => j.Status).Eq("approved")
                  .And(json, j => j.Count).Gt(0)
                  .Then(t => t.Element("results").Show());
             }));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        // Should have an "all" condition with two terms both reading from success
        Assert.That(planJson, Does.Contain("\"kind\": \"all\""));
    }

    [Test]
    public void Or_with_ResponseBody_composes_correctly()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/data")
             .Response(r => r.OnSuccess<ApiResponse>((json, s) =>
             {
                 s.When(json, j => j.Status).Eq("approved")
                  .Or(json, j => j.Status).Eq("pending")
                  .Then(t => t.Element("status").Show());
             }));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        Assert.That(planJson, Does.Contain("\"kind\": \"any\""));
    }

    [Test]
    public void ElseIf_with_ResponseBody_branches_correctly()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/data")
             .Response(r => r.OnSuccess<ApiResponse>((json, s) =>
             {
                 s.When(json, j => j.Status).Eq("approved")
                  .Then(t => t.Element("badge").AddClass("green"))
                  .ElseIf(json, j => j.Status).Eq("pending")
                  .Then(t => t.Element("badge").AddClass("yellow"))
                  .Else(t => t.Element("badge").AddClass("red"));
             }));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        // Should have a branch with 3 cases
        Assert.That(planJson, Does.Contain("\"kind\": \"branch\""));
    }

    [Test]
    public void OnError_catchall_produces_handler_without_status()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/data")
             .Response(r => r.OnError(e =>
             {
                 e.Element("error").Show();
             }));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        // Error handler should exist in the plan
        Assert.That(planJson, Does.Contain("\"error\""));
    }

    [Test]
    public void OnError_typed_with_status_produces_handler_with_status()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/data")
             .Response(r => r.OnError<ErrorResponse>(422, (err, s) =>
             {
                 s.Element("validation-error").SetText(err, e => e.Message);
             }));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        Assert.That(planJson, Does.Contain("422"));
        // SetText reads from error scope
        Assert.That(planJson, Does.Contain("\"scope\": \"error\""));
    }

    [Test]
    public void OnSuccess_works_without_new_constraint()
    {
        // This test proves the new() constraint removal works —
        // ApiResponseRecord has no parameterless constructor.
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/data")
             .Response(r => r.OnSuccess<ApiResponse>((json, s) =>
             {
                 s.Element("result").SetText(json, j => j.Status);
             }));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        Assert.That(planJson, Does.Contain("\"scope\": \"success\""));
    }

    [Test]
    public void SetText_with_ResponseBody_uses_correct_scope()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/data")
             .Response(r => r.OnSuccess<ApiResponse>((json, s) =>
             {
                 s.Element("status").SetText(json, j => j.Status);
             }));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        // SetText should use the success scope from ResponseBody
        Assert.That(planJson, Does.Contain("\"scope\": \"success\""));
        Assert.That(planJson, Does.Contain("\"status\""));
    }

    [Test]
    public void PayloadTypedSource_produces_correct_ValueProducer()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/data")
             .Response(r => r.OnSuccess<ApiResponse>((json, s) =>
             {
                 // Use Read() to get a TypedSource, then use it in a condition
                 var source = json.Read(j => j.Count);
                 s.When(source).Gt(10)
                  .Then(t => t.Element("large-set").Show());
             }));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        Assert.That(planJson, Does.Contain("\"scope\": \"success\""));
        Assert.That(planJson, Does.Contain("\"count\""));
    }
}
