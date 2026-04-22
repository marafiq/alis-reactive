using Alis.Reactive.Builders;

namespace Alis.Reactive.UnitTests.Shape;

/// <summary>
/// End-to-end proof that Fix 2 (ElementBuilder event/response reads) and Fix 3
/// (HTTP gather event fields) carry the honest shape through to the rendered plan JSON.
/// </summary>
public class TypedFieldEventArgs
{
    public string? Name { get; set; }
    public DateTime ObservedAt { get; set; }
    public decimal Amount { get; set; }
    public bool IsActive { get; set; }
}

public class TypedFieldResponseBody
{
    public string? Title { get; set; }
    public DateTime CreatedAt { get; set; }
}

[TestFixture]
public class WhenThreadingShapeEndToEnd : PlanTestBase
{
    // ── Fix 2: ElementBuilder.SetText(event, x => x.TypedField) ──

    [Test]
    public void SetText_event_datetime_field_emits_date_shape_in_plan_json()
    {
        var plan = CreatePlan();
        var args = new TypedFieldEventArgs();
        Trigger(plan).DomReady(p =>
        {
            p.Element("echo").SetText(args, a => a.ObservedAt);
        });

        var json = plan.RenderFormatted();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("\"kind\": \"date\""),
            "SetText(args, a => a.ObservedAt) with DateTime property must emit Shape.Date in the ReadProducer");
    }

    [Test]
    public void SetText_event_decimal_field_emits_number_shape_in_plan_json()
    {
        var plan = CreatePlan();
        var args = new TypedFieldEventArgs();
        Trigger(plan).DomReady(p =>
        {
            p.Element("echo").SetText(args, a => a.Amount);
        });

        var json = plan.RenderFormatted();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("\"kind\": \"number\""),
            "SetText(args, a => a.Amount) with decimal property must emit Shape.Number in the ReadProducer");
    }

    [Test]
    public void SetText_response_body_datetime_field_emits_date_shape_in_plan_json()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Get("/api/thing").Response(r => r.OnSuccess<TypedFieldResponseBody>((body, s) =>
            {
                s.Element("echo").SetText(body, b => b.CreatedAt);
            }));
        });

        var json = plan.RenderFormatted();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("\"kind\": \"date\""),
            "SetText(body, b => b.CreatedAt) with DateTime property must emit Shape.Date in the ReadProducer");
    }

    // ── Fix 3: HTTP gather FromEvent(args, x => x.TypedField, "param") ──

    [Test]
    public void Gather_FromEvent_datetime_field_emits_date_shape_in_plan_json()
    {
        var plan = CreatePlan();
        var args = new TypedFieldEventArgs();
        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/submit", g => g.FromEvent(args, a => a.ObservedAt, "when"));
        });

        var json = plan.RenderFormatted();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("\"kind\": \"date\""),
            "FromEvent(args, a => a.ObservedAt, \"when\") must carry Shape.Date into the request body");
    }

    [Test]
    public void Gather_FromEvent_decimal_field_emits_number_shape_in_plan_json()
    {
        var plan = CreatePlan();
        var args = new TypedFieldEventArgs();
        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/submit", g => g.FromEvent(args, a => a.Amount, "amount"));
        });

        var json = plan.RenderFormatted();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("\"kind\": \"number\""),
            "FromEvent(args, a => a.Amount, \"amount\") must carry Shape.Number into the request body");
    }

    [Test]
    public void Gather_FromEvent_bool_field_emits_boolean_shape_in_plan_json()
    {
        var plan = CreatePlan();
        var args = new TypedFieldEventArgs();
        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/submit", g => g.FromEvent(args, a => a.IsActive, "active"));
        });

        var json = plan.RenderFormatted();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("\"kind\": \"boolean\""),
            "FromEvent(args, a => a.IsActive, \"active\") must carry Shape.Boolean into the request body");
    }

    // ── Fix 3: outer ObjectProducer carries shape (not Shape.None) ──

    [Test]
    public void Gather_statics_and_event_fields_outer_object_carries_non_none_shape()
    {
        var plan = CreatePlan();
        var args = new TypedFieldEventArgs();
        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/submit", g => g
                .Static("source", "kiosk")
                .FromEvent(args, a => a.ObservedAt, "when"));
        });

        var json = plan.RenderFormatted();
        AssertSchemaValid(json);
        // ValueProducer.Object(fields, shape: Shape.OpenObject()) — the outer object carries "kind": "object".
        Assert.That(json, Does.Contain("\"kind\": \"object\""),
            "BuildStaticAndEventFields must pass Shape.OpenObject() so the outer ObjectProducer isn't Shape.None");
    }
}
