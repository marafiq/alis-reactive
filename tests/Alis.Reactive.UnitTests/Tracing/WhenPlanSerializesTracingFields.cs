using System.Text.Json;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.UnitTests.Tracing;

/// <summary>
/// Locks the JSON contract for the tracing fields added to the plan model.
/// These tests guarantee the server side <c>Plan</c> can actually emit
/// <c>traceparent</c> and <c>traceLevel</c> values that the browser runtime
/// reads at boot time. Without this round-trip, the tracing feature works
/// only for hand-built TS tests and every server rendered page silently
/// loses distributed-trace correlation.
/// </summary>
[TestFixture]
public class WhenPlanSerializesTracingFields : PlanTestBase
{
    private const string ValidTraceparent =
        "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01";

    [Test]
    public void plan_without_tracing_fields_omits_them_from_json()
    {
        var plan = CreatePlan();
        var json = plan.Render();

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.That(root.TryGetProperty("traceparent", out _), Is.False,
            "plan JSON must omit traceparent when none is set");
        Assert.That(root.TryGetProperty("traceLevel", out _), Is.False,
            "plan JSON must omit traceLevel when none is set");

        AssertSchemaValid(json);
    }

    [Test]
    public void plan_with_explicit_traceparent_emits_it_as_top_level_property()
    {
        // Reach the internal plan model via the ReactivePlan's context to
        // simulate what the render-time auto-populate path does.
        var plan = CreatePlan();
        plan.Context.Plan.Traceparent = ValidTraceparent;

        var json = plan.Render();

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.That(root.GetProperty("traceparent").GetString(), Is.EqualTo(ValidTraceparent),
            "plan JSON must carry the exact traceparent string set by the render path");

        AssertSchemaValid(json);
    }

    [Test]
    public void plan_with_trace_level_emits_it_as_top_level_property()
    {
        var plan = CreatePlan();
        plan.Context.Plan.TraceLevel = "debug";

        var json = plan.Render();

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.That(root.GetProperty("traceLevel").GetString(), Is.EqualTo("debug"),
            "plan JSON must carry the exact trace level set by the render path");

        AssertSchemaValid(json);
    }

    [Test]
    public void plan_with_both_trace_fields_emits_both()
    {
        var plan = CreatePlan();
        plan.Context.Plan.Traceparent = ValidTraceparent;
        plan.Context.Plan.TraceLevel = "trace";

        var json = plan.Render();

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.That(root.GetProperty("traceparent").GetString(), Is.EqualTo(ValidTraceparent));
        Assert.That(root.GetProperty("traceLevel").GetString(), Is.EqualTo("trace"));

        AssertSchemaValid(json);
    }

    [Test]
    public void invalid_traceparent_is_still_serialized_and_rejected_by_schema()
    {
        // The C# Plan model does not validate the traceparent string shape,
        // because the render path writes whatever Activity.Current.Id returns.
        // The JSON schema catches malformed values at the contract boundary;
        // the TS runtime also validates via parseTraceparent.
        var plan = CreatePlan();
        plan.Context.Plan.Traceparent = "not-a-traceparent";

        var json = plan.Render();

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.That(root.GetProperty("traceparent").GetString(), Is.EqualTo("not-a-traceparent"));

        // Schema MUST reject — traceparent field has a W3C pattern constraint.
        Assert.Throws<AssertionException>(
            () => AssertSchemaValid(json),
            "schema must reject a traceparent that does not match the W3C pattern");
    }

    [Test]
    public void invalid_trace_level_is_rejected_by_schema()
    {
        var plan = CreatePlan();
        plan.Context.Plan.TraceLevel = "bogus";

        var json = plan.Render();

        Assert.Throws<AssertionException>(
            () => AssertSchemaValid(json),
            "schema must reject a trace level outside the enum");
    }
}
