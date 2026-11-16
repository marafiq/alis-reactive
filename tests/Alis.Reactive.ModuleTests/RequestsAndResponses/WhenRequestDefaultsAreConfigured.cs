using System;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.RequestsAndResponses;

[TestFixture]
public sealed class WhenRequestDefaultsAreConfigured
{
    private const string NoRetry = """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"none"}}]}""";
    private const string StandardRetry = """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"standard"}},{"name":"attempts","value":{"kind":"lit","value":3}},{"name":"firstWaitMilliseconds","value":{"kind":"lit","value":2000}}]}""";
    private const string OnlyAccept = """{"kind":"list","items":[{"kind":"record","fields":[{"name":"name","value":{"kind":"lit","value":"Accept"}},{"name":"value","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"lit","value":"*/*"}}]}},{"name":"form","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"text"}}]}}]}]}""";
    private const string OwnLayerOnly = """[{"whileLoading":[],"finally":[]}]""";
    private const string NoRoute = """{"kind":"none","do":[]}""";
    private const string StatusBusy = """{"kind":"set","path":["textContent"],"members":"required","content":true,"target":{"kind":"obj","id":"status"},"value":{"kind":"lit","value":"Busy"}}""";

    private static readonly TestCaseData[] EveryRetriedVerb =
    [
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Get("/beds"))).SetArgDisplayNames("GET"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Head("/beds"))).SetArgDisplayNames("HEAD"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Options("/beds"))).SetArgDisplayNames("OPTIONS"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Put("/beds"))).SetArgDisplayNames("PUT"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Delete("/beds"))).SetArgDisplayNames("DELETE"),
    ];

    private static readonly TestCaseData[] EveryNoRetry =
    [
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Get("/beds").NoRetry())).SetArgDisplayNames("GET"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Get("/beds").OnSuccess(pipeline => { }).NoRetry())).SetArgDisplayNames("GET after success"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Head("/beds").NoRetry())).SetArgDisplayNames("HEAD"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Head("/beds").OnSuccess(pipeline => { }).NoRetry())).SetArgDisplayNames("HEAD after success"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Options("/beds").NoRetry())).SetArgDisplayNames("OPTIONS"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Options("/beds").OnSuccess(pipeline => { }).NoRetry())).SetArgDisplayNames("OPTIONS after success"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Put("/beds").NoRetry())).SetArgDisplayNames("PUT"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Put("/beds").OnSuccess(pipeline => { }).NoRetry())).SetArgDisplayNames("PUT after success"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Delete("/beds").NoRetry())).SetArgDisplayNames("DELETE"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Delete("/beds").OnSuccess(pipeline => { }).NoRetry())).SetArgDisplayNames("DELETE after success"),
    ];

    private static readonly TestCaseData[] EveryNoDefaults =
    [
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Get("/beds").NoDefaults())).SetArgDisplayNames("GET"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Get("/beds").OnSuccess(pipeline => { }).NoDefaults())).SetArgDisplayNames("GET after success"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Head("/beds").NoDefaults())).SetArgDisplayNames("HEAD"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Head("/beds").OnSuccess(pipeline => { }).NoDefaults())).SetArgDisplayNames("HEAD after success"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Options("/beds").NoDefaults())).SetArgDisplayNames("OPTIONS"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Options("/beds").OnSuccess(pipeline => { }).NoDefaults())).SetArgDisplayNames("OPTIONS after success"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Post("/beds").NoDefaults())).SetArgDisplayNames("POST"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Post("/beds").OnSuccess(pipeline => { }).NoDefaults())).SetArgDisplayNames("POST after success"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Put("/beds").NoDefaults())).SetArgDisplayNames("PUT"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Put("/beds").OnSuccess(pipeline => { }).NoDefaults())).SetArgDisplayNames("PUT after success"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Delete("/beds").NoDefaults())).SetArgDisplayNames("DELETE"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Delete("/beds").OnSuccess(pipeline => { }).NoDefaults())).SetArgDisplayNames("DELETE after success"),
    ];

    private static readonly TestCaseData[] RefusedAtStartup =
    [
        new TestCaseData((Action<RequestDefaults>)(requests => requests.Header("X-Ward", "North").Header("x-ward", "South")),
            "Header 'x-ward' is supplied twice; send each header name once, ignoring case.").SetArgDisplayNames("a header twice"),
        new TestCaseData((Action<RequestDefaults>)(requests => requests.Header("Accept", "text/html")),
            "Accept is set by the success route; choose its Content kind.").SetArgDisplayNames("a header the route owns"),
        new TestCaseData((Action<RequestDefaults>)(requests => requests.Header("traceparent", "00")),
            "Header 'traceparent' is written by the framework or the browser; use an app header name.").SetArgDisplayNames("a header the framework writes"),
        new TestCaseData((Action<RequestDefaults>)(requests => requests.Header("X-Ward", "North\r\nInjected: yes")),
            "The header X-Ward holds a line break, which HTTP does not carry in a field value; send it in the body.").SetArgDisplayNames("a header's line break"),
        new TestCaseData((Action<RequestDefaults>)(requests => requests.ShowBusy().ShowBusy()),
            "AddReactiveRequests shows the busy indicator once; call ShowBusy() once.").SetArgDisplayNames("ShowBusy twice"),
        new TestCaseData((Action<RequestDefaults>)(requests => requests.WhileLoading(pipeline => { }).WhileLoading(pipeline => { })),
            "AddReactiveRequests already has a WhileLoading; put its steps in one block.").SetArgDisplayNames("WhileLoading twice"),
        new TestCaseData((Action<RequestDefaults>)(requests => requests.Finally(pipeline => { }).Finally(pipeline => { })),
            "AddReactiveRequests already has a Finally; put its steps in one block.").SetArgDisplayNames("Finally twice"),
        new TestCaseData((Action<RequestDefaults>)(requests => requests.OnError(200, Content.None, (reply, pipeline) => { })),
            "OnError status 200 is outside 300 to 599; name an error status in that range.").SetArgDisplayNames("a status below 300"),
        new TestCaseData((Action<RequestDefaults>)(requests => requests.OnError(600, Content.Json<SearchAddress>(), (reply, pipeline) => { })),
            "OnError status 600 is outside 300 to 599; name an error status in that range.").SetArgDisplayNames("a status above 599"),
        new TestCaseData((Action<RequestDefaults>)(requests => requests.OnError(503, Content.None, (reply, pipeline) => { }).OnError(503, Content.Text, (reply, pipeline) => { })),
            "AddReactiveRequests already has a default route for status 503; write one route a status.").SetArgDisplayNames("a status twice"),
        new TestCaseData((Action<RequestDefaults>)(requests => requests.ShowValidationErrors().OnError(400, Content.None, (reply, pipeline) => { })),
            "AddReactiveRequests already has a default route for status 400; write one route a status, ShowValidationErrors() among them.").SetArgDisplayNames("a 400 route beside ShowValidationErrors"),
        new TestCaseData((Action<RequestDefaults>)(requests => requests.ShowValidationErrors().ShowValidationErrors()),
            "AddReactiveRequests already has a default route for status 400; write one route a status, ShowValidationErrors() among them.").SetArgDisplayNames("ShowValidationErrors twice"),
        new TestCaseData((Action<RequestDefaults>)(requests => requests.OnError((failed, pipeline) => { }).OnError<SearchAddress>((failed, pipeline) => { })),
            "AddReactiveRequests already has a catch-all route; write one default catch-all.").SetArgDisplayNames("a second catch-all"),
        new TestCaseData((Action<RequestDefaults>)(requests => requests.Retry(3, TimeSpan.FromSeconds(2)).Retry(4, TimeSpan.FromSeconds(1))),
            "AddReactiveRequests sets the retry once; call Retry(attempts, firstWait) once.").SetArgDisplayNames("Retry twice"),
        new TestCaseData((Action<RequestDefaults>)(requests => requests.Retry(0, TimeSpan.FromSeconds(2))),
            "Retry(0, ..) retries no request; give attempts from 1, and write NoRetry() on a request that is sent once.").SetArgDisplayNames("no attempts"),
        new TestCaseData((Action<RequestDefaults>)(requests => requests.Retry(3, TimeSpan.FromSeconds(-1))),
            "Retry's first wait of -00:00:01 is outside 0 to 30 seconds, the time a request's retries end at; give a first wait under 30 seconds.").SetArgDisplayNames("a wait below zero"),
        new TestCaseData((Action<RequestDefaults>)(requests => requests.Retry(3, TimeSpan.FromSeconds(30))),
            "Retry's first wait of 00:00:30 is outside 0 to 30 seconds, the time a request's retries end at; give a first wait under 30 seconds.").SetArgDisplayNames("a wait of 30 seconds"),
        new TestCaseData((Action<RequestDefaults>)(requests => requests.Retry(3, TimeSpan.FromTicks(5000))),
            "Retry's first wait of 00:00:00.0005000 is not a whole number of milliseconds; give it in whole milliseconds.").SetArgDisplayNames("a part of a millisecond"),
    ];

    [TestCaseSource(nameof(EveryRetriedVerb))]
    public void every_request_that_can_be_sent_again_takes_three_retries_from_two_seconds(Action<Pipeline<RequestDesk>> write)
    {
        var plan = Rendered(requests => { }, write);
        Assert.That(RequestPage.Send(plan, "retry").GetRawText(), Is.EqualTo(StandardRetry));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void retry_sets_the_numbers_every_request_takes()
    {
        var plan = Rendered(requests => requests.Retry(5, TimeSpan.FromMilliseconds(1500)), pipeline => pipeline.Get("/beds"));
        Assert.That(RequestPage.Send(plan, "retry").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"standard"}},{"name":"attempts","value":{"kind":"lit","value":5}},{"name":"firstWaitMilliseconds","value":{"kind":"lit","value":1500}}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_post_is_never_sent_again()
    {
        var plan = Rendered(requests => requests.Retry(5, TimeSpan.FromMilliseconds(1500)), pipeline => pipeline.Post("/beds"));
        Assert.That(RequestPage.Send(plan, "retry").GetRawText(), Is.EqualTo(NoRetry));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [TestCaseSource(nameof(EveryNoRetry))]
    public void no_retry_sends_the_request_once(Action<Pipeline<RequestDesk>> write)
    {
        var plan = Rendered(requests => { }, write);
        Assert.That(RequestPage.Send(plan, "retry").GetRawText(), Is.EqualTo(NoRetry));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_default_header_is_sent_before_the_requests_own_and_gives_way_to_one_of_its_name()
    {
        var plan = Rendered(requests => requests.Header("X-Requested-With", "XMLHttpRequest").Header("X-Ward", "North"),
            pipeline => pipeline.Get<SearchRequest>("/beds", request => request.Header("x-ward", "South")));
        Assert.That(RequestPage.Send(plan, "headers").GetRawText(), Is.EqualTo(
            """{"kind":"list","items":[""" +
            """{"kind":"record","fields":[{"name":"name","value":{"kind":"lit","value":"X-Requested-With"}},{"name":"value","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"lit","value":"XMLHttpRequest"}}]}},{"name":"form","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"text"}}]}}]},""" +
            """{"kind":"record","fields":[{"name":"name","value":{"kind":"lit","value":"x-ward"}},{"name":"value","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"lit","value":"South"}}]}},{"name":"form","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"text"}}]}}]},""" +
            """{"kind":"record","fields":[{"name":"name","value":{"kind":"lit","value":"Accept"}},{"name":"value","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"lit","value":"*/*"}}]}},{"name":"form","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"text"}}]}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void the_defaults_layer_is_the_first_around_the_requests_own()
    {
        var plan = Rendered(requests => requests
                .WhileLoading(pipeline => pipeline.Element("status").Text.Set("Loading"))
                .Finally(pipeline => pipeline.Element("status").Text.Set("Done")),
            pipeline => pipeline.Get("/beds")
                .Finally(pipeline => pipeline.Element("save").Text.Set("Saved"))
                .WhileLoading(pipeline => pipeline.Element("save").Text.Set("Saving")));
        Assert.That(RequestPage.Request(plan).GetProperty("layers").GetRawText(), Is.EqualTo(
            "[" + Layer(Text("status", "Loading"), Text("status", "Done")) + "," + Layer(Text("save", "Saving"), Text("save", "Saved")) + "]"));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_status_the_request_routes_replaces_the_defaults_route_and_the_other_default_routes_are_added()
    {
        var plan = Rendered(requests => requests
                .OnError(503, Content.None, (reply, pipeline) => pipeline.Element("status").Text.Set("Busy"))
                .OnError(404, Content.Text, (reply, pipeline) => pipeline.Element("status").Text.Set(reply.Body)),
            pipeline => pipeline.Get("/beds").OnError(404, Content.None, (reply, pipeline) => pipeline.Element("status").Text.Set("Gone")));
        Assert.That(RequestPage.Request(plan).GetProperty("statuses").GetRawText(), Is.EqualTo(
            """[{"status":404,"kind":"none","do":[""" + Text("status", "Gone") + """]},{"status":503,"kind":"none","do":[""" + StatusBusy + "]}]"));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_default_status_route_reads_the_body_as_its_kind_names_it()
    {
        var plan = Rendered(requests => requests.OnError(409, Content.Json<SearchAddress>(), (reply, pipeline) => pipeline.Element("status").Text.Set(reply.Status)),
            pipeline => pipeline.Get("/beds"));
        Assert.That(RequestPage.Request(plan).GetProperty("statuses").GetRawText(), Is.EqualTo(
            """[{"status":409,"kind":"json","do":[{"kind":"set","path":["textContent"],"members":"required","content":true,"target":{"kind":"obj","id":"status"},"value":{"kind":"as","expr":{"kind":"get","path":["status"],"members":"required","target":{"kind":"ref","name":"reply1"}},"shape":{"kind":"string","date":"none","negativeZero":"0"}}}]}]"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void the_defaults_catch_all_runs_where_the_request_names_none()
    {
        var defaults = (Action<RequestDefaults>)(requests => requests.OnError((failed, pipeline) => pipeline.Element("status").Text.Set("Failed")));
        var taken = Rendered(defaults, pipeline => pipeline.Get("/beds"));
        var own = Rendered(defaults, pipeline => pipeline.Get("/beds").OnError((failed, pipeline) => pipeline.Element("status").Text.Set("Own")));
        Assert.That(RequestPage.Request(taken).GetProperty("otherwise").GetRawText(), Is.EqualTo("""{"kind":"none","do":[""" + Text("status", "Failed") + "]}"));
        Assert.That(RequestPage.Request(own).GetProperty("otherwise").GetRawText(), Is.EqualTo("""{"kind":"none","do":[""" + Text("status", "Own") + "]}"));
        Assert.That(PlanSchema.ProblemsOf(taken), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(own), Is.Empty);
    }

    [Test]
    public void a_typed_default_catch_all_reads_a_json_body()
    {
        var plan = Rendered(requests => requests.OnError<SearchAddress>((failed, pipeline) => pipeline.Element("status").Text.Set("Failed")), pipeline => pipeline.Get("/beds"));
        Assert.That(RequestPage.Request(plan).GetProperty("otherwise").GetRawText(), Is.EqualTo("""{"kind":"json","do":[""" + Text("status", "Failed") + "]}"));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [TestCaseSource(nameof(EveryNoDefaults))]
    public void no_defaults_takes_none_of_the_defaults_parts(Action<Pipeline<RequestDesk>> write)
    {
        var plan = Rendered(requests => requests.Header("X-Requested-With", "XMLHttpRequest")
            .WhileLoading(pipeline => pipeline.Element("status").Text.Set("Loading"))
            .OnError(503, Content.None, (reply, pipeline) => pipeline.Element("status").Text.Set("Busy"))
            .OnError((failed, pipeline) => pipeline.Element("status").Text.Set("Failed")), write);
        var request = RequestPage.Request(plan);
        Assert.That(request.GetProperty("layers").GetRawText(), Is.EqualTo(OwnLayerOnly));
        Assert.That(request.GetProperty("statuses").GetRawText(), Is.EqualTo("[]"));
        Assert.That(request.GetProperty("otherwise").GetRawText(), Is.EqualTo(NoRoute));
        Assert.That(RequestPage.Send(plan, "headers").GetRawText(), Is.EqualTo(OnlyAccept));
        Assert.That(RequestPage.Send(plan, "retry").GetRawText(), Is.EqualTo(NoRetry));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_request_a_default_route_sends_takes_no_defaults()
    {
        var plan = Rendered(requests => requests
                .Header("X-Requested-With", "XMLHttpRequest")
                .OnError(401, Content.None, (reply, pipeline) => pipeline.Get("/session")),
            pipeline => pipeline.Get("/beds"));
        var renewal = RequestPage.Request(plan).GetProperty("statuses")[0].GetProperty("do")[0];
        Assert.That(renewal.GetProperty("layers").GetRawText(), Is.EqualTo(OwnLayerOnly));
        Assert.That(renewal.GetProperty("statuses").GetRawText(), Is.EqualTo("[]"));
        Assert.That(RequestPage.Field(renewal.GetProperty("send"), "retry").GetRawText(), Is.EqualTo(NoRetry));
        Assert.That(RequestPage.Field(renewal.GetProperty("send"), "headers").GetRawText(), Is.EqualTo(OnlyAccept));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_request_the_defaults_finally_sends_takes_no_defaults()
    {
        var plan = Rendered(requests => requests
                .OnError(503, Content.None, (reply, pipeline) => pipeline.Element("status").Text.Set("Busy"))
                .Finally(pipeline => pipeline.Post("/audit")),
            pipeline => pipeline.Get("/beds"));
        var audit = RequestPage.Request(plan).GetProperty("layers")[0].GetProperty("finally")[0];
        Assert.That(audit.GetProperty("layers").GetRawText(), Is.EqualTo(OwnLayerOnly));
        Assert.That(audit.GetProperty("statuses").GetRawText(), Is.EqualTo("[]"));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [TestCaseSource(nameof(RefusedAtStartup))]
    public void a_default_that_cannot_hold_is_refused_at_startup(Action<RequestDefaults> defaults, string refusal)
        => Assert.That(() => new ServiceCollection().AddReactiveRequests(defaults), Throws.InvalidOperationException.With.Message.EqualTo(refusal));

    private static string Rendered(Action<RequestDefaults> defaults, Action<Pipeline<RequestDesk>> write)
        => RequestPage.RenderedIn(app => app.AddReactiveRequests(requests => defaults(requests.NoAntiforgery())), write);

    private static string Layer(string whileLoading, string @finally) => """{"whileLoading":[""" + whileLoading + """],"finally":[""" + @finally + "]}";

    private static string Text(string id, string text)
        => """{"kind":"set","path":["textContent"],"members":"required","content":true,"target":{"kind":"obj","id":""" + "\"" + id + "\"" +
            """},"value":{"kind":"lit","value":""" + "\"" + text + "\"" + "}}";
}
