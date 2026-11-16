using System;
using System.Text.Json;
using Alis.Reactive.Declarations;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Vendors;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.RequestsAndResponses;

[TestFixture]
public sealed class WhenValidationErrorsAreShown
{
    private const string FormObject = """{"kind":"plugin","name":"alis.form","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}}""";
    private const string AtTheNotice = """{"kind":"record","fields":[{"name":"destination","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"notice"}},{"name":"separator","value":{"kind":"lit","value":" "}}]}}]}""";
    private const string NoticeContract = """{"kind":"list","items":[{"kind":"lit","value":"notice"}]}""";
    private const string NoContract = """{"kind":"list","items":[]}""";
    private const string InTheContactSummary = """{"kind":"record","fields":[{"name":"destination","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"summary"}},{"name":"containerId","value":{"kind":"lit","value":"contact"}}]}}]}""";
    private const string OutsideARoute = "ShowValidationErrors shows a server's errors as a request's failures, and this step is in no route of a request; write it in the request's OnError or OnSuccess.";

    [Test]
    public void the_defaults_show_a_400s_errors_at_the_notice_with_the_inputs_the_request_sent()
    {
        var plan = Rendered(requests => requests.ShowValidationErrors(), pipeline => pipeline.Post("/beds"));
        Assert.That(RequestPage.Request(plan).GetProperty("statuses").GetRawText(), Is.EqualTo("""[{"status":400,"kind":"json","do":[""" + ShownFor("reply1") + "]}]"));
        Assert.That(RequestPage.Send(plan, "contracts").GetRawText(), Is.EqualTo(NoticeContract));
        Assert.That(JsonDocument.Parse(plan).RootElement.GetProperty("objects").GetProperty("$form").GetRawText(), Is.EqualTo(FormObject));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void the_defaults_show_a_400s_errors_in_the_summary_of_the_container_the_request_sends_and_name_no_notice()
    {
        var plan = Rendered(requests => requests.ShowValidationErrors(), pipeline => pipeline.Post<IntakeSend>("/intakes", request => request.Inputs(pipeline.Container<IntakeSend>("contact"))));
        Assert.That(RequestPage.Request(plan).GetProperty("statuses").GetRawText(), Is.EqualTo("""[{"status":400,"kind":"json","do":[""" + ShownFor("reply1", InTheContactSummary) + "]}]"));
        Assert.That(RequestPage.Send(plan, "contracts").GetRawText(), Is.EqualTo(NoContract));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_route_shows_a_problem_as_its_own_requests_failures()
    {
        var plan = Rendered(requests => { }, pipeline => pipeline.Put("/beds").OnError<ValidationProblem>(400, (refused, pipeline) => pipeline.ShowValidationErrors(refused.Body)));
        Assert.That(RequestPage.Request(plan).GetProperty("statuses").GetRawText(), Is.EqualTo("""[{"status":400,"kind":"json","do":[""" + ShownFor("reply1") + "]}]"));
        Assert.That(RequestPage.Send(plan, "contracts").GetRawText(), Is.EqualTo(NoticeContract));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_route_of_a_request_inside_another_route_shows_the_inner_requests_failures()
    {
        var plan = Rendered(requests => { }, pipeline => pipeline.Get("/beds").OnSuccess(pipeline =>
            pipeline.Post("/beds/hold").OnError<ValidationProblem>(400, (refused, pipeline) => pipeline.ShowValidationErrors(refused.Body))));
        var inner = RequestPage.Request(plan).GetProperty("success").GetProperty("do")[0];
        Assert.That(inner.GetProperty("statuses").GetRawText(), Is.EqualTo("""[{"status":400,"kind":"json","do":[""" + ShownFor("reply2") + "]}]"));
        Assert.That(RequestPage.Field(inner.GetProperty("send"), "contracts").GetRawText(), Is.EqualTo(NoticeContract));
        Assert.That(RequestPage.Send(plan, "contracts").GetRawText(), Is.EqualTo(NoContract));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_request_that_routes_its_own_400_replaces_the_defaults_and_names_no_notice()
    {
        var plan = Rendered(requests => requests.ShowValidationErrors(),
            pipeline => pipeline.Post("/beds").OnError(400, Content.None, (refused, pipeline) => pipeline.Element("status").Text.Set("Refused")));
        Assert.That(RequestPage.Request(plan).GetProperty("statuses").GetRawText(), Is.EqualTo(
            """[{"status":400,"kind":"none","do":[{"kind":"set","path":["textContent"],"members":"required","content":true,"target":{"kind":"obj","id":"status"},"value":{"kind":"lit","value":"Refused"}}]}]"""));
        Assert.That(RequestPage.Send(plan, "contracts").GetRawText(), Is.EqualTo(NoContract));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void the_step_in_no_route_of_a_request_is_refused()
        => Assert.That(() => Rendered(requests => { }, pipeline => pipeline.Get("/beds").OnSuccess<ValidationProblem>((reply, pipeline) => { })
                .Finally(pipeline => pipeline.ShowValidationErrors(pipeline.Component<ProblemPanel>("problems").Problem))),
            Throws.InvalidOperationException.With.Message.EqualTo(OutsideARoute));

    [Test]
    public void the_step_in_a_handler_with_no_request_is_refused()
        => Assert.That(() => Rendered(requests => { }, pipeline => pipeline.ShowValidationErrors(pipeline.Component<ProblemPanel>("problems").Problem)),
            Throws.InvalidOperationException.With.Message.EqualTo(OutsideARoute));

    private static string Rendered(Action<RequestDefaults> defaults, Action<Pipeline<RequestDesk>> write)
        => RequestPage.RenderedIn(app => app.AddReactiveRequests(requests => defaults(requests.NoAntiforgery())), write);

    private static string ShownFor(string reply) => ShownFor(reply, AtTheNotice);

    private static string ShownFor(string reply, string place)
        => """{"kind":"do","await":false,"expr":{"kind":"call","path":["showErrors"],"members":"required","target":{"kind":"obj","id":"$form"},"args":[""" +
            """{"kind":"get","path":["value"],"members":"optional","target":{"kind":"get","path":["body"],"members":"required","target":{"kind":"ref","name":""" + "\"" + reply + "\"" + "}}}," +
            """{"kind":"get","path":["inputs"],"members":"required","target":{"kind":"ref","name":""" + "\"" + reply + "\"" + "}}," +
            place + "]}}";
}

public sealed class ProblemPanel : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public ReadOnly<ValidationProblem> Problem => ReadOnly<ValidationProblem>();
}
