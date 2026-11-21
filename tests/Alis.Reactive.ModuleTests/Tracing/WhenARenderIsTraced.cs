using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.Tracing;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc.Rendering;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Tracing;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenARenderIsTraced
{
    private const string View = "/Care/Round.cshtml";

    [Test]
    public void a_render_no_request_activity_holds_writes_the_trace_off_and_reads_no_view()
    {
        var html = Views.Of(new Round("North"));
        var plan = TwoHandlersOf(html);

        var json = RoundPlans.JsonOf(html.RenderPlan(plan));

        Assert.That(json, Is.EqualTo(RoundPlans.Head + "\"trace\":{\"kind\":\"off\"}," + RoundPlans.Body));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_render_whose_request_activity_is_not_recorded_writes_the_trace_off()
    {
        using var request = RoundPlans.ARequest(ActivityTraceFlags.None);
        var html = Views.Of(new Round("North"));
        var plan = TwoHandlersOf(html);

        var json = RoundPlans.JsonOf(html.RenderPlan(plan));

        Assert.That(json, Is.EqualTo(RoundPlans.Head + "\"trace\":{\"kind\":\"off\"}," + RoundPlans.Body));
    }

    [Test]
    public void a_recorded_render_writes_the_trace_on_with_its_context_and_a_handler_called_outside_the_view_at_its_position()
    {
        using var request = RoundPlans.ARequest(ActivityTraceFlags.Recorded);
        var html = Views.Of(new Round("North"));
        html.ViewContext.ExecutingFilePath = View;
        var plan = TwoHandlersOf(html);

        var json = RoundPlans.JsonOf(html.RenderPlan(plan));

        Assert.That(json, Is.EqualTo(RoundPlans.Head +
            "\"trace\":{\"kind\":\"on\",\"traceparent\":\"" + RoundPlans.TraceparentOf(request) +
            "\",\"tracestate\":\"rojo=00f067aa0ba902b7\",\"console\":false,\"sources\":[{\"view\":\"/Care/Round.cshtml\",\"line\":1},{\"view\":\"/Care/Round.cshtml\",\"line\":2}]}," +
            RoundPlans.Body));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void the_app_that_sets_console_writes_console_in_a_traced_render()
    {
        using var request = RoundPlans.ARequest(ActivityTraceFlags.Recorded);
        var html = Views.Of(new Round("North"), _ => { }, services => services.AddReactiveTracing(tracing => tracing.Console = true));
        html.ViewContext.ExecutingFilePath = View;
        var plan = TwoHandlersOf(html);

        var json = RoundPlans.JsonOf(html.RenderPlan(plan));

        Assert.That(json, Is.EqualTo(RoundPlans.Head +
            "\"trace\":{\"kind\":\"on\",\"traceparent\":\"" + RoundPlans.TraceparentOf(request) +
            "\",\"tracestate\":\"rojo=00f067aa0ba902b7\",\"console\":true,\"sources\":[{\"view\":\"/Care/Round.cshtml\",\"line\":1},{\"view\":\"/Care/Round.cshtml\",\"line\":2}]}," +
            RoundPlans.Body));
    }

    [Test]
    public void the_app_that_leaves_console_unset_writes_it_false()
    {
        using var request = RoundPlans.ARequest(ActivityTraceFlags.Recorded);
        var html = Views.Of(new Round("North"), _ => { }, services => services.AddReactiveTracing(_ => { }));
        html.ViewContext.ExecutingFilePath = View;
        var plan = TwoHandlersOf(html);

        var json = RoundPlans.JsonOf(html.RenderPlan(plan));

        Assert.That(json, Does.StartWith(RoundPlans.Head +
            "\"trace\":{\"kind\":\"on\",\"traceparent\":\"" + RoundPlans.TraceparentOf(request) + "\",\"tracestate\":\"rojo=00f067aa0ba902b7\",\"console\":false,"));
    }

    [Test]
    public void a_render_the_app_listens_to_names_its_render_activity_as_the_pages_parent()
    {
        var stopped = new List<Activity>();
        using var listener = AListenerOf(stopped);
        using var request = RoundPlans.ARequest(ActivityTraceFlags.Recorded);
        var html = Views.Of(new Round("North"));
        html.ViewContext.ExecutingFilePath = View;
        var plan = TwoHandlersOf(html);

        var json = RoundPlans.JsonOf(html.RenderPlan(plan));

        Assert.That(stopped.ConvertAll(activity => (activity.OperationName, activity.ParentSpanId.ToHexString(), activity.GetTagItem("alis.plan_id"))),
            Is.EqualTo(new[] { ("alis.render", request.SpanId.ToHexString(), (object?)"Alis.Reactive.ModuleTests.Tracing.Round") }));
        Assert.That(json, Does.StartWith(RoundPlans.Head + "\"trace\":{\"kind\":\"on\",\"traceparent\":\"" + RoundPlans.TraceparentOf(stopped[0]) + "\""));
        Assert.That(Activity.Current, Is.SameAs(request));
    }

    [Test]
    public void a_render_whose_recorded_activity_has_a_hierarchical_id_is_refused()
    {
        using var request = new Activity("GET /Care/Round").SetIdFormat(ActivityIdFormat.Hierarchical).Start();
        request.ActivityTraceFlags = ActivityTraceFlags.Recorded;
        var html = Views.Of(new Round("North"));
        html.ViewContext.ExecutingFilePath = View;
        var plan = TwoHandlersOf(html);

        Assert.That(
            () => html.RenderPlan(plan),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "The request's activity is recorded under a hierarchical id, which W3C Trace Context cannot carry to the page: keep Activity.DefaultIdFormat at its W3C default."));
    }

    [Test]
    public async Task each_handler_of_a_traced_view_carries_the_view_and_the_line_of_its_builder_call()
    {
        using var request = RoundPlans.ARequest(ActivityTraceFlags.Recorded);
        var html = Views.Of(new Round("North"));

        var rendered = Views.TextOf(await html.PartialAsync("/Tracing/RoundBoard.cshtml", new Round("North")));

        Assert.That(RoundPlans.JsonOf(rendered), Is.EqualTo(RoundPlans.Head +
            "\"trace\":{\"kind\":\"on\",\"traceparent\":\"" + RoundPlans.TraceparentOf(request) +
            "\",\"tracestate\":\"rojo=00f067aa0ba902b7\",\"console\":false,\"sources\":[{\"view\":\"/Tracing/RoundBoard.cshtml\",\"line\":8},{\"view\":\"/Tracing/RoundBoard.cshtml\",\"line\":9},{\"view\":\"/Tracing/RoundBoard.cshtml\",\"line\":12}]}," +
            RoundPlans.BoardBody));
    }

    private static ReactivePlan<Round> TwoHandlersOf(IHtmlHelper<Round> html)
    {
        var plan = html.ReactivePlan<Round>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Element("round-status").Text.Set("begun")));
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Element("round-status").Text.Set("done")));
        return plan;
    }

    private static ActivityListener AListenerOf(List<Activity> stopped)
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == ReactiveTracing.SourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = stopped.Add,
        };
        ActivitySource.AddActivityListener(listener);
        return listener;
    }
}
