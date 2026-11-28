using System;
using System.Text.Json;
using Alis.Reactive.Conditions;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.RequestsAndResponses;

[TestFixture]
public sealed class WhenANoticeIsShown
{
    private const string NoticeObject = """{"kind":"plugin","name":"notice","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}}""";
    private const string SearchName = """{"kind":"op","op":"coalesce","args":[{"kind":"as","expr":{"kind":"get","path":["name"],"members":"required","target":{"kind":"obj","id":"search"}},"shape":{"kind":"string","date":"none","negativeZero":"0"}},{"kind":"lit","value":""}]}""";
    private const string NoticeContract = """{"kind":"list","items":[{"kind":"lit","value":"notice"}]}""";
    private const string NoContract = """{"kind":"list","items":[]}""";

    private static readonly TestCaseData[] EveryKindOfAText =
    [
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Notice().Success("Saved")), Shown("""{"kind":"lit","value":"Saved"}""", "success")).SetArgDisplayNames("success"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Notice().Error("Not saved")), Shown("""{"kind":"lit","value":"Not saved"}""", "error")).SetArgDisplayNames("error"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Notice().Warning("Nearly full")), Shown("""{"kind":"lit","value":"Nearly full"}""", "warning")).SetArgDisplayNames("warning"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Notice().Info("Rounds at nine")), Shown("""{"kind":"lit","value":"Rounds at nine"}""", "info")).SetArgDisplayNames("info"),
    ];

    private static readonly TestCaseData[] EveryKindOfASource =
    [
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Notice().Success(pipeline.Component<SearchPanel>("search").Name)), Shown(SearchName, "success")).SetArgDisplayNames("success"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Notice().Error(pipeline.Component<SearchPanel>("search").Name)), Shown(SearchName, "error")).SetArgDisplayNames("error"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Notice().Warning(pipeline.Component<SearchPanel>("search").Name)), Shown(SearchName, "warning")).SetArgDisplayNames("warning"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Notice().Info(pipeline.Component<SearchPanel>("search").Name)), Shown(SearchName, "info")).SetArgDisplayNames("info"),
    ];

    [TestCaseSource(nameof(EveryKindOfAText))]
    public void a_notice_shows_a_text_written_where_the_view_renders_as_its_kind(Action<Pipeline<RequestDesk>> write, string shown)
    {
        var plan = RequestPage.Rendered(write);
        Assert.That(RequestPage.Steps(plan)[0].GetRawText(), Is.EqualTo(shown));
        Assert.That(JsonDocument.Parse(plan).RootElement.GetProperty("objects").GetProperty("$plugin:notice").GetRawText(), Is.EqualTo(NoticeObject));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [TestCaseSource(nameof(EveryKindOfASource))]
    public void a_notice_shows_what_a_source_reads_as_text_and_an_empty_text_where_it_reads_nothing(Action<Pipeline<RequestDesk>> write, string shown)
    {
        var plan = RequestPage.Rendered(write);
        Assert.That(RequestPage.Steps(plan)[0].GetRawText(), Is.EqualTo(shown));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_default_route_that_shows_a_notice_names_the_notice_contract()
    {
        var plan = Rendered(requests => requests.OnError((failed, pipeline) => pipeline.Notice().Error("Not saved")), pipeline => pipeline.Get("/beds"));
        Assert.That(RequestPage.Send(plan, "contracts").GetRawText(), Is.EqualTo(NoticeContract));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_notice_inside_a_condition_of_a_default_route_names_the_notice_contract_after_the_busy_one()
    {
        var plan = Rendered(requests => requests.ShowBusy().OnError(503, Content.None, (reply, pipeline) =>
            pipeline.When(pipeline.Component<SearchPanel>("search").Count.Eq(0)).Then(pipeline => pipeline.Notice().Warning("Try later"))), pipeline => pipeline.Get("/beds"));
        Assert.That(RequestPage.Send(plan, "contracts").GetRawText(), Is.EqualTo("""{"kind":"list","items":[{"kind":"lit","value":"busy"},{"kind":"lit","value":"notice"}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_notice_a_default_routes_request_shows_names_the_notice_contract_on_the_request_it_routes()
    {
        var plan = Rendered(requests => requests.OnError(401, Content.None, (reply, pipeline) =>
            pipeline.Get("/session").OnSuccess(pipeline => pipeline.Notice().Info("Signed in again"))), pipeline => pipeline.Get("/beds"));
        var renewal = RequestPage.Request(plan).GetProperty("statuses")[0].GetProperty("do")[0];
        Assert.That(RequestPage.Send(plan, "contracts").GetRawText(), Is.EqualTo(NoticeContract));
        Assert.That(RequestPage.Field(renewal.GetProperty("send"), "contracts").GetRawText(), Is.EqualTo(NoContract));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_notice_in_the_requests_own_route_names_no_contract()
    {
        var plan = Rendered(requests => { }, pipeline => pipeline.Get("/beds").OnError((failed, pipeline) => pipeline.Notice().Error("Not saved")));
        Assert.That(RequestPage.Send(plan, "contracts").GetRawText(), Is.EqualTo(NoContract));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_default_route_the_request_replaces_names_no_contract()
    {
        var plan = Rendered(requests => requests.OnError(503, Content.None, (reply, pipeline) => pipeline.Notice().Error("Busy")),
            pipeline => pipeline.Get("/beds").OnError(503, Content.None, (reply, pipeline) => pipeline.Element("status").Text.Set("Busy")));
        Assert.That(RequestPage.Send(plan, "contracts").GetRawText(), Is.EqualTo(NoContract));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_request_that_takes_no_defaults_names_no_contract()
    {
        var plan = Rendered(requests => requests.OnError((failed, pipeline) => pipeline.Notice().Error("Not saved")), pipeline => pipeline.Get("/beds").NoDefaults());
        Assert.That(RequestPage.Send(plan, "contracts").GetRawText(), Is.EqualTo(NoContract));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    private static string Rendered(Action<RequestDefaults> defaults, Action<Pipeline<RequestDesk>> write)
        => RequestPage.RenderedIn(app => app.AddReactiveRequests(requests => defaults(requests.NoAntiforgery())), write);

    private static string Shown(string text, string kind)
        => """{"kind":"do","await":false,"expr":{"kind":"call","path":["show"],"members":"required","target":{"kind":"obj","id":"$plugin:notice"},"args":[""" + text +
            """,{"kind":"lit","value":""" + "\"" + kind + "\"" + "}]}}";
}
