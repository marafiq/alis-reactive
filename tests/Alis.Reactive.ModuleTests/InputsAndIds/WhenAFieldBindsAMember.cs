using System.Text.Json.Serialization;
using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Html;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.InputsAndIds;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAFieldBindsAMember
{
    private const string RoomId = "Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room";

    [Test]
    public void the_plan_holds_the_input_by_its_id_with_its_name_path_value_commit_class_and_message()
    {
        var html = Views.Of(new MoveIn { Room = "B-12" });
        var plan = html.ReactivePlan<MoveIn>();
        MoveInPlans.Box(html, plan, m => m.Room);
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo(MoveInPlans.Head + "\"" + RoomId + "\":" + """{"name":"Room","path":["room"],"value":{"kind":"op","op":"coalesce","args":[{"kind":"as","expr":{"kind":"get","path":["value"],"members":"required","target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room"}},"shape":{"kind":"nullable","inner":{"kind":"string","date":"none","negativeZero":"0"}}},{"kind":"lit","value":""}]},"committed":"change","invalidClass":"alis-has-error","message":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room_message"}},"containers":{}}},"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room":{"kind":"element","id":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room","vendor":"native"}""" + MoveInPlans.Tail));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_blank_number_reads_the_start_a_new_model_gives_its_member()
    {
        var html = Views.Of(new MoveIn { Nights = 9, ExtraNights = 2 });
        var plan = html.ReactivePlan<MoveIn>();
        MoveInPlans.Box(html, plan, m => m.Nights);
        MoveInPlans.Box(html, plan, m => m.ExtraNights);
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo(MoveInPlans.Head + "\"" + """Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__ExtraNights":{"name":"ExtraNights","path":["extraNights"],"value":{"kind":"op","op":"coalesce","args":[{"kind":"as","expr":{"kind":"get","path":["value"],"members":"required","target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__ExtraNights"}},"shape":{"kind":"nullable","inner":{"kind":"number","of":"integer","text":"view","overflow":"nothing"}}},{"kind":"lit","value":0}]},"committed":"change","invalidClass":"alis-has-error","message":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__ExtraNights_message"},"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Nights":{"name":"Nights","path":["nights"],"value":{"kind":"op","op":"coalesce","args":[{"kind":"as","expr":{"kind":"get","path":["value"],"members":"required","target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Nights"}},"shape":{"kind":"nullable","inner":{"kind":"number","of":"integer","text":"view","overflow":"nothing"}}},{"kind":"lit","value":5}]},"committed":"change","invalidClass":"alis-has-error","message":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Nights_message"}},"containers":{}}},"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__ExtraNights":{"kind":"element","id":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__ExtraNights","vendor":"native"},"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Nights":{"kind":"element","id":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Nights","vendor":"native"}""" + MoveInPlans.Tail));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_list_position_and_a_nullable_fraction_take_their_body_path_and_shape()
    {
        var html = Views.Of(new MoveIn { Doses = [new Dose(), new Dose { Amount = 0.125m }] });
        var plan = html.ReactivePlan<MoveIn>();
        MoveInPlans.Box(html, plan, m => m.Doses[1].Amount);
        MoveInPlans.Box(html, plan, m => m.Strength);
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo(MoveInPlans.Head + "\"" + """Alis_Reactive_ModuleTests_InputsAndIds_Dose__Doses_1__Amount":{"name":"Doses[1].Amount","path":["doses",1,"amount"],"value":{"kind":"op","op":"coalesce","args":[{"kind":"as","expr":{"kind":"get","path":["value"],"members":"required","target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_Dose__Doses_1__Amount"}},"shape":{"kind":"nullable","inner":{"kind":"number","of":"fraction","text":"view","overflow":"nothing"}}},{"kind":"lit","value":0}]},"committed":"change","invalidClass":"alis-has-error","message":"Alis_Reactive_ModuleTests_InputsAndIds_Dose__Doses_1__Amount_message"},"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Strength":{"name":"Strength","path":["strength"],"value":{"kind":"op","op":"coalesce","args":[{"kind":"as","expr":{"kind":"get","path":["value"],"members":"required","target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Strength"}},"shape":{"kind":"nullable","inner":{"kind":"number","of":"fraction","text":"view","overflow":"nothing"}}},{"kind":"lit","value":null}]},"committed":"change","invalidClass":"alis-has-error","message":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Strength_message"}},"containers":{}}},"Alis_Reactive_ModuleTests_InputsAndIds_Dose__Doses_1__Amount":{"kind":"element","id":"Alis_Reactive_ModuleTests_InputsAndIds_Dose__Doses_1__Amount","vendor":"native"},"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Strength":{"kind":"element","id":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Strength","vendor":"native"}""" + MoveInPlans.Tail));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void the_field_starts_its_component_at_the_members_value_as_the_plan_writes_it()
    {
        var html = Views.Of(new MoveIn { Room = "B-12", Nights = 9 });
        var plan = html.ReactivePlan<MoveIn>();
        Assert.That(html.InputField(plan, m => m.Room).ValueJson, Is.EqualTo("\"B-12\""));
        Assert.That(html.InputField(plan, m => m.Nights).ValueJson, Is.EqualTo("9"));
        Assert.That(html.InputField(plan, m => m.Strength).ValueJson, Is.EqualTo("null"));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_field_with_a_label_renders_its_wrapper_label_marker_and_message_around_its_component()
    {
        var html = Views.Of(new MoveIn());
        var plan = html.ReactivePlan<MoveIn>();
        var rendered = html.InputField(plan, m => m.Room, options => options.Label("Room").Required()).Render<IntakeBox>(new HtmlString("<input>"));
        Assert.That(Views.TextOf(rendered), Is.EqualTo("""<div class="alis-field"><label class="alis-field__label" for="Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room">Room <span class="alis-field__required">*</span></label><input><span id="Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room_message" class="alis-field__message" hidden></span></div>"""));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_field_with_no_options_renders_no_label_and_no_marker()
    {
        var html = Views.Of(new MoveIn());
        var plan = html.ReactivePlan<MoveIn>();
        var rendered = MoveInPlans.Box(html, plan, m => m.Room);
        Assert.That(Views.TextOf(rendered), Is.EqualTo("""<div class="alis-field"><input><span id="Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room_message" class="alis-field__message" hidden></span></div>"""));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_hidden_field_renders_its_component_alone_and_binds_no_message_element()
    {
        var html = Views.Of(new MoveIn());
        var plan = html.ReactivePlan<MoveIn>();
        var rendered = html.InputField(plan, m => m.Room, options => options.Label("Room").Required().Hidden()).Render<IntakeBox>(new HtmlString("<input>"));
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(Views.TextOf(rendered), Is.EqualTo("<input>"));
        Assert.That(json, Is.EqualTo(MoveInPlans.Head + "\"" + RoomId + "\":" + """{"name":"Room","path":["room"],"value":{"kind":"op","op":"coalesce","args":[{"kind":"as","expr":{"kind":"get","path":["value"],"members":"required","target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room"}},"shape":{"kind":"nullable","inner":{"kind":"string","date":"none","negativeZero":"0"}}},{"kind":"lit","value":""}]},"committed":"change","invalidClass":"alis-has-error","message":""}},"containers":{}}},"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room":{"kind":"element","id":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room","vendor":"native"}""" + MoveInPlans.Tail));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_dictionary_key_is_a_member_of_the_body_path()
    {
        var html = Views.Of(new MoveIn { Rounds = new() { ["am"] = new Dose { Amount = 0.25m } } });
        var plan = html.ReactivePlan<MoveIn>();
        MoveInPlans.Box(html, plan, m => m.Rounds["am"].Amount);
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo(MoveInPlans.Head + "\"" + """Alis_Reactive_ModuleTests_InputsAndIds_Dose__Rounds_am__Amount":{"name":"Rounds[am].Amount","path":["rounds","am","amount"],"value":{"kind":"op","op":"coalesce","args":[{"kind":"as","expr":{"kind":"get","path":["value"],"members":"required","target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_Dose__Rounds_am__Amount"}},"shape":{"kind":"nullable","inner":{"kind":"number","of":"fraction","text":"view","overflow":"nothing"}}},{"kind":"lit","value":0}]},"committed":"change","invalidClass":"alis-has-error","message":"Alis_Reactive_ModuleTests_InputsAndIds_Dose__Rounds_am__Amount_message"}},"containers":{}}},"Alis_Reactive_ModuleTests_InputsAndIds_Dose__Rounds_am__Amount":{"kind":"element","id":"Alis_Reactive_ModuleTests_InputsAndIds_Dose__Rounds_am__Amount","vendor":"native"}""" + MoveInPlans.Tail));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_blank_member_reads_the_start_the_endpoint_binds_whether_it_fills_its_holder_in_place_or_makes_a_new_one()
    {
        var html = Views.Of(new Arrival());
        var plan = html.ReactivePlan<Arrival>();
        html.InputField(plan, m => m.Kin.Name).Render<IntakeBox>(new HtmlString("<input>"));
        html.InputField(plan, m => m.Guardian.Name).Render<IntakeBox>(new HtmlString("<input>"));
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.InputsAndIds.Arrival","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"$model":{"kind":"plugin","name":"alis.model","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"model","inputs":{"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Guardian_Name":{"name":"Guardian.Name","path":["guardian","name"],"value":{"kind":"op","op":"coalesce","args":[{"kind":"as","expr":{"kind":"get","path":["value"],"members":"required","target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Guardian_Name"}},"shape":{"kind":"nullable","inner":{"kind":"string","date":"none","negativeZero":"0"}}},{"kind":"lit","value":""}]},"committed":"change","invalidClass":"alis-has-error","message":"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Guardian_Name_message"},"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Kin_Name":{"name":"Kin.Name","path":["kin","name"],"value":{"kind":"op","op":"coalesce","args":[{"kind":"as","expr":{"kind":"get","path":["value"],"members":"required","target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Kin_Name"}},"shape":{"kind":"nullable","inner":{"kind":"string","date":"none","negativeZero":"0"}}},{"kind":"lit","value":"Unknown"}]},"committed":"change","invalidClass":"alis-has-error","message":"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Kin_Name_message"}},"containers":{}}},"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Guardian_Name":{"kind":"element","id":"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Guardian_Name","vendor":"native"},"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Kin_Name":{"kind":"element","id":"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Kin_Name","vendor":"native"}},"handlers":[]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void an_enum_blank_uses_the_live_form_even_when_the_app_writes_enums_as_text()
    {
        var html = Views.Of(new MoveIn());
        var plan = html.ReactivePlan<MoveIn>();
        MoveInPlans.Box(html, plan, m => m.Shift);
        MoveInPlans.Box(html, plan, m => m.Stay);
        var asNumber = MoveInPlans.JsonOf(html.RenderPlan(plan));
        var named = Views.Of(new MoveIn(), options => options.Converters.Add(new JsonStringEnumConverter()));
        var namedPlan = named.ReactivePlan<MoveIn>();
        MoveInPlans.Box(named, namedPlan, m => m.Shift);
        var asText = MoveInPlans.JsonOf(named.RenderPlan(namedPlan));
        Assert.That(asNumber, Is.EqualTo(MoveInPlans.Head + "\"" + """Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Shift":{"name":"Shift","path":["shift"],"value":{"kind":"op","op":"coalesce","args":[{"kind":"as","expr":{"kind":"get","path":["value"],"members":"required","target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Shift"}},"shape":{"kind":"nullable","inner":{"kind":"number","of":"integer","text":"view","overflow":"nothing"}}},{"kind":"lit","value":1}]},"committed":"change","invalidClass":"alis-has-error","message":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Shift_message"},"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Stay":{"name":"Stay","path":["stay"],"value":{"kind":"op","op":"coalesce","args":[{"kind":"as","expr":{"kind":"get","path":["value"],"members":"required","target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Stay"}},"shape":{"kind":"nullable","inner":{"kind":"string","date":"none","negativeZero":"0"}}},{"kind":"lit","value":null}]},"committed":"change","invalidClass":"alis-has-error","message":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Stay_message"}},"containers":{}}},"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Shift":{"kind":"element","id":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Shift","vendor":"native"},"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Stay":{"kind":"element","id":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Stay","vendor":"native"}""" + MoveInPlans.Tail));
        Assert.That(asText, Is.EqualTo(MoveInPlans.Head + "\"" + """Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Shift":{"name":"Shift","path":["shift"],"value":{"kind":"op","op":"coalesce","args":[{"kind":"as","expr":{"kind":"get","path":["value"],"members":"required","target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Shift"}},"shape":{"kind":"nullable","inner":{"kind":"number","of":"integer","text":"view","overflow":"nothing"}}},{"kind":"lit","value":1}]},"committed":"change","invalidClass":"alis-has-error","message":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Shift_message"}},"containers":{}}},"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Shift":{"kind":"element","id":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Shift","vendor":"native"}""" + MoveInPlans.Tail));
        Assert.That(PlanSchema.ProblemsOf(asNumber), Is.Empty);
    }
}
