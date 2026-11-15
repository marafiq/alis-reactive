using System;
using System.Text.Json;
using Alis.Reactive.Declarations;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Vendors;
using Microsoft.AspNetCore.Html;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.RequestsAndResponses;

[TestFixture]
public sealed class WhenARequestOwnsItsBlocks
{
    private const string AfterAwait = "A write to the event's payload after an awaited step reaches no vendor: write it before the first request, Confirm or async method.";

    [Test]
    public void loading_before_any_await_can_write_the_vendors_payload()
    {
        var plan = WithPayload((raised, pipeline) => pipeline.Get("/staff").WhileLoading(pipeline => raised.Note.Set("Loading")));
        Assert.That(RequestPage.Request(plan).GetProperty("layers")[0].GetProperty("whileLoading").GetRawText(), Is.EqualTo(
            """[{"kind":"set","path":["note"],"members":"required","content":false,"target":{"kind":"ref","name":"e"},"value":{"kind":"lit","value":"Loading"}}]"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_request_is_an_awaited_step_for_the_steps_after_it()
        => Assert.That(() => WithPayload((raised, pipeline) =>
        {
            pipeline.Get("/staff");
            raised.Note.Set("Late");
        }), Throws.InvalidOperationException.With.Message.EqualTo(AfterAwait));

    [Test]
    public void a_route_cannot_write_the_vendors_original_payload()
        => Assert.That(() => WithPayload((raised, pipeline) => pipeline.Head("/staff").OnSuccess((reply, pipeline) => raised.Note.Set("Late"))),
            Throws.InvalidOperationException.With.Message.EqualTo(AfterAwait));

    [Test]
    public void finally_cannot_write_the_vendors_original_payload()
        => Assert.That(() => WithPayload((raised, pipeline) => pipeline.Options("/staff").Finally(pipeline => raised.Note.Set("Late"))),
            Throws.InvalidOperationException.With.Message.EqualTo(AfterAwait));

    [Test]
    public void loading_after_an_earlier_await_cannot_write_the_original_payload()
        => Assert.That(() => WithPayload((raised, pipeline) =>
        {
            pipeline.Component<RequestPanel>("request").Wait();
            pipeline.Get("/staff").WhileLoading(pipeline => raised.Note.Set("Late"));
        }), Throws.InvalidOperationException.With.Message.EqualTo(AfterAwait));

    [Test]
    public void a_listener_inside_chained_keeps_the_reply_it_captured()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get("/staff").OnSuccess(Content.Text, (reply, pipeline) => { })
            .Chained((reply, pipeline) => pipeline.Listen(pipeline.Component<RequestPanel>("request"), panel => panel.Raised,
                (raised, pipeline) => pipeline.Element("heard").Text.Set(reply.Body))));
        var listening = RequestPage.Request(plan).GetProperty("chained")[0];
        Assert.That(listening.GetProperty("do").GetRawText(), Is.EqualTo(
            """[{"kind":"set","path":["textContent"],"members":"required","content":true,"target":{"kind":"obj","id":"heard"},"value":{"kind":"as","expr":{"kind":"get","path":["value"],"members":"optional","target":{"kind":"get","path":["body"],"members":"required","target":{"kind":"ref","name":"reply1"}}},"shape":{"kind":"string","date":"none","negativeZero":"0"}}}]"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void an_outer_reply_stays_visible_in_an_inner_request_route()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get("/staff").OnSuccess(Content.Text, (outer, pipeline) =>
            pipeline.Head("/photo").OnSuccess((inner, pipeline) => pipeline.Element("staff").Text.Set(outer.Body))));
        Assert.That(RequestPage.Request(plan).GetProperty("success").GetProperty("do")[0].GetProperty("reply").GetString(), Is.EqualTo("reply2"));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_reply_cannot_be_retained_into_finally()
    {
        ReadOnly<int>? retained = null;
        Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get("/staff").OnSuccess(Content.None, (reply, pipeline) => retained = reply.Status)
            .Finally(pipeline => pipeline.Element("status").Text.Set(retained!))),
            Throws.InvalidOperationException.With.Message.EqualTo("A request's reply is read outside its route or Chained block; read it in that block or a block inside it."));
    }

    [Test]
    public void request_parts_cannot_continue_after_their_callback()
    {
        QueryParts<SearchRequest>? retained = null;
        Assert.That(() => RequestPage.Rendered(pipeline =>
        {
            pipeline.Get<SearchRequest>("/staff", request => retained = request);
            retained!.Query(r => r.Name, "Ada");
        }), Throws.InvalidOperationException.With.Message.EqualTo("The request's parts are already written; write every part in its request callback."));
    }

    [Test]
    public void a_parts_stage_cannot_be_written_twice()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("/staff", request =>
        {
            request.Query(r => r.Id, 7);
            request.Query(r => r.Name, "Ada");
        })), Throws.InvalidOperationException.With.Message.EqualTo("A request's parts stage is already used; write its parts in one chain."));

    [Test]
    public void head_and_options_keep_their_routes_after_success()
    {
        var head = RequestPage.Rendered(pipeline => pipeline.Head("/staff").OnSuccess((reply, pipeline) => { })
            .WhileLoading(pipeline => { }).Finally(pipeline => { }).OnError(404, (reply, pipeline) => { })
            .OnError((failed, pipeline) => { }).Chained((reply, pipeline) => { }));
        var options = RequestPage.Rendered(pipeline => pipeline.Options("/staff").OnSuccess<SearchAddress>((reply, pipeline) => { })
            .WhileLoading(pipeline => { }).Finally(pipeline => { })
            .OnError(400, Content.Html, (reply, pipeline) => { }).OnError(404, Content.None, (reply, pipeline) => { })
            .OnError<SearchAddress>(409, (reply, pipeline) => { }).OnError<SearchAddress>((failed, pipeline) => { })
            .Chained((reply, pipeline) => { }));
        Assert.That(RequestPage.Request(head).GetProperty("statuses").GetRawText(), Is.EqualTo("""[{"status":404,"kind":"none","do":[]}]"""));
        Assert.That(RequestPage.Request(options).GetProperty("statuses").GetRawText(), Is.EqualTo("""[{"status":400,"kind":"html","do":[]},{"status":404,"kind":"none","do":[]},{"status":409,"kind":"json","do":[]}]"""));
        Assert.That(PlanSchema.ProblemsOf(head), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(options), Is.Empty);
    }

    [Test]
    public void get_keeps_all_its_error_forms_after_success()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get("/staff").OnSuccess<SearchAddress>((reply, pipeline) => { })
            .OnError(400, Content.Text, (reply, pipeline) => { }).OnError(404, Content.None, (reply, pipeline) => { })
            .OnError<SearchAddress>(409, (reply, pipeline) => { }).OnError<SearchAddress>((failed, pipeline) => { }));
        Assert.That(RequestPage.Request(plan).GetProperty("statuses").GetRawText(), Is.EqualTo("""[{"status":400,"kind":"text","do":[]},{"status":404,"kind":"none","do":[]},{"status":409,"kind":"json","do":[]}]"""));
        Assert.That(RequestPage.Request(plan).GetProperty("otherwise").GetRawText(), Is.EqualTo("""{"kind":"json","do":[]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    private static string WithPayload(Action<RequestRaised, Pipeline<RequestDesk>> write)
    {
        var html = Views.Of(new RequestDesk(), _ => { }, app => app.AddReactiveRequests(requests => requests.NoAntiforgery()));
        var plan = html.ReactivePlan<RequestDesk>();
        plan.Render<RequestPanel>("request", HtmlString.Empty).Reactive(panel => panel.Raised, write);
        var rendered = Views.TextOf(html.RenderPlan(plan));
        return rendered["<script type=\"application/json\" data-reactive-plan>".Length..^"</script>".Length];
    }
}

public sealed class RequestPanel : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public Event<RequestRaised> Raised => Event<RequestRaised>();

    public void Wait() => MethodAsync();
}

public sealed class RequestRaised : JsObject
{
    public WriteOnly<string> Note => WriteOnly<string>();
}
