using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Conditions;
using Alis.Reactive.Declarations;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.RequestsAndResponses;

[TestFixture]
public sealed class WhenARequestReplyIsRead
{
    [Test]
    public void loading_finally_routes_and_chained_keep_their_own_blocks()
    {
        var plan = RequestPage.Rendered(pipeline =>
        {
            pipeline.Get("/staff")
                .Finally(pipeline => pipeline.Element("finally").Text.Set("Settled"))
                .WhileLoading(pipeline => pipeline.Element("loading").Text.Set("Loading"))
                .OnError(404, Content.Text, (reply, pipeline) => pipeline.Element("missing").Text.Set(reply.Body))
                .OnSuccess(Content.Text, (reply, pipeline) => pipeline.Element("found").Text.Set(reply.Body))
                .OnError((failed, pipeline) => pipeline.Element("failed").Text.Set(failed.Status))
                .Chained((reply, pipeline) => pipeline.Element("chained").Text.Set(reply.Body));
            pipeline.Element("after").Text.Set("After");
        });
        var request = RequestPage.Request(plan);
        Assert.That(request.GetProperty("layers").GetRawText(), Is.EqualTo(
            """[{"whileLoading":[{"kind":"set","path":["textContent"],"members":"required","content":true,"target":{"kind":"obj","id":"loading"},"value":{"kind":"lit","value":"Loading"}}],"finally":[{"kind":"set","path":["textContent"],"members":"required","content":true,"target":{"kind":"obj","id":"finally"},"value":{"kind":"lit","value":"Settled"}}]}]"""));
        Assert.That(request.GetProperty("success").GetRawText(), Is.EqualTo(
            """{"kind":"text","do":[{"kind":"set","path":["textContent"],"members":"required","content":true,"target":{"kind":"obj","id":"found"},"value":{"kind":"as","expr":{"kind":"get","path":["value"],"members":"optional","target":{"kind":"get","path":["body"],"members":"required","target":{"kind":"ref","name":"reply1"}}},"shape":{"kind":"string","date":"none","negativeZero":"0"}}}]}"""));
        Assert.That(request.GetProperty("statuses").GetRawText(), Is.EqualTo(
            """[{"status":404,"kind":"text","do":[{"kind":"set","path":["textContent"],"members":"required","content":true,"target":{"kind":"obj","id":"missing"},"value":{"kind":"as","expr":{"kind":"get","path":["value"],"members":"optional","target":{"kind":"get","path":["body"],"members":"required","target":{"kind":"ref","name":"reply1"}}},"shape":{"kind":"string","date":"none","negativeZero":"0"}}}]}]"""));
        Assert.That(request.GetProperty("otherwise").GetRawText(), Is.EqualTo(
            """{"kind":"none","do":[{"kind":"set","path":["textContent"],"members":"required","content":true,"target":{"kind":"obj","id":"failed"},"value":{"kind":"as","expr":{"kind":"get","path":["status"],"members":"required","target":{"kind":"ref","name":"reply1"}},"shape":{"kind":"string","date":"none","negativeZero":"0"}}}]}"""));
        Assert.That(request.GetProperty("chained").GetRawText(), Is.EqualTo(
            """[{"kind":"set","path":["textContent"],"members":"required","content":true,"target":{"kind":"obj","id":"chained"},"value":{"kind":"as","expr":{"kind":"get","path":["value"],"members":"optional","target":{"kind":"get","path":["body"],"members":"required","target":{"kind":"ref","name":"reply1"}}},"shape":{"kind":"string","date":"none","negativeZero":"0"}}}]"""));
        Assert.That(RequestPage.Steps(plan)[1].GetRawText(), Is.EqualTo(
            """{"kind":"set","path":["textContent"],"members":"required","content":true,"target":{"kind":"obj","id":"after"},"value":{"kind":"lit","value":"After"}}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_json_success_reads_the_apps_member_names_and_starting_values()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get("/staff").OnSuccess<SearchAddress>((reply, pipeline) => pipeline.Element("ward").Text.Set(reply.Body.Read(body => body.Ward))),
            json => json.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseUpper);
        Assert.That(RequestPage.Request(plan).GetProperty("success").GetRawText(), Is.EqualTo(
            """{"kind":"json","do":[{"kind":"set","path":["textContent"],"members":"required","content":true,"target":{"kind":"obj","id":"ward"},"value":{"kind":"as","expr":{"kind":"data","path":[{"name":"WARD","requires":[],"absent":{"kind":"start","value":""}}],"target":{"kind":"get","path":["value"],"members":"optional","target":{"kind":"get","path":["body"],"members":"required","target":{"kind":"ref","name":"reply1"}}}},"shape":{"kind":"string","date":"none","negativeZero":"0"}}}]}"""));
        Assert.That(RequestPage.Field(RequestPage.Send(plan, "headers").GetProperty("items")[0], "value").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"lit","value":"application/json"}}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void an_error_route_does_not_change_the_success_accept_header()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Options("/staff").OnError<SearchAddress>(400, (reply, pipeline) => pipeline.Element("error").Text.Set(reply.Body.Read(body => body.Ward))).OnSuccess(Content.Html, (reply, pipeline) => { }));
        Assert.That(RequestPage.Request(plan).GetProperty("statuses")[0].GetProperty("kind").GetString(), Is.EqualTo("json"));
        Assert.That(RequestPage.Request(plan).GetProperty("success").GetProperty("kind").GetString(), Is.EqualTo("html"));
        Assert.That(RequestPage.Field(RequestPage.Field(RequestPage.Send(plan, "headers").GetProperty("items")[0], "value"), "value").GetRawText(), Is.EqualTo("""{"kind":"lit","value":"text/html"}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_file_success_reads_the_contracts_names_under_other_app_naming()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get("/export").OnSuccess(Content.File, (reply, pipeline) => pipeline.Element("file").Text.Set(reply.Body.Read(file => file.Name))),
            json => json.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseUpper);
        Assert.That(RequestPage.Request(plan).GetProperty("success").GetProperty("kind").GetString(), Is.EqualTo("file"));
        Assert.That(RequestPage.Request(plan).GetProperty("success").GetProperty("do")[0].GetProperty("value").GetProperty("expr").GetProperty("path").GetRawText(), Is.EqualTo(
            """[{"name":"name","requires":[],"absent":{"kind":"start","value":""}}]"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void head_status_and_catch_all_routes_have_no_body()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Head("/photo")
            .OnError(405, (reply, pipeline) => pipeline.Element("method").Text.Set(reply.Status))
            .OnSuccess((reply, pipeline) => { })
            .OnError((failed, pipeline) => pipeline.Element("failed").Text.Set(failed.Status))
            .Chained((reply, pipeline) => pipeline.Element("status").Text.Set(reply.Status)));
        Assert.That(RequestPage.Request(plan).GetProperty("statuses")[0].GetProperty("kind").GetString(), Is.EqualTo("none"));
        Assert.That(RequestPage.Request(plan).GetProperty("otherwise").GetProperty("kind").GetString(), Is.EqualTo("none"));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void the_typed_catch_all_reads_optional_json_and_an_outcome_choice()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get("/staff").OnError<SearchAddress>((failed, pipeline) =>
        {
            pipeline.Element("ward").Text.Set(failed.Body.Read(body => body.Ward));
            pipeline.When(failed.Outcome.Eq(FailureOutcome.Content)).Then(pipeline => pipeline.Element("status").Text.Set("Wrong content"));
        }), json => json.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper)));
        Assert.That(RequestPage.Request(plan).GetProperty("otherwise").GetProperty("kind").GetString(), Is.EqualTo("json"));
        Assert.That(RequestPage.Request(plan).GetProperty("otherwise").GetProperty("do")[1].GetProperty("when").GetProperty("args")[1].GetRawText(), Is.EqualTo("""{"kind":"lit","value":"content"}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void the_sent_request_uses_its_csharp_paths_under_json_renaming()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("/staff", request => request.Query(r => r.Name, "Ada"))
            .OnSuccess(Content.None, (reply, pipeline) => pipeline.Element("name").Text.Set(reply.Request.Read(sent => sent.Name))));
        Assert.That(RequestPage.Request(plan).GetProperty("success").GetProperty("do")[0].GetProperty("value").GetProperty("expr").GetRawText(), Is.EqualTo(
            """{"kind":"get","path":["Name"],"members":"optional","target":{"kind":"get","path":["request"],"members":"required","target":{"kind":"ref","name":"reply1"}}}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_reply_cannot_escape_its_routes_and_chained_block()
    {
        ReadOnly<int>? status = null;
        Assert.That(() => RequestPage.Rendered(pipeline =>
        {
            pipeline.Get("/staff").OnSuccess(Content.None, (reply, pipeline) => status = reply.Status);
            pipeline.Element("status").Text.Set(status!);
        }), Throws.InvalidOperationException.With.Message.EqualTo("A request's reply is read outside its route or Chained block; read it in that block or a block inside it."));
    }

    [Test]
    public void a_request_sentence_cannot_continue_after_another_step()
        => Assert.That(() => RequestPage.Rendered(pipeline =>
        {
            var request = pipeline.Get("/staff");
            pipeline.Element("status").Text.Set("Ready");
            request.Finally(pipeline => { });
        }), Throws.InvalidOperationException.With.Message.EqualTo("A request's next door continues its current stage before any other step of its block; write the request in one sentence."));

    [Test]
    public void a_retained_stage_cannot_be_written_twice()
        => Assert.That(() => RequestPage.Rendered(pipeline =>
        {
            var request = pipeline.Options("/staff");
            request.WhileLoading(pipeline => { });
            request.Finally(pipeline => { });
        }), Throws.InvalidOperationException.With.Message.EqualTo("A request's next door continues its current stage before any other step of its block; write the request in one sentence."));

    [TestCase(299, "OnError status 299 is outside 300 to 599; name an error status in that range.")]
    [TestCase(600, "OnError status 600 is outside 300 to 599; name an error status in that range.")]
    public void a_status_route_names_an_error_status(int status, string message)
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get("/staff").OnError(status, Content.None, (reply, pipeline) => { })),
            Throws.InvalidOperationException.With.Message.EqualTo(message));

    [Test]
    public void a_status_has_one_route()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get("/staff")
            .OnError(404, Content.None, (reply, pipeline) => { }).OnError(404, Content.None, (reply, pipeline) => { })),
            Throws.InvalidOperationException.With.Message.EqualTo("OnError already names status 404; write one route for that status."));

    [Test]
    public void a_request_has_one_catch_all()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Options("/staff").OnError((failed, pipeline) => { }).OnError((failed, pipeline) => { })),
            Throws.InvalidOperationException.With.Message.EqualTo("OnError already has a catch-all route; write one catch-all for the request."));

    [Test]
    public void a_request_has_one_loading_block()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get("/staff").WhileLoading(pipeline => { }).WhileLoading(pipeline => { })),
            Throws.InvalidOperationException.With.Message.EqualTo("WhileLoading is already written for this request; put its steps in one block."));

    [Test]
    public void a_request_has_one_finally_block()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Head("/staff").Finally(pipeline => { }).Finally(pipeline => { })),
            Throws.InvalidOperationException.With.Message.EqualTo("Finally is already written for this request; put its steps in one block."));

    [TestCase("Set-Cookie", "Response header 'Set-Cookie' is withheld by the browser; read an exposed response header.")]
    [TestCase("sEt-CoOkIe2", "Response header 'sEt-CoOkIe2' is withheld by the browser; read an exposed response header.")]
    public void a_response_header_the_browser_withholds_is_refused(string name, string message)
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Head("/photo").OnSuccess((reply, pipeline) => pipeline.Element("header").Text.Set(reply.Headers.Get(name)))),
            Throws.InvalidOperationException.With.Message.EqualTo(message));
}
