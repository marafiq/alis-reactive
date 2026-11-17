using System;
using System.Linq;
using System.Text.Json;
using Alis.Reactive.Conditions;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Vendors;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenARowActionIsDeclared
{
    private const string Head =
        """{"version":7,"planId":"Alis.Reactive.ModuleTests.Vendors.Ward","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"}""";

    private const string Native =
        ""","vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}}""";

    private const string NoVendor = ""","vendors":{}""";

    private const string DocumentAndRows =
        ""","objects":{"$document":{"kind":"plugin","name":"alis.document","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}},"$plugin:ward-rows":{"kind":"plugin","name":"ward-rows","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}}""";

    private const string Status = ""","status":{"kind":"element","id":"status","vendor":"native"}""";

    private const string OnLoad = """{"on":"$document","path":[],"event":"ready","payload":[],"members":"required","policy":"overlap","do":[""";

    private const string ShownAsText = ""","shape":{"kind":"string","date":"none","negativeZero":"0"}}""";

    private static readonly ResidentRow Ada = new(1, "Ada", "North", new ResidentRoom("12", 1));

    private static readonly ResidentRow Bea = new(2, "Bea", "South", new ResidentRoom("14", 2));

    private static string Listen(string policy, int listen)
        => "{\"kind\":\"listen\",\"event\":\"chosen\",\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"},\"release\":{\"kind\":\"living\"},\"payload\":[0],\"members\":\"required\",\"policy\":\"" +
            policy + "\",\"payloadName\":\"listenPayload" + listen + "\",\"argumentsName\":\"listenArguments" + listen + "\",\"object\":{\"kind\":\"ref\",\"name\":\"result" + listen + "\"},\"do\":[";

    [Test]
    public void one_action_rendered_for_two_rows_writes_one_handler_one_call_one_listen_and_an_entry_per_row()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        var discharge = plan.Rows<ResidentRow>("residents").WardButton(HandlerPolicy.Overlap, (chosen, pipeline) => pipeline.Element("status").Text.Set(chosen.Row.Read(row => row.Name)));
        var adas = discharge.Render(Ada, "Discharge");
        var beas = discharge.Render(Bea, "Discharge");
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(adas, Is.EqualTo("""<button type="button" id="residents-0-0">Discharge</button>"""));
        Assert.That(beas, Is.EqualTo("""<button type="button" id="residents-0-1">Discharge</button>"""));
        Assert.That(json, Is.EqualTo(Head + Native + DocumentAndRows + Status + """},"handlers":[""" + OnLoad +
            """{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["controls"],"members":"required","target":{"kind":"obj","id":"$plugin:ward-rows"},"args":[{"kind":"lit","value":[{"id":"residents-0-0","row":{"name":"Ada"}},{"id":"residents-0-1","row":{"name":"Bea"}}]}]}},""" +
            Listen("overlap", 1) +
            """{"kind":"set","path":["textContent"],"members":"required","content":true,"target":{"kind":"obj","id":"status"},"value":{"kind":"as","expr":{"kind":"data","path":[{"name":"name","requires":[],"absent":{"kind":"start","value":null}}],"target":{"kind":"get","path":["row"],"members":"required","target":{"kind":"ref","name":"listenPayload1"}}}""" + ShownAsText + "}" +
            "]}]}]}"));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_second_action_on_the_same_rows_numbers_its_controls_from_its_own_place_in_a_handler_after_the_first()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        var rows = plan.Rows<ResidentRow>("residents");
        var discharge = rows.WardButton(HandlerPolicy.Overlap, (chosen, pipeline) => pipeline.Element("status").Text.Set(chosen.Row.Read(row => row.Name)));
        var transfer = rows.WardButton(HandlerPolicy.Overlap, (chosen, pipeline) => pipeline.Element("status").Text.Set(chosen.Row.Read(row => row.Wing)));
        var discharges = discharge.Render(Ada, "Discharge");
        var transfers = transfer.Render(Ada, "Transfer");
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(discharges, Is.EqualTo("""<button type="button" id="residents-0-0">Discharge</button>"""));
        Assert.That(transfers, Is.EqualTo("""<button type="button" id="residents-1-0">Transfer</button>"""));
        Assert.That(json, Is.EqualTo(Head + Native + DocumentAndRows + Status + """},"handlers":[""" + OnLoad +
            """{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["controls"],"members":"required","target":{"kind":"obj","id":"$plugin:ward-rows"},"args":[{"kind":"lit","value":[{"id":"residents-0-0","row":{"name":"Ada"}}]}]}},""" +
            Listen("overlap", 1) +
            """{"kind":"set","path":["textContent"],"members":"required","content":true,"target":{"kind":"obj","id":"status"},"value":{"kind":"as","expr":{"kind":"data","path":[{"name":"name","requires":[],"absent":{"kind":"start","value":null}}],"target":{"kind":"get","path":["row"],"members":"required","target":{"kind":"ref","name":"listenPayload1"}}}""" + ShownAsText + "}" +
            "]}]}," + OnLoad +
            """{"kind":"let","name":"result2","await":false,"binds":"line","expr":{"kind":"call","path":["controls"],"members":"required","target":{"kind":"obj","id":"$plugin:ward-rows"},"args":[{"kind":"lit","value":[{"id":"residents-1-0","row":{"wing":"North"}}]}]}},""" +
            Listen("overlap", 2) +
            """{"kind":"set","path":["textContent"],"members":"required","content":true,"target":{"kind":"obj","id":"status"},"value":{"kind":"as","expr":{"kind":"data","path":[{"name":"wing","requires":[],"absent":{"kind":"start","value":null}}],"target":{"kind":"get","path":["row"],"members":"required","target":{"kind":"ref","name":"listenPayload2"}}}""" + ShownAsText + "}" +
            "]}]}]}"));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void an_entry_holds_each_member_its_block_reads_in_a_branch_a_request_and_a_route_and_no_other()
    {
        var html = Views.Of(new Ward("North"), _ => { }, app => app.AddReactiveRequests(requests => requests.NoAntiforgery()));
        var plan = html.ReactivePlan<Ward>();
        var discharge = plan.Rows<ResidentRow>("residents").WardButton(HandlerPolicy.Overlap, (chosen, pipeline) =>
        {
            pipeline.When(chosen.Row.Read(row => row.Wing).Eq("North")).Then(pipeline => pipeline.Element("status").Text.Set("North wing"));
            pipeline.Get<ResidentId>("/Residents/{id}", request => request.Path(r => r.Id, chosen.Row.Read(row => row.Id))).NoDefaults()
                .OnSuccess(pipeline => pipeline.Element("room").Text.Set(chosen.Row.Read(row => row.Room.Number)));
        });
        discharge.Render(Ada, "Discharge");
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo(Head + Native + DocumentAndRows + ""","room":{"kind":"element","id":"room","vendor":"native"}""" + Status + """},"handlers":[""" + OnLoad +
            """{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["controls"],"members":"required","target":{"kind":"obj","id":"$plugin:ward-rows"},"args":[{"kind":"lit","value":[{"id":"residents-0-0","row":{"id":1,"wing":"North","room":{"number":"12"}}}]}]}},""" +
            Listen("overlap", 1) +
            """{"kind":"if","when":{"kind":"op","op":"eq","args":[{"kind":"data","path":[{"name":"wing","requires":[],"absent":{"kind":"start","value":null}}],"target":{"kind":"get","path":["row"],"members":"required","target":{"kind":"ref","name":"listenPayload1"}}},{"kind":"lit","value":"North"}]},"then":[{"kind":"set","path":["textContent"],"members":"required","content":true,"target":{"kind":"obj","id":"status"},"value":{"kind":"lit","value":"North wing"}}],"else":[]},""" +
            """{"kind":"let","name":"requestValue1","await":false,"binds":"line","expr":{"kind":"data","path":[{"name":"id","requires":[],"absent":{"kind":"start","value":0}}],"target":{"kind":"get","path":["row"],"members":"required","target":{"kind":"ref","name":"listenPayload1"}}}},""" +
            """{"kind":"request","argument":"argument1","reply":"reply1","send":{"kind":"record","fields":[{"name":"method","value":{"kind":"lit","value":"GET"}},""" +
            """{"name":"url","value":{"kind":"record","fields":[{"name":"parts","value":{"kind":"list","items":[{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"text"}},{"name":"text","value":{"kind":"lit","value":"/Residents/"}}]},""" +
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"member"}},{"name":"name","value":{"kind":"lit","value":"Id"}},""" +
            """{"name":"value","value":{"kind":"op","op":"cond","args":[{"kind":"op","op":"or","args":[{"kind":"op","op":"eq","args":[{"kind":"ref","name":"requestValue1"},{"kind":"lit","value":null}]},{"kind":"op","op":"neq","args":[{"kind":"ref","name":"requestValue1"},{"kind":"lit","value":null}]}]},""" +
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"ref","name":"requestValue1"}}]},{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"nothing"}}]}]}},""" +
            """{"name":"form","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"number"}}]}}]}]}},{"name":"query","value":{"kind":"lit","value":""}}]}},""" +
            """{"name":"query","value":{"kind":"record","fields":[{"name":"fields","value":{"kind":"list","items":[]}},{"name":"inputs","value":{"kind":"record","fields":[{"name":"container","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"none"}}]}},{"name":"individual","value":{"kind":"list","items":[]}}]}}]}},""" +
            """{"name":"body","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"none"}}]}},""" +
            """{"name":"headers","value":{"kind":"list","items":[{"kind":"record","fields":[{"name":"name","value":{"kind":"lit","value":"Accept"}},{"name":"value","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},{"name":"value","value":{"kind":"lit","value":"*/*"}}]}},{"name":"form","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"text"}}]}},{"name":"when","value":{"kind":"lit","value":true}}]}]}},""" +
            """{"name":"antiforgery","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"none"}}]}},{"name":"retry","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"none"}}]}},""" +
            """{"name":"busy","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"none"}}]}},{"name":"respondedJson","value":{"kind":"lit","value":false}},{"name":"contracts","value":{"kind":"list","items":[]}}]},"check":[],"layers":[{"whileLoading":[],"finally":[]}],"success":{"kind":"none","do":[""" +
            """{"kind":"set","path":["textContent"],"members":"required","content":true,"target":{"kind":"obj","id":"room"},"value":{"kind":"as","expr":{"kind":"data","path":[{"name":"room","requires":[],"absent":{"kind":"nothing"}},{"name":"number","requires":[],"absent":{"kind":"start","value":null}}],"target":{"kind":"get","path":["row"],"members":"required","target":{"kind":"ref","name":"listenPayload1"}}}""" + ShownAsText + "}" +
            """]},"statuses":[],"otherwise":{"kind":"none","do":[]},"chained":[]}""" +
            "]}]}]}"));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void the_control_a_rows_activation_holds_its_success_route_writes_holds_it_read_from_the_payload_when_the_request_starts()
    {
        var html = Views.Of(new Ward("North"), _ => { }, app => app.AddReactiveRequests(requests => requests.NoAntiforgery().ShowBusy()));
        var plan = html.ReactivePlan<Ward>();
        plan.Rows<ResidentRow>("residents").WardButton(HandlerPolicy.Overlap, (chosen, pipeline) => pipeline.Post("/residents/discharge")
            .OnSuccess(Content.None, (reply, pipeline) =>
            {
                chosen.Element.Text.Set("Discharged");
                pipeline.Element("status").Text.Set("Discharged");
            })).Render(new ResidentRow(1, "Ada", "North", new ResidentRoom("12", 1)), "Discharge");
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(Busy(json), Is.EqualTo(
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"targets"}},{"name":"values","value":{"kind":"list","items":[""" +
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"component"}},{"name":"value","value":{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"held"}},""" +
            """{"name":"value","value":{"kind":"get","path":["element"],"members":"required","target":{"kind":"ref","name":"listenPayload1"}}}]}}]},""" +
            """{"kind":"record","fields":[{"name":"kind","value":{"kind":"lit","value":"element"}},{"name":"id","value":{"kind":"lit","value":"status"}}]}]}}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_row_whose_type_requires_a_member_keeps_it_beside_the_member_read_and_a_row_requiring_none_keeps_the_read_alone()
    {
        var doses = Views.Of(new Ward("North"));
        var dosePlan = doses.ReactivePlan<Ward>();
        dosePlan.Rows<DoseRow>("doses").WardButton(HandlerPolicy.Overlap, (chosen, pipeline) => pipeline.Element("status").Text.Set(chosen.Row.Read(row => row.Medicine)))
            .Render(new DoseRow { Id = 7, Medicine = "Saline", Note = "After lunch" }, "Give");
        var notes = Views.Of(new Ward("North"));
        var notePlan = notes.ReactivePlan<Ward>();
        notePlan.Rows<NoteRow>("doses").WardButton(HandlerPolicy.Overlap, (chosen, pipeline) => pipeline.Element("status").Text.Set(chosen.Row.Read(row => row.Medicine)))
            .Render(new NoteRow { Id = 7, Medicine = "Saline", Note = "After lunch" }, "Give");
        var doseJson = BellPlans.JsonOf(doses.RenderPlan(dosePlan));
        var noteJson = BellPlans.JsonOf(notes.RenderPlan(notePlan));
        Assert.That(doseJson, Is.EqualTo(Head + Native + DocumentAndRows + Status + """},"handlers":[""" + OnLoad +
            """{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["controls"],"members":"required","target":{"kind":"obj","id":"$plugin:ward-rows"},"args":[{"kind":"lit","value":[{"id":"doses-0-0","row":{"id":7,"medicine":"Saline"}}]}]}},""" +
            Listen("overlap", 1) +
            """{"kind":"set","path":["textContent"],"members":"required","content":true,"target":{"kind":"obj","id":"status"},"value":{"kind":"as","expr":{"kind":"data","path":[{"name":"medicine","requires":["id"],"absent":{"kind":"start","value":""}}],"target":{"kind":"get","path":["row"],"members":"required","target":{"kind":"ref","name":"listenPayload1"}}}""" + ShownAsText + "}" +
            "]}]}]}"));
        Assert.That(noteJson, Is.EqualTo(Head + Native + DocumentAndRows + Status + """},"handlers":[""" + OnLoad +
            """{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["controls"],"members":"required","target":{"kind":"obj","id":"$plugin:ward-rows"},"args":[{"kind":"lit","value":[{"id":"doses-0-0","row":{"medicine":"Saline"}}]}]}},""" +
            Listen("overlap", 1) +
            """{"kind":"set","path":["textContent"],"members":"required","content":true,"target":{"kind":"obj","id":"status"},"value":{"kind":"as","expr":{"kind":"data","path":[{"name":"medicine","requires":[],"absent":{"kind":"start","value":""}}],"target":{"kind":"get","path":["row"],"members":"required","target":{"kind":"ref","name":"listenPayload1"}}}""" + ShownAsText + "}" +
            "]}]}]}"));
        Assert.That(PlanSchema.ProblemsOf(doseJson), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(noteJson), Is.Empty);
    }

    [Test]
    public void a_block_that_passes_the_row_whole_holds_it_whole_and_a_block_that_reads_nothing_of_it_holds_an_empty_row()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        var rows = plan.Rows<ResidentRow>("residents");
        var pin = rows.WardButton(HandlerPolicy.Overlap, (chosen, pipeline) => pipeline.Dispatch(WardEvents.Pinned, chosen.Row));
        var discharge = rows.WardButton(HandlerPolicy.Overlap, (chosen, pipeline) => chosen.Element.Text.Set("Discharged"));
        pin.Render(Ada, "Pin");
        discharge.Render(Ada, "Discharge");
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo(Head + NoVendor + DocumentAndRows + """},"handlers":[""" + OnLoad +
            """{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["controls"],"members":"required","target":{"kind":"obj","id":"$plugin:ward-rows"},"args":[{"kind":"lit","value":[{"id":"residents-0-0","row":{"id":1,"name":"Ada","wing":"North","room":{"number":"12","floor":1}}}]}]}},""" +
            Listen("overlap", 1) +
            """{"kind":"do","await":false,"expr":{"kind":"call","path":["dispatch"],"members":"required","target":{"kind":"obj","id":"$document"},"args":[{"kind":"lit","value":"pinned"},{"kind":"get","path":["row"],"members":"required","target":{"kind":"ref","name":"listenPayload1"}}]}}""" +
            "]}]}," + OnLoad +
            """{"kind":"let","name":"result2","await":false,"binds":"line","expr":{"kind":"call","path":["controls"],"members":"required","target":{"kind":"obj","id":"$plugin:ward-rows"},"args":[{"kind":"lit","value":[{"id":"residents-1-0","row":{}}]}]}},""" +
            Listen("overlap", 2) +
            """{"kind":"set","path":["textContent"],"members":"required","content":true,"target":{"kind":"get","path":["element"],"members":"required","target":{"kind":"ref","name":"listenPayload2"}},"value":{"kind":"lit","value":"Discharged"}}""" +
            "]}]}]}"));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void replace_and_skip_are_the_policy_of_the_actions_listen_while_its_handler_on_load_stays_overlap()
    {
        var html = Views.Of(new Ward("North"));
        var plan = html.ReactivePlan<Ward>();
        var rows = plan.Rows<ResidentRow>("residents");
        var open = rows.WardButton(HandlerPolicy.Replace, (chosen, pipeline) => chosen.Element.Text.Set("Opened"));
        var save = rows.WardButton(HandlerPolicy.Skip, (chosen, pipeline) => chosen.Element.Text.Set("Saved"));
        open.Render(Ada, "Open");
        save.Render(Ada, "Save");
        var json = BellPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo(Head + NoVendor + DocumentAndRows + """},"handlers":[""" + OnLoad +
            """{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["controls"],"members":"required","target":{"kind":"obj","id":"$plugin:ward-rows"},"args":[{"kind":"lit","value":[{"id":"residents-0-0","row":{}}]}]}},""" +
            Listen("replace", 1) +
            """{"kind":"set","path":["textContent"],"members":"required","content":true,"target":{"kind":"get","path":["element"],"members":"required","target":{"kind":"ref","name":"listenPayload1"}},"value":{"kind":"lit","value":"Opened"}}""" +
            "]}]}," + OnLoad +
            """{"kind":"let","name":"result2","await":false,"binds":"line","expr":{"kind":"call","path":["controls"],"members":"required","target":{"kind":"obj","id":"$plugin:ward-rows"},"args":[{"kind":"lit","value":[{"id":"residents-1-0","row":{}}]}]}},""" +
            Listen("skip", 2) +
            """{"kind":"set","path":["textContent"],"members":"required","content":true,"target":{"kind":"get","path":["element"],"members":"required","target":{"kind":"ref","name":"listenPayload2"}},"value":{"kind":"lit","value":"Saved"}}""" +
            "]}]}]}"));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_links_address_is_the_url_its_get_requests_for_the_row_each_path_value_one_encoded_segment_and_each_query_member_form_encoded_after_the_urls_own_query()
    {
        var html = Views.Of(new Ward("North"), _ => { }, app => app.AddReactiveRequests(requests => requests.NoAntiforgery()));
        var plan = html.ReactivePlan<Ward>();
        var rows = plan.Rows<ResidentRow>("residents");
        var open = rows.WardLink(HandlerPolicy.Replace, (chosen, pipeline) => pipeline.Get<ResidentKey>("/Residents/{name}", request => request.Path(r => r.Name, chosen.Row.Read(row => row.Name))));
        var wing = rows.WardLink(HandlerPolicy.Replace, (chosen, pipeline) => pipeline.Get<WingQuery>("/Residents?tab=care", request => request.Query(r => r.Wing, chosen.Row.Read(row => row.Wing))));
        var numbered = rows.WardLink(HandlerPolicy.Replace, (chosen, pipeline) => pipeline.Get<ResidentId>("/Residents/{id}", request => request.Path(r => r.Id, chosen.Row.Read(row => row.Id))));
        var leveled = plan.Rows<LeveledRow>("levels").WardLink(HandlerPolicy.Replace, (chosen, pipeline) => pipeline.Get<CareQuery>("/Residents", request => request.Query(r => r.Level, chosen.Row.Read(row => row.Level))));
        var room = new ResidentRoom("12", 1);
        var spaced = open.Render(new ResidentRow(1, "Ada Lovelace", "North", room), "Ada Lovelace");
        var plus = open.Render(new ResidentRow(2, "a+b", "North", room), "a+b");
        var apostrophe = open.Render(new ResidentRow(3, "O'Neil", "North", room), "O'Neil");
        var parentheses = open.Render(new ResidentRow(4, "Unit (2)", "North", room), "Unit (2)");
        var tilde = open.Render(new ResidentRow(5, "~north", "North", room), "~north");
        var accented = open.Render(new ResidentRow(6, "Zoé", "North", room), "Zoé");
        var queried = wing.Render(new ResidentRow(7, "Ada", "a b+c", room), "Ada");
        var byId = numbered.Render(new ResidentRow(42, "Ada", "North", room), "Ada");
        var nursing = leveled.Render(new LeveledRow("Ada", CareLevel.Nursing), "Ada");
        html.RenderPlan(plan);
        Assert.That(spaced, Is.EqualTo("""<a id="residents-0-0" href="/Residents/Ada%20Lovelace">Ada Lovelace</a>"""));
        Assert.That(plus, Is.EqualTo("""<a id="residents-0-1" href="/Residents/a%2Bb">a+b</a>"""));
        Assert.That(apostrophe, Is.EqualTo("""<a id="residents-0-2" href="/Residents/O%27Neil">O'Neil</a>"""));
        Assert.That(parentheses, Is.EqualTo("""<a id="residents-0-3" href="/Residents/Unit%20%282%29">Unit (2)</a>"""));
        Assert.That(tilde, Is.EqualTo("""<a id="residents-0-4" href="/Residents/~north">~north</a>"""));
        Assert.That(accented, Is.EqualTo("""<a id="residents-0-5" href="/Residents/Zo%C3%A9">Zoé</a>"""));
        Assert.That(queried, Is.EqualTo("""<a id="residents-1-0" href="/Residents?tab=care&Wing=a+b%2Bc">Ada</a>"""));
        Assert.That(byId, Is.EqualTo("""<a id="residents-2-0" href="/Residents/42">Ada</a>"""));
        Assert.That(nursing, Is.EqualTo("""<a id="levels-0-0" href="/Residents?Level=Nursing">Ada</a>"""));
    }

    [Test]
    public void a_link_whose_get_sends_a_member_under_a_condition_is_refused_and_the_same_member_with_no_condition_is_in_its_address()
    {
        var html = Views.Of(new Ward("North"), _ => { }, app => app.AddReactiveRequests(requests => requests.NoAntiforgery()));
        var plan = html.ReactivePlan<Ward>();
        var wing = plan.Rows<ResidentRow>("residents").WardLink(HandlerPolicy.Overlap, (chosen, pipeline) => pipeline.Get<WingQuery>("/Residents", request => request.Query(r => r.Wing, chosen.Row.Read(row => row.Wing))));
        var queried = wing.Render(new ResidentRow(1, "Ada", "North", new ResidentRoom("12", 1)), "Ada");
        html.RenderPlan(plan);
        var guarded = Views.Of(new Ward("North"), _ => { }, app => app.AddReactiveRequests(requests => requests.NoAntiforgery())).ReactivePlan<Ward>().Rows<ResidentRow>("residents");

        Assert.That(queried, Is.EqualTo("""<a id="residents-0-0" href="/Residents?Wing=North">Ada</a>"""));
        Assert.That(() => guarded.WardLink(HandlerPolicy.Overlap, (chosen, pipeline) => pipeline.Get<WingQuery>("/Residents", request => request
                .When(pipeline.Element("filter").Hidden).Then(filtered => filtered.Query(r => r.Wing, chosen.Row.Read(row => row.Wing))))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "The row link on 'residents' sends 'GET /Residents' with 'r => r.Wing' under a condition, and a link's address is written for each row before any condition is read: send the member with no condition, or render a button for the action, whose block reads the condition."));
    }

    [Test]
    public void each_row_action_the_view_cannot_render_is_refused_with_what_is_wrong_and_its_cure()
    {
        var html = Views.Of(new Ward("North"), _ => { }, app => app.AddReactiveRequests(requests => requests.NoAntiforgery()).AddReactiveFluentValidation(validation => validation.Flags<CareNeeds>()));
        var plan = html.ReactivePlan<Ward>();
        var rows = plan.Rows<ResidentRow>("residents");
        var again = plan.Rows<ResidentRow>("residents").WardButton(HandlerPolicy.Overlap, (chosen, pipeline) => chosen.Element.Text.Set("Discharged"));
        rows.WardButton(HandlerPolicy.Overlap, (chosen, pipeline) => chosen.Element.Text.Set("Discharged")).Render(Ada, "Discharge");
        var open = rows.WardLink(HandlerPolicy.Overlap, (chosen, pipeline) => pipeline.Get<ResidentKey>("/Residents/{name}", request => request.Path(r => r.Name, chosen.Row.Read(row => row.Name))));
        var counts = plan.Rows<CountRow>("counts");
        var countShown = counts.WardButton(HandlerPolicy.Overlap, (chosen, pipeline) => pipeline.Element("status").Text.Set(chosen.Row.Read(row => row.Count)));
        var nameShown = counts.WardButton(HandlerPolicy.Overlap, (chosen, pipeline) => pipeline.Element("status").Text.Set(chosen.Row.Read(row => row.Name)));
        var tallies = plan.Rows<CountRow>("tallies").WardLink(HandlerPolicy.Overlap, (chosen, pipeline) => pipeline.Get<CountKey>("/Counts/{count}", request => request.Path(r => r.Count, chosen.Row.Read(row => row.Count))));
        var levels = plan.Rows<LeveledRow>("levels").WardLink(HandlerPolicy.Overlap, (chosen, pipeline) => pipeline.Get<CareQuery>("/Residents", request => request.Query(r => r.Level, chosen.Row.Read(row => row.Level))));
        var late = rows.WardButton(HandlerPolicy.Overlap, (chosen, pipeline) => chosen.Element.Text.Set("Late"));
        Func<string> aSectionsRow = () => late.Render(Bea, "Late");
        var room = new ResidentRoom("12", 1);

        Assert.That(() => again.Render(Ada, "Discharge"), Throws.InvalidOperationException.With.Message.EqualTo(
            "Element 'residents-0-0' renders a second time in this plan: render each component once."));
        Assert.That(() => rows.Declare<WardRowsPlugin, WardRowControls<ResidentRow>, WardRowChosen<ResidentRow>>((plugin, entries) => plugin.Controls(entries), controls => controls.Chosen,
                chosen => chosen.Detail.Row, HandlerPolicy.Overlap, (chosen, pipeline) => chosen.Element.Text.Set("Discharged")),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A row action on 'residents' reads its row from a member that is not WardRowChosen<ResidentRow>'s own: name the member of WardRowChosen<ResidentRow> that holds the row, as a ReadOnly<ResidentRow>."));
        Assert.That(() => rows.Declare<WardRowsPlugin, WardRowControls<ResidentRow>, WardRowChosen<ResidentRow>>((plugin, entries) => plugin.Current<ResidentRow>(), controls => controls.Chosen,
                chosen => chosen.Row, HandlerPolicy.Overlap, (chosen, pipeline) => chosen.Element.Text.Set("Discharged")),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A row action on 'residents' listens to an object its plugin did not return for the action's controls: pass the controls its source receives, once, to the method of WardRowsPlugin that returns the object raising the action."));
        Assert.That(() => rows.Declare<WardRowsPlugin, WardRowControls<ResidentRow>, WardRowChosen<ResidentRow>>((plugin, entries) =>
                {
                    plugin.Forget(entries);
                    return plugin.Current<ResidentRow>();
                }, controls => controls.Chosen, chosen => chosen.Row, HandlerPolicy.Overlap, (chosen, pipeline) => chosen.Element.Text.Set("Discharged")),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A row action on 'residents' listens to an object its plugin did not return for the action's controls: pass the controls its source receives, once, to the method of WardRowsPlugin that returns the object raising the action."));
        Assert.That(() => rows.WardLink(HandlerPolicy.Overlap, (chosen, pipeline) => chosen.Element.Text.Set("Opened")), Throws.InvalidOperationException.With.Message.EqualTo(
            "The row link on 'residents' sends no request at its steps' own level, so its links have no address: send one GET there, or render a button for the action."));
        Assert.That(() => rows.WardLink(HandlerPolicy.Overlap, (chosen, pipeline) =>
            {
                pipeline.Get("/Residents");
                pipeline.Get("/Wings");
            }), Throws.InvalidOperationException.With.Message.EqualTo(
            "The row link on 'residents' sends 2 requests at its steps' own level, and a link opens one address: send one GET there, or render a button for the action."));
        Assert.That(() => rows.WardLink(HandlerPolicy.Overlap, (chosen, pipeline) => pipeline.Post<ResidentId>("/Residents/{id}", request => request.Path(r => r.Id, chosen.Row.Read(row => row.Id)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "The row link on 'residents' sends 'POST /Residents/{id}', and a link opens its address by GET alone: send a GET, or render a button for the action."));
        Assert.That(() => rows.WardLink(HandlerPolicy.Overlap, (chosen, pipeline) => pipeline.Get<ResidentKey>("/Residents/{name}", request => request.Path(r => r.Name, chosen.Row.Read(row => row.Name)).Header("X-Ward", "North"))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "The row link on 'residents' sends 'GET /Residents/{name}' with a header, which a link's address cannot carry: send the URL's members alone, or render a button for the action."));
        Assert.That(() => rows.WardLink(HandlerPolicy.Overlap, (chosen, pipeline) => pipeline.Get<WingQuery>("/Residents", request => request.Input(r => r.Wing, pipeline.Input(m => m.Name)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "The row link on 'residents' sends 'GET /Residents' with an input's value, which a link's address cannot carry: send the URL's members alone, or render a button for the action."));
        Assert.That(() => rows.WardLink(HandlerPolicy.Overlap, (chosen, pipeline) => pipeline.Get<WingQuery>("/Residents", request => request.Inputs(pipeline.Container<WingQuery>("filters")))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "The row link on 'residents' sends 'GET /Residents' with the inputs of 'filters', which a link's address cannot carry: send the URL's members alone, or render a button for the action."));
        Assert.That(() => rows.WardLink(HandlerPolicy.Overlap, (chosen, pipeline) => pipeline.Get<ShownQuery>("/Residents", request => request.Query(r => r.Hidden, pipeline.Element("filter").Hidden))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'r => r.Hidden' of the row link on 'residents' reads a value neither the view nor the row holds, which a link's address cannot carry: fill it with a value or a read of the row, or render a button for the action."));
        Assert.That(() => rows.WardLink(HandlerPolicy.Overlap, (chosen, pipeline) => pipeline.Get<AdmittedQuery>("/Residents", request => request.Query(r => r.Admitted, new DateTime(2026, 11, 16)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'r => r.Admitted' of the row link on 'residents' holds a date, which a link's address does not write: render a button for the action, or send the value as text the view formats."));
        Assert.That(() => rows.WardLink(HandlerPolicy.Overlap, (chosen, pipeline) => pipeline.Get<SeenQuery>("/Residents", request => request.Query(r => r.Seen, new DateTimeOffset(2026, 11, 16, 9, 30, 0, TimeSpan.Zero)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'r => r.Seen' of the row link on 'residents' holds a date with its offset, which a link's address does not write: render a button for the action, or send the value as text the view formats."));
        Assert.That(() => plan.Rows<NeedsRow>("needs").WardLink(HandlerPolicy.Overlap, (chosen, pipeline) => pipeline.Get<NeedsQuery>("/Residents", request => request.Query(r => r.Needs, chosen.Row.Read(row => row.Needs)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'r => r.Needs' of the row link on 'needs' holds flags, which a link's address does not write: render a button for the action, or send the value as text the view formats."));
        Assert.That(() => rows.WardLink(HandlerPolicy.Overlap, (chosen, pipeline) => pipeline.Get<WingsQuery>("/Residents", request => request.Query(r => r.Wings, new[] { "North", "South" }))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'r => r.Wings' of the row link on 'residents' holds a list, which a link's address does not write: render a button for the action, or send the value as text the view formats."));
        Assert.That(() => rows.WardLink(HandlerPolicy.Overlap, (chosen, pipeline) => pipeline.Get<WeightQuery>("/Residents", request => request.Query(r => r.Weight, 61.5))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'r => r.Weight' of the row link on 'residents' holds a fraction, which a link's address does not write: render a button for the action, or send the value as text the view formats."));
        Assert.That(() => open.Render(new ResidentRow(8, null!, "North", room), "Nobody"), Throws.InvalidOperationException.With.Message.EqualTo(
            "Row link 'residents-1-0' fills 'r => r.Name' of 'GET /Residents/{name}' with nothing, and a path segment holds nonempty text with no '/': give the row a value there, or send the member in the query."));
        Assert.That(() => open.Render(new ResidentRow(9, "", "North", room), "Empty"), Throws.InvalidOperationException.With.Message.EqualTo(
            "Row link 'residents-1-1' fills 'r => r.Name' of 'GET /Residents/{name}' with empty text, and a path segment holds nonempty text with no '/': give the row a value there, or send the member in the query."));
        Assert.That(() => open.Render(new ResidentRow(10, "a/b", "North", room), "a/b"), Throws.InvalidOperationException.With.Message.EqualTo(
            "Row link 'residents-1-2' fills 'r => r.Name' of 'GET /Residents/{name}' with 'a/b', which holds '/', and a path segment holds nonempty text with no '/': give the row a value there, or send the member in the query."));
        Assert.That(() => levels.Render(new LeveledRow("Ada", (CareLevel)7), "Ada"), Throws.InvalidOperationException.With.Message.EqualTo(
            "Row link 'levels-0-0' fills 'r => r.Level' with 7, which its form 'enum' writes no text for: give the row a value of that form, or render a button for the action."));
        Assert.That(() => tallies.Render(new CountRow(9007199254740993, "Ada"), "Count"), Throws.InvalidOperationException.With.Message.EqualTo(
            "'CountRow.count' holds a JavaScript number, and 9007199254740993 is a whole number past 9007199254740991, which JavaScript reads as another value; write a finite number of at most 15 significant digits."));
        Assert.That(() => countShown.Render(new CountRow(9007199254740993, "Ada"), "Count"), Throws.InvalidOperationException.With.Message.EqualTo(
            "'CountRow.count' holds a JavaScript number, and 9007199254740993 is a whole number past 9007199254740991, which JavaScript reads as another value; write a finite number of at most 15 significant digits."));
        Assert.That(() => nameShown.Render(new CountRow(9007199254740993, "Ada"), "Name"), Throws.Nothing);
        html.RenderPlan(plan);
        Assert.That(() => aSectionsRow(), Throws.InvalidOperationException.With.Message.EqualTo(
            "A door takes the plan of 'Alis.Reactive.ModuleTests.Vendors.Ward' after it rendered, so the page never runs what the door writes; " +
            "write every door above Html.RenderPlan, and where a @section holds one, render the plan in a section the layout renders after it."));
    }

    private static string Busy(string plan)
        => RequestIn(JsonDocument.Parse(plan).RootElement)!.Value.GetProperty("send").GetProperty("fields").EnumerateArray()
            .Single(field => field.GetProperty("name").GetString() == "busy").GetProperty("value").GetRawText();

    private static JsonElement? RequestIn(JsonElement node)
    {
        if (node.ValueKind == JsonValueKind.Array) return node.EnumerateArray().Select(RequestIn).FirstOrDefault(found => found is not null);
        if (node.ValueKind != JsonValueKind.Object) return null;
        if (node.TryGetProperty("kind", out var kind) && kind.ValueKind == JsonValueKind.String && kind.GetString() == "request") return node;
        return node.EnumerateObject().Select(member => RequestIn(member.Value)).FirstOrDefault(found => found is not null);
    }
}
