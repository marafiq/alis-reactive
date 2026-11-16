using System;
using System.Collections.Generic;
using Alis.Reactive.Declarations;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Validation;
using Alis.Reactive.Vendors;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.RequestsAndResponses;

[TestFixture]
public sealed class WhenAPartsConditionIsWritten
{
    private const string Urgent = """{"kind":"get","path":["urgent"],"members":"required","target":{"kind":"obj","id":"desk"}}""";
    private const string Guard = """{"kind":"ref","name":"partCondition1"}""";
    private const string Sent = """{"kind":"lit","value":true}""";
    private const string ReadsNothing = """{"kind":"op","op":"nullableValue","args":[{"kind":"lit","value":null}]}""";
    private const string UrgentHeader =
        """{"kind":"record","fields":[{"name":"name","value":{"kind":"lit","value":"X-Urgent"}},{"name":"value","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"lit","value":"yes"}}]}},{"name":"form","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"text"}}]}},{"name":"when","value":{"kind":"ref","name":"partCondition1"}}]}""";

    private static readonly string Condition =
        """{"kind":"let","name":"partCondition1","await":false,"binds":"line","expr":{"kind":"op","op":"cond","args":[""" + Urgent + """,{"kind":"lit","value":true},{"kind":"lit","value":false}]}}""";

    private static IEnumerable<TestCaseData> StagesAfterTheBodyOrTheContainer()
    {
        yield return new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline =>
            pipeline.Post<SearchRequest>("/notes", request => request.Json(body => body.Set(r => r.Id, 7))
                .When(pipeline.Component<PartsDesk>("desk").Urgent).Then(urgent => urgent.Header("X-Urgent", "yes"))))).SetArgDisplayNames("after Json");
        yield return new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline =>
            pipeline.Post<NoteRequest>("/notes", request => request.Text("Night round done")
                .When(pipeline.Component<PartsDesk>("desk").Urgent).Then(urgent => urgent.Header("X-Urgent", "yes"))))).SetArgDisplayNames("after Text");
        yield return new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline =>
            pipeline.Get<SearchRequest>("/residents", request => request.Inputs(pipeline.Container<SearchRequest>("search"))
                .When(pipeline.Component<PartsDesk>("desk").Urgent).Then(urgent => urgent.Header("X-Urgent", "yes"))))).SetArgDisplayNames("after Inputs");
    }

    [Test]
    public void a_gets_query_member_and_header_carry_the_condition_read_once_as_a_boolean_before_the_request()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("/residents", request => request.Query(r => r.Id, 7)
            .When(pipeline.Component<PartsDesk>("desk").Urgent).Then(urgent => urgent.Query(r => r.Count, 2).Header("X-Urgent", "yes"))));
        var fields = RequestPage.Field(RequestPage.Send(plan, "query"), "fields").GetProperty("items");
        var headers = RequestPage.Send(plan, "headers").GetProperty("items");
        Assert.That(RequestPage.Steps(plan)[0].GetRawText(), Is.EqualTo(Condition));
        Assert.That(RequestPage.Field(fields[0], "when").GetRawText(), Is.EqualTo(Sent));
        Assert.That(RequestPage.Field(fields[1], "when").GetRawText(), Is.EqualTo(Guard));
        Assert.That(headers[0].GetRawText(), Is.EqualTo(UrgentHeader));
        Assert.That(RequestPage.Field(headers[1], "when").GetRawText(), Is.EqualTo(Sent));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_posts_member_under_a_condition_is_sent_as_multipart_and_its_source_is_read_only_when_the_condition_holds()
    {
        var plan = RequestPage.Rendered(pipeline =>
        {
            var desk = pipeline.Component<PartsDesk>("desk");
            pipeline.Post<SearchRequest>("/notes", request => request.When(desk.Urgent).Then(note => note.Set(r => r.Name, desk.Note)));
        });
        var body = RequestPage.Send(plan, "body");
        Assert.That(RequestPage.Steps(plan)[0].GetRawText(), Is.EqualTo(Condition));
        Assert.That(RequestPage.Steps(plan)[1].GetRawText(), Is.EqualTo(
            """{"kind":"let","name":"requestValue1","await":false,"binds":"line","expr":{"kind":"op","op":"cond","args":[""" + Guard +
            """,{"kind":"get","path":["note"],"members":"required","target":{"kind":"obj","id":"desk"}},""" + ReadsNothing + "]}}"));
        Assert.That(RequestPage.Field(body, "kind").GetRawText(), Is.EqualTo("""{"kind":"lit","value":"multipart"}"""));
        Assert.That(RequestPage.Field(RequestPage.Field(RequestPage.Field(body, "parts").GetProperty("items")[0], "field"), "when").GetRawText(), Is.EqualTo(Guard));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_json_member_under_a_condition_is_listed_with_its_condition_and_so_is_a_holder_only_guarded_members_fill()
    {
        var plan = RequestPage.Rendered(pipeline =>
        {
            var desk = pipeline.Component<PartsDesk>("desk");
            pipeline.Post<SearchRequest>("/notes", request => request.Json(body => body.Set(r => r.Id, 7)
                .When(desk.Urgent).Then(urgent => urgent.Set(r => r.Address.Ward, "North").Set(r => r.Name, desk.Note))));
        });
        Assert.That(RequestPage.Field(RequestPage.Send(plan, "body"), "guarded").GetRawText(), Is.EqualTo(
            """{"kind":"list","items":[""" +
            """{"kind":"record","fields":[{"name":"path","value":{"kind":"list","items":[{"kind":"lit","value":"address"}]}},{"name":"when","value":""" + Guard + "}]}," +
            """{"kind":"record","fields":[{"name":"path","value":{"kind":"list","items":[{"kind":"lit","value":"address"},{"kind":"lit","value":"ward"}]}},{"name":"when","value":""" + Guard + "}]}," +
            """{"kind":"record","fields":[{"name":"path","value":{"kind":"list","items":[{"kind":"lit","value":"not.the.binding"}]}},{"name":"when","value":""" + Guard + "}]}]}"));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_json_body_with_no_condition_lists_no_member()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Post<SearchRequest>("/notes", request => request.Json(body => body.Set(r => r.Id, 7).Set(r => r.Address.Ward, "North"))));
        Assert.That(RequestPage.Field(RequestPage.Send(plan, "body"), "guarded").GetRawText(), Is.EqualTo("""{"kind":"list","items":[]}"""));
    }

    [Test]
    public void a_url_encoded_literal_under_a_condition_is_held_as_written_and_carries_the_condition()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Post<SearchRequest>("/notes", request => request.UrlEncoded(body => body
            .When(pipeline.Component<PartsDesk>("desk").Urgent).Then(urgent => urgent.Set(r => r.Id, 7)))));
        var field = RequestPage.Field(RequestPage.Send(plan, "body"), "fields").GetProperty("items")[0];
        Assert.That(RequestPage.Field(field, "when").GetRawText(), Is.EqualTo(Guard));
        Assert.That(RequestPage.Field(field, "value").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"lit","value":7}}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void the_check_reads_a_member_under_a_condition_as_nothing_where_the_condition_fails()
    {
        var html = Views.Of(new Transfer(), _ => { }, app =>
        {
            app.AddReactiveRequests(requests => requests.NoAntiforgery());
            app.AddReactiveFluentValidation(validation => validation.Add<TransferRules>());
        });
        var reactive = html.ReactivePlan<Transfer>();
        html.On(reactive, trigger => trigger.DomReady(pipeline => pipeline.Post<Transfer>("/transfers", request => request
            .When(pipeline.Component<PartsDesk>("desk").Urgent).Then(urgent => urgent.Set(r => r.Ward, "North")))));
        var plan = Views.TextOf(html.RenderPlan(reactive))["<script type=\"application/json\" data-reactive-plan>".Length..^"</script>".Length];
        Assert.That(RequestPage.Request(plan).GetProperty("check")[0].GetProperty("expr").GetProperty("args")[2].GetRawText(), Is.EqualTo(
            """{"kind":"list","items":[{"kind":"record","fields":[{"name":"path","value":{"kind":"list","items":[{"kind":"lit","value":"ward"}]}},{"name":"value","value":{"kind":"op","op":"cond","args":[""" + Guard +
            """,{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"lit","value":"North"}}]},{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"nothing"}}]}]}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_member_that_fills_the_url_under_a_condition_is_refused_where_the_view_renders_and_the_same_member_outside_it_fills_the_url()
    {
        const string urlUnderACondition =
            "'r => r.Id' fills a placeholder of the URL '/residents/{id}' under a condition on the request's parts, and the URL takes no condition: a request that differs in its URL is a second request under the pipeline's When.";
        Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Get<SearchRequest>("/residents/{id}", request => request
            .When(pipeline.Component<PartsDesk>("desk").Urgent).Then(urgent => urgent.Path(r => r.Id, 7)))), Throws.InvalidOperationException.With.Message.EqualTo(urlUnderACondition));
        Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Post<SearchRequest>("/residents/{id}", request => request
            .When(pipeline.Component<PartsDesk>("desk").Urgent).Then(urgent => urgent.Set(r => r.Id, 7)))), Throws.InvalidOperationException.With.Message.EqualTo(urlUnderACondition));
        var plan = RequestPage.Rendered(pipeline => pipeline.Post<SearchRequest>("/residents/{id}", request => request.Set(r => r.Id, 7)
            .When(pipeline.Component<PartsDesk>("desk").Urgent).Then(urgent => urgent.Set(r => r.Count, 2))));
        Assert.That(RequestPage.Send(plan, "url").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"parts","value":{"kind":"list","items":[{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"text"}},{"name":"text","value":{"kind":"lit","value":"/residents/"}}]},{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"member"}},{"name":"name","value":{"kind":"lit","value":"Id"}},{"name":"value","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"lit","value":7}}]}},{"name":"form","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"number"}}]}}]}]}},{"name":"query","value":{"kind":"lit","value":""}}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_file_part_under_a_condition_carries_it_and_its_file_is_read_only_when_the_condition_holds()
    {
        var plan = RequestPage.Rendered(pipeline =>
        {
            var files = pipeline.Component<BodyDesk>("files");
            pipeline.Post<NoteRequest>("/notes", request => request.Multipart(body => body.When(pipeline.Component<PartsDesk>("desk").Urgent).Then(urgent => urgent.File(r => r.Photo, files.Photo))));
        });
        Assert.That(RequestPage.Field(RequestPage.Field(RequestPage.Send(plan, "body"), "parts").GetProperty("items")[0], "when").GetRawText(), Is.EqualTo(Guard));
        Assert.That(RequestPage.Steps(plan)[1].GetRawText(), Is.EqualTo(
            """{"kind":"let","name":"requestValue1","await":false,"binds":"line","expr":{"kind":"op","op":"cond","args":[""" + Guard +
            """,{"kind":"get","path":["photo"],"members":"required","target":{"kind":"obj","id":"files"}},""" + ReadsNothing + "]}}"));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [TestCaseSource(nameof(StagesAfterTheBodyOrTheContainer))]
    public void a_header_after_the_body_or_the_container_takes_a_condition(Action<Pipeline<RequestDesk>> write)
    {
        var plan = RequestPage.Rendered(write);
        Assert.That(RequestPage.Send(plan, "headers").GetProperty("items")[0].GetRawText(), Is.EqualTo(UrgentHeader));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_condition_inside_a_branch_holds_only_when_the_branchs_own_condition_holds()
    {
        var plan = RequestPage.Rendered(pipeline =>
        {
            var desk = pipeline.Component<PartsDesk>("desk");
            pipeline.Get<SearchRequest>("/residents", request => request.When(desk.Urgent).Then(urgent => urgent.When(desk.Night).Then(night => night.Query(r => r.Count, 2))));
        });
        Assert.That(RequestPage.Steps(plan)[1].GetRawText(), Is.EqualTo(
            """{"kind":"let","name":"partCondition2","await":false,"binds":"line","expr":{"kind":"op","op":"and","args":[""" + Guard +
            """,{"kind":"op","op":"cond","args":[{"kind":"get","path":["night"],"members":"required","target":{"kind":"obj","id":"desk"}},{"kind":"lit","value":true},{"kind":"lit","value":false}]}]}}"""));
        Assert.That(RequestPage.Field(RequestPage.Field(RequestPage.Send(plan, "query"), "fields").GetProperty("items")[0], "when").GetRawText(), Is.EqualTo(
            """{"kind":"ref","name":"partCondition2"}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_default_header_is_sent_where_the_condition_of_the_requests_own_header_of_its_name_fails()
    {
        var plan = RequestPage.RenderedIn(app => app.AddReactiveRequests(requests => requests.NoAntiforgery().Header("X-Desk", "front")),
            pipeline => pipeline.Get<SearchRequest>("/residents", request => request.When(pipeline.Component<PartsDesk>("desk").Urgent).Then(urgent => urgent.Header("x-desk", "triage"))));
        var headers = RequestPage.Send(plan, "headers").GetProperty("items");
        Assert.That(RequestPage.Field(headers[0], "name").GetRawText(), Is.EqualTo("""{"kind":"lit","value":"X-Desk"}"""));
        Assert.That(RequestPage.Field(headers[0], "when").GetRawText(), Is.EqualTo(
            """{"kind":"op","op":"cond","args":[""" + Guard + """,{"kind":"lit","value":false},{"kind":"lit","value":true}]}"""));
        Assert.That(RequestPage.Field(headers[1], "name").GetRawText(), Is.EqualTo("""{"kind":"lit","value":"x-desk"}"""));
        Assert.That(RequestPage.Field(headers[1], "when").GetRawText(), Is.EqualTo(Guard));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_named_input_under_a_condition_is_looked_up_only_when_the_condition_holds_and_left_out_otherwise()
    {
        var html = Views.Of(new IntakeDesk(), _ => { }, app => app.AddReactiveRequests(requests => requests.NoAntiforgery()));
        var reactive = html.ReactivePlan<IntakeDesk>();
        html.On(reactive, trigger => trigger.DomReady(pipeline => pipeline.Post<IntakeSend>("/intakes", request => request
            .When(pipeline.Component<PartsDesk>("desk").Urgent).Then(ward => ward.Input(r => r.Ward, pipeline.Input(m => m.Ward))))));
        var plan = Views.TextOf(html.RenderPlan(reactive))["<script type=\"application/json\" data-reactive-plan>".Length..^"</script>".Length];
        Assert.That(RequestPage.Field(RequestPage.Field(RequestPage.Send(plan, "body"), "inputs"), "individual").GetRawText(), Is.EqualTo(
            """{"kind":"list","items":[{"kind":"op","op":"cond","args":[""" + Guard +
            """,{"kind":"call","path":["input"],"members":"required","target":{"kind":"obj","id":"$model"},"args":[{"kind":"lit","value":"Alis_Reactive_ModuleTests_RequestsAndResponses_IntakeDesk__Ward"},{"kind":"lit","value":"Ward"}]},""" +
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"leftOut"}}]}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }
}

public sealed class PartsDesk : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public ReadOnly<bool> Urgent => ReadOnly<bool>("urgent");

    public ReadOnly<bool> Night => ReadOnly<bool>("night");

    public ReadOnly<string?> Note => ReadOnly<string?>("note");
}
