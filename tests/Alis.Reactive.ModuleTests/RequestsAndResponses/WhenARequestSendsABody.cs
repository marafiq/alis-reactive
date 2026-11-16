using System;
using System.Collections.Generic;
using System.Text.Json;
using Alis.Reactive.Declarations;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Vendors;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.RequestsAndResponses;

[TestFixture]
public sealed class WhenARequestSendsABody
{
    private const string None = """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"none"}}]}""";
    private const string NoInputs = """{"kind":"record","fields":[{"name":"container","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"none"}}]}},{"name":"individual","value":{"kind":"list","items":[]}}]}""";
    private const string Token = """{"kind":"lit","value":"token"}""";
    private const string InTheHeader = """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"header"}},{"name":"name","value":{"kind":"lit","value":"RequestVerificationToken"}}]}""";
    private const string InTheField = """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"field"}},{"name":"name","value":{"kind":"lit","value":"__RequestVerificationToken"}}]}""";

    private static readonly TestCaseData[] UnsafeRequests =
    [
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Post("/notes"))).SetArgDisplayNames("POST"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Put<SearchRequest>("/notes", request => request.Json(body => { })))).SetArgDisplayNames("PUT"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Delete<SearchRequest>("/notes", request => request.Text("gone")))).SetArgDisplayNames("DELETE"),
    ];

    private static readonly TestCaseData[] BodiesNoHostReadsATokenIn =
    [
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Post<NoteRequest>("/notes", request => request.Json(body => { }))), "JSON").SetArgDisplayNames("JSON"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Post<NoteRequest>("/notes", request => request.Text("note"))), "text").SetArgDisplayNames("text"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Post<NoteRequest>("/notes", request => request.File(pipeline.Component<BodyDesk>("desk").Photo))), "file").SetArgDisplayNames("file"),
    ];

    private static readonly TestCaseData[] RequestsOfEachKind =
    [
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Get("/notes")), "'GET /notes'").SetArgDisplayNames("GET"),
        new TestCaseData((Action<Pipeline<RequestDesk>>)(pipeline => pipeline.Delete("/notes")), "'DELETE /notes'").SetArgDisplayNames("DELETE"),
    ];

    [Test]
    public void a_post_with_no_body_sends_none_and_no_token_when_the_app_chose_none()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Post("/notes"));
        Assert.That(RequestPage.Send(plan, "method").GetRawText(), Is.EqualTo("""{"kind":"lit","value":"POST"}"""));
        Assert.That(RequestPage.Send(plan, "body").GetRawText(), Is.EqualTo(None));
        Assert.That(RequestPage.Send(plan, "antiforgery").GetRawText(), Is.EqualTo(None));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_json_body_names_its_members_by_the_apps_json_and_nests_them()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Put<SearchRequest>("/notes", request => request.Json(body => body
            .Set(r => r.Id, 7).Set(r => r.Address.Ward, "North").Set(r => r.Name, "Ada").Set(r => r.On, new DateTime(2026, 1, 12, 9, 30, 0, DateTimeKind.Utc)))));
        Assert.That(RequestPage.Send(plan, "body").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"json"}},{"name":"value","value":{"kind":"record","fields":[{"name":"id","value":{"kind":"lit","value":7}},{"name":"address","value":{"kind":"record","fields":[{"name":"ward","value":{"kind":"lit","value":"North"}}]}},{"name":"not.the.binding","value":{"kind":"lit","value":"Ada"}},{"name":"on","value":{"kind":"lit","value":"2026-01-12T09:30:00"}}]}},{"name":"guarded","value":{"kind":"list","items":[]}},{"name":"inputs","value":""" + NoInputs + "}]}"));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_json_body_that_sets_nothing_sends_an_empty_object()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Delete<SearchRequest>("/notes", request => request.Json(body => { })));
        Assert.That(RequestPage.Field(RequestPage.Send(plan, "body"), "value").GetRawText(), Is.EqualTo("""{"kind":"record","fields":[]}"""));
        Assert.That(RequestPage.Send(plan, "method").GetRawText(), Is.EqualTo("""{"kind":"lit","value":"DELETE"}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_json_body_leaves_out_a_value_type_or_list_that_reads_nothing_and_writes_null_for_a_nullable()
    {
        var plan = RequestPage.Rendered(pipeline =>
        {
            var desk = pipeline.Component<BodyDesk>("desk");
            pipeline.Post<NoteRequest>("/notes", request => request.Json(body => body.Set(r => r.Id, desk.Count).Set(r => r.Beds, desk.OptionalCount).Set(r => r.Rooms, desk.Rooms)));
        });
        Assert.That(RequestPage.Field(RequestPage.Send(plan, "body"), "value").GetRawText(), Is.EqualTo(
            """{"kind":"as","expr":{"kind":"record","fields":[{"name":"id","value":{"kind":"ref","name":"requestValue1"}},{"name":"beds","value":{"kind":"ref","name":"requestValue2"}},{"name":"rooms","value":{"kind":"ref","name":"requestValue3"}}]},"shape":{"kind":"object","fields":{"id":{"shape":{"kind":"number","of":"fraction","text":"invariant","overflow":"nothing"},"nothing":"omit"},"rooms":{"shape":{"kind":"array","item":{"kind":"number","of":"fraction","text":"invariant","overflow":"nothing"}},"nothing":"omit"}}}}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_literal_null_list_is_left_out_of_a_json_body_and_a_literal_null_text_is_null()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Post<SearchRequest>("/notes", request => request.Json(body => body.Set(r => r.Ids, (int[])null!).Set(r => r.Name, (string?)null))));
        Assert.That(RequestPage.Field(RequestPage.Send(plan, "body"), "value").GetRawText(), Is.EqualTo(
            """{"kind":"as","expr":{"kind":"record","fields":[{"name":"ids","value":{"kind":"lit","value":null}},{"name":"not.the.binding","value":{"kind":"lit","value":null}}]},"shape":{"kind":"object","fields":{"ids":{"shape":{"kind":"array","item":{"kind":"number","of":"fraction","text":"invariant","overflow":"nothing"}},"nothing":"omit"}}}}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_reply_refuses_to_read_back_a_member_the_json_body_sends_and_reads_one_form_data_sends()
    {
        Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Post<SearchRequest>("/notes", request => request.Json(body => body.Set(r => r.Id, 7)))
                .OnSuccess(Content.None, (saved, pipeline) => pipeline.Element("id").Text.Set(saved.Request.Read(r => r.Id)))),
            Throws.InstanceOf<NotImplementedException>().With.Message.EqualTo(
                "'r => r.Id' is sent in the request's JSON body, and a reply reads a sent member back by its C# path, which the plan's JSON body does not carry (schema.md, HttpSend's json body: its value and inputs); " +
                "read the value from its source, or send the member in the path, query or form data."));
        var form = RequestPage.Rendered(pipeline => pipeline.Post<SearchRequest>("/notes", request => request.UrlEncoded(body => body.Set(r => r.Id, 7)))
            .OnSuccess(Content.None, (saved, pipeline) => pipeline.Element("id").Text.Set(saved.Request.Read(r => r.Id))));
        Assert.That(PlanSchema.ProblemsOf(form), Is.Empty);
    }

    [Test]
    public void a_date_a_source_holds_is_sent_in_json_as_its_wall_text()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Post<NoteRequest>("/notes", request => request.Json(body => body.Set(r => r.On, pipeline.Component<BodyDesk>("desk").On))));
        Assert.That(RequestPage.Field(RequestPage.Send(plan, "body"), "value").GetRawText(), Is.EqualTo(
            """{"kind":"as","expr":{"kind":"record","fields":[{"name":"on","value":{"kind":"as","expr":{"kind":"ref","name":"requestValue1"},"shape":{"kind":"string","date":"date","negativeZero":"0"}}}]},"shape":{"kind":"object","fields":{"on":{"shape":{"kind":"string","date":"none","negativeZero":"0"},"nothing":"omit"}}}}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_member_that_fills_a_placeholder_is_sent_in_the_body_too()
    {
        var form = RequestPage.Rendered(pipeline => pipeline.Post<SearchRequest>("/notes/{id}", request => request.Path(r => r.Id, 7).UrlEncoded(body => body.Set(r => r.Name, "Ada"))));
        Assert.That(Names(RequestPage.Field(RequestPage.Send(form, "body"), "fields")), Is.EqualTo(new[] { "Id", "Name" }));
        var json = RequestPage.Rendered(pipeline => pipeline.Post<SearchRequest>("/notes/{id}", request => request.Json(body => body.Set(r => r.Name, "Ada")).Path(r => r.Id, 7)));
        Assert.That(RequestPage.Field(RequestPage.Send(json, "body"), "value").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"id","value":{"kind":"lit","value":7}},{"name":"not.the.binding","value":{"kind":"lit","value":"Ada"}}]}"""));
        Assert.That(PlanSchema.ProblemsOf(form), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void form_data_names_each_field_by_its_csharp_path_and_a_multipart_field_is_a_part()
    {
        var plan = RequestPage.Rendered(pipeline => pipeline.Post<SearchRequest>("/notes", request => request.Multipart(body => body.Set(r => r.Name, "Ada").Set(r => r.Address.Ward, "North"))));
        var parts = RequestPage.Field(RequestPage.Send(plan, "body"), "parts");
        Assert.That(RequestPage.Field(parts.GetProperty("items")[0], "kind").GetRawText(), Is.EqualTo("""{"kind":"lit","value":"field"}"""));
        Assert.That(RequestPage.Field(RequestPage.Field(parts.GetProperty("items")[1], "field"), "name").GetRawText(), Is.EqualTo("""{"kind":"lit","value":"Address.Ward"}"""));
        Assert.That(RequestPage.Field(RequestPage.Field(parts.GetProperty("items")[0], "field"), "name").GetRawText(), Is.EqualTo("""{"kind":"lit","value":"Name"}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_file_part_sends_one_file_or_a_list_under_its_members_path()
    {
        var plan = RequestPage.Rendered(pipeline =>
        {
            var desk = pipeline.Component<BodyDesk>("desk");
            pipeline.Post<NoteRequest>("/notes", request => request.Multipart(body => body.File(r => r.Photo, desk.Photo).File(r => r.Scans, desk.Scans)));
        });
        var parts = RequestPage.Field(RequestPage.Send(plan, "body"), "parts").GetProperty("items");
        Assert.That(parts[0].GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"file"}},{"name":"name","value":{"kind":"lit","value":"Photo"}},{"name":"value","value":{"kind":"op","op":"cond","args":[{"kind":"op","op":"neq","args":[{"kind":"ref","name":"requestValue1"},{"kind":"lit","value":null}]},{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"list","items":[{"kind":"ref","name":"requestValue1"}]}}]},{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"nothing"}}]}]}},{"name":"when","value":{"kind":"lit","value":true}}]}"""));
        Assert.That(RequestPage.Field(RequestPage.Field(parts[1], "value").GetProperty("args")[1], "value").GetRawText(), Is.EqualTo(
            """{"kind":"op","op":"toArray","args":[{"kind":"ref","name":"requestValue2"}]}"""));
        Assert.That(RequestPage.Steps(plan)[0].GetRawText(), Is.EqualTo(
            """{"kind":"let","name":"requestValue1","await":false,"binds":"line","expr":{"kind":"get","path":["photo"],"members":"required","target":{"kind":"obj","id":"desk"}}}"""));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_text_or_a_file_is_the_whole_body()
    {
        var text = RequestPage.Rendered(pipeline => pipeline.Post<NoteRequest>("/notes", request => request.Text("Night round done")));
        Assert.That(RequestPage.Send(text, "body").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"text"}},{"name":"value","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"lit","value":"Night round done"}}]}},{"name":"inputs","value":""" + NoInputs + "}]}"));
        var file = RequestPage.Rendered(pipeline => pipeline.Put<NoteRequest>("/notes/photo", request => request.File(pipeline.Component<BodyDesk>("desk").Photo)));
        Assert.That(RequestPage.Field(RequestPage.Send(file, "body"), "kind").GetRawText(), Is.EqualTo("""{"kind":"lit","value":"file"}"""));
        Assert.That(PlanSchema.ProblemsOf(text), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(file), Is.Empty);
    }

    [TestCaseSource(nameof(UnsafeRequests))]
    public void the_header_place_sends_the_hosts_token_with_each_unsafe_method_to_the_pages_origin(Action<Pipeline<RequestDesk>> write)
    {
        var plan = Choosing(AntiforgeryIn.Header, write);
        var token = RequestPage.Send(plan, "antiforgery");
        Assert.That(RequestPage.Field(token, "kind").GetRawText(), Is.EqualTo(Token));
        Assert.That(RequestPage.Field(token, "place").GetRawText(), Is.EqualTo(InTheHeader));
        Assert.That(RequestPage.Field(token, "value").GetProperty("value").GetString(), Is.Not.Empty);
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
    }

    [Test]
    public void a_get_and_a_request_to_another_origin_send_no_token()
    {
        Assert.That(RequestPage.Send(Choosing(AntiforgeryIn.Header, pipeline => pipeline.Get("/notes")), "antiforgery").GetRawText(), Is.EqualTo(None));
        Assert.That(RequestPage.Send(Choosing(AntiforgeryIn.Header, pipeline => pipeline.Post("https://elsewhere.example/notes")), "antiforgery").GetRawText(), Is.EqualTo(None));
        Assert.That(RequestPage.Send(Choosing(AntiforgeryIn.FormField, pipeline => pipeline.Post("https://elsewhere.example/notes")), "body").GetRawText(), Is.EqualTo(None));
    }

    [Test]
    public void the_field_place_sends_a_url_encoded_body_holding_the_field_alone_when_the_request_names_none()
    {
        var plan = Choosing(AntiforgeryIn.FormField, pipeline => pipeline.Post("/notes"));
        Assert.That(RequestPage.Field(RequestPage.Send(plan, "antiforgery"), "place").GetRawText(), Is.EqualTo(InTheField));
        Assert.That(RequestPage.Send(plan, "body").GetRawText(), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"urlEncoded"}},{"name":"fields","value":{"kind":"list","items":[]}},{"name":"inputs","value":""" + NoInputs + "}]}"));
        var multipart = Choosing(AntiforgeryIn.FormField, pipeline => pipeline.Put<SearchRequest>("/notes", request => request.Multipart(body => body.Set(r => r.Name, "Ada"))));
        Assert.That(RequestPage.Field(RequestPage.Send(multipart, "antiforgery"), "place").GetRawText(), Is.EqualTo(InTheField));
        Assert.That(PlanSchema.ProblemsOf(plan), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(multipart), Is.Empty);
    }

    [TestCaseSource(nameof(BodiesNoHostReadsATokenIn))]
    public void the_field_place_refuses_a_body_no_host_reads_a_token_in(Action<Pipeline<RequestDesk>> write, string format)
        => Assert.That(() => Choosing(AntiforgeryIn.FormField, write),
            Throws.InvalidOperationException.With.Message.EqualTo(
                $"'POST /notes' sends a {format} body, and the app sends the antiforgery token in the form field __RequestVerificationToken, which no host reads in that body; " +
                "send a URL-encoded or multipart body, or call Antiforgery(AntiforgeryIn.Header) in AddReactiveRequests."));

    [Test]
    public void a_post_in_an_app_that_made_no_antiforgery_choice_is_refused()
        => Assert.That(() => RequestPage.RenderedIn(app => app.AddReactiveRequests(requests => { }), pipeline => pipeline.Post("/notes")),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'POST /notes' sends no antiforgery token, since the app chose none: call Antiforgery(AntiforgeryIn.Header), Antiforgery(AntiforgeryIn.FormField) or NoAntiforgery() in AddReactiveRequests."));

    [Test]
    public void a_get_in_an_app_that_made_no_antiforgery_choice_is_written()
        => Assert.That(PlanSchema.ProblemsOf(RequestPage.RenderedIn(app => app.AddReactiveRequests(requests => { }), pipeline => pipeline.Get("/notes"))), Is.Empty);

    [TestCaseSource(nameof(RequestsOfEachKind))]
    public void a_request_in_an_app_that_never_called_add_reactive_requests_is_refused(Action<Pipeline<RequestDesk>> write, string request)
        => Assert.That(() => RequestPage.RenderedIn(_ => { }, write),
            Throws.InvalidOperationException.With.Message.EqualTo(
                $"{request} is rendered in an app that never called AddReactiveRequests; call services.AddReactiveRequests(requests => ..) once at startup."));

    [Test]
    public void an_app_makes_one_antiforgery_choice()
        => Assert.That(() => RequestPage.RenderedIn(app => app.AddReactiveRequests(requests => requests.NoAntiforgery().Antiforgery(AntiforgeryIn.Header)), pipeline => pipeline.Get("/notes")),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "AddReactiveRequests makes one antiforgery choice; call Antiforgery(AntiforgeryIn.Header), Antiforgery(AntiforgeryIn.FormField) or NoAntiforgery() once."));

    [Test]
    public void an_enum_a_source_holds_in_a_json_body_names_the_node_it_waits_on()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Post<NoteRequest>("/notes", request => request.Json(body => body.Set(r => r.Shift, pipeline.Component<SearchPanel>("search").Shift)))),
            Throws.InstanceOf<NotImplementedException>().With.Message.StartsWith("'r => r.Shift' holds an enum a source reads, and a JSON body sends it as its endpoint binds it (D9.13)"));

    [Test]
    public void a_json_body_literals_member_past_javascripts_whole_numbers_is_refused_naming_its_path()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Post<BillRequest>("/bills", request => request.Json(body => body.Set(r => r.Charge, new BilledCharge(9007199254740993m))))),
            Throws.InvalidOperationException.With.Message.EqualTo("'r => r.Charge.amount' holds a JavaScript number, and 9007199254740993 is a whole number past 9007199254740991, which JavaScript reads as another value; write a finite number of at most 15 significant digits."));

    [Test]
    public void a_member_sent_in_the_path_and_set_in_the_body_is_refused()
        => Assert.That(() => RequestPage.Rendered(pipeline => pipeline.Post<SearchRequest>("/notes/{id}", request => request.Path(r => r.Id, 7).Json(body => body.Set(r => r.Id, 8)))),
            Throws.InvalidOperationException.With.Message.EqualTo("Request member 'Id' overlaps a member already supplied; send each member once, and send either an object or its members."));

    private static string Choosing(AntiforgeryIn place, Action<Pipeline<RequestDesk>> write)
        => RequestPage.RenderedIn(app => app.AddReactiveRequests(requests => requests.Antiforgery(place)), write);

    private static string[] Names(JsonElement fields)
    {
        var names = new List<string>();
        foreach (var field in fields.GetProperty("items").EnumerateArray()) names.Add(RequestPage.Field(field, "name").GetProperty("value").GetString()!);
        return [.. names];
    }
}

public sealed class NoteRequest
{
    public int Id { get; init; }

    public int? Beds { get; init; }

    public DateTime On { get; init; }

    public List<int> Rooms { get; init; } = [];

    public RequestShift Shift { get; init; }

    public IFormFile? Photo { get; init; }

    public List<IFormFile> Scans { get; init; } = [];
}

public sealed class BodyDesk : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public ReadOnly<int> Count => ReadOnly<int>("count");

    public ReadOnly<int?> OptionalCount => ReadOnly<int?>("optionalCount");

    public ReadOnly<List<int>> Rooms => ReadOnly<List<int>>("rooms");

    public ReadOnly<DateTime> On => ReadOnly<DateTime>("on");

    public BrowserFile Photo => LiveObject<BrowserFile>("photo");

    public LiveObjects<BrowserFile> Scans => LiveObjects<BrowserFile>("scans");
}
