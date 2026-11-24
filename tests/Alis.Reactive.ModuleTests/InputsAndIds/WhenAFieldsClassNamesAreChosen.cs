using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Html;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.InputsAndIds;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAFieldsClassNamesAreChosen
{
    [Test]
    public void a_field_renders_the_names_the_app_chose_and_the_default_of_each_it_left_unset()
    {
        var html = Views.Of(new MoveIn(), _ => { }, services => services.AddReactiveClassNames(names =>
        {
            names.Field = "mb-3";
            names.FieldMessage = "invalid-feedback";
            names.InvalidInput = "is-invalid";
        }));
        var plan = html.ReactivePlan<MoveIn>();
        var rendered = html.InputField(plan, m => m.Room, options => options.Label("Room").Required()).Render<IntakeBox>(new HtmlString("<input>"));
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(Views.TextOf(rendered), Is.EqualTo("""<div class="mb-3"><label class="alis-field__label" for="Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room">Room <span class="alis-field__required">*</span></label><input><span id="Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room_message" class="invalid-feedback" hidden></span></div>"""));
        Assert.That(json, Is.EqualTo(MoveInPlans.Head + "\"" + """Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room":{"name":"Room","path":["room"],"converted":{"kind":"as","expr":{"kind":"get","path":["value"],"members":"required","target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room"}},"shape":{"kind":"nullable","inner":{"kind":"string","date":"none","negativeZero":"0"}}},"blank":{"kind":"lit","value":""},"committed":"change","invalidClass":"is-invalid","message":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room_message"}},"containers":{}}},"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room":{"kind":"element","id":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room","vendor":"native"}""" + MoveInPlans.Tail));
    }

    [Test]
    public void an_app_that_never_chose_renders_every_default()
    {
        var html = Views.Of(new MoveIn());
        var plan = html.ReactivePlan<MoveIn>();
        var rendered = html.InputField(plan, m => m.Room, options => options.Label("Room").Required()).Render<IntakeBox>(new HtmlString("<input>"));
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(Views.TextOf(rendered), Is.EqualTo("""<div class="alis-field"><label class="alis-field__label" for="Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room">Room <span class="alis-field__required">*</span></label><input><span id="Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room_message" class="alis-field__message" hidden></span></div>"""));
        Assert.That(json, Is.EqualTo(MoveInPlans.Head + "\"" + """Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room":{"name":"Room","path":["room"],"converted":{"kind":"as","expr":{"kind":"get","path":["value"],"members":"required","target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room"}},"shape":{"kind":"nullable","inner":{"kind":"string","date":"none","negativeZero":"0"}}},"blank":{"kind":"lit","value":""},"committed":"change","invalidClass":"alis-has-error","message":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room_message"}},"containers":{}}},"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room":{"kind":"element","id":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room","vendor":"native"}""" + MoveInPlans.Tail));
    }

    [Test]
    public void an_invalid_inputs_class_is_one_class_name()
    {
        Assert.That(() => Views.Of(new MoveIn(), _ => { }, services => services.AddReactiveClassNames(names => names.InvalidInput = "is invalid")), Throws.ArgumentException.With.Message.EqualTo("InvalidInput is one class name, which an invalid input takes and loses on its own; 'is invalid' is not one."));
        Assert.That(() => Views.Of(new MoveIn(), _ => { }, services => services.AddReactiveClassNames(names => names.InvalidInput = "is-invalid")), Throws.Nothing);
    }

    [Test]
    public void a_chosen_name_holds_a_class()
    {
        Assert.That(() => Views.Of(new MoveIn(), _ => { }, services => services.AddReactiveClassNames(names => names.FieldLabel = " ")), Throws.ArgumentException.With.Message.EqualTo("FieldLabel needs a class name; leave it unset to keep its default."));
        Assert.That(() => Views.Of(new MoveIn(), _ => { }, services => services.AddReactiveClassNames(names => names.FieldLabel = "form-label fw-bold")), Throws.Nothing);
    }
}
