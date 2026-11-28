using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.RequestsAndResponses;
using Microsoft.AspNetCore.Html;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.InputsAndIds;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAViewRendersARequestContainer
{
    private const string Head = """{"version":7,"planId":"Alis.Reactive.ModuleTests.InputsAndIds.Family","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{},"objects":{"$model":{"kind":"plugin","name":"alis.model","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"model","inputs":{},"containers":{""";
    private const string Tail = """}}}},"handlers":[]}""";

    [Test]
    public void an_element_container_renders_a_fieldset_and_its_own_summary()
    {
        var html = Views.Of(new Family());
        var plan = html.ReactivePlan<Family>();
        var block = html.BeginRequestContainer(plan, "kin");
        Assert.That(html.ViewContext.Writer.ToString(), Is.EqualTo("""<fieldset id="kin"><div id="kin_summary" class="validation-summary-valid" hidden><ul></ul></div>"""));
        Assert.That(block.Container, Is.Not.Null);
        block.Dispose();
        block.Dispose();
        Assert.That(html.ViewContext.Writer.ToString(), Is.EqualTo("""<fieldset id="kin"><div id="kin_summary" class="validation-summary-valid" hidden><ul></ul></div></fieldset>"""));
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo(Head + "\"" + """kin":{"model":"Alis.Reactive.ModuleTests.InputsAndIds.Family","prefix":{"name":"","path":[]},"summary":"kin_summary","live":[]}""" + Tail));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_view_reaches_a_container_another_view_renders_without_registering_it_again()
    {
        var owner = Views.Of(new Family());
        var ownerPlan = owner.ReactivePlan<Family>();
        using (owner.BeginRequestContainer(ownerPlan, "kin")) { }
        var ownerJson = MoveInPlans.JsonOf(owner.RenderPlan(ownerPlan));
        Assert.That(ownerJson, Is.EqualTo(Head + "\"" + """kin":{"model":"Alis.Reactive.ModuleTests.InputsAndIds.Family","prefix":{"name":"","path":[]},"summary":"kin_summary","live":[]}""" + Tail));
        Assert.That(PlanSchema.ProblemsOf(ownerJson), Is.Empty);
        Assert.That(owner.ViewContext.Writer.ToString(), Is.EqualTo("""<fieldset id="kin"><div id="kin_summary" class="validation-summary-valid" hidden><ul></ul></div></fieldset>"""));

        var html = Views.Of(new MoveIn());
        var plan = html.ReactivePlan<MoveIn>();
        RequestContainer<Family>? reached = null;
        html.On(plan, trigger => trigger.DomReady(pipeline => reached = pipeline.Container<Family>("kin")));
        Assert.That(reached, Is.Not.Null);
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.InputsAndIds.MoveIn","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{},"objects":{"$document":{"kind":"plugin","name":"alis.document","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}}},"handlers":[{"on":"$document","path":[],"event":"ready","payload":[],"members":"required","policy":"overlap","do":[]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
        Assert.That(html.ViewContext.Writer.ToString(), Is.EqualTo(""));
    }

    [Test]
    public void two_containers_keep_two_summaries_and_their_inputs_whichever_renders_first()
    {
        var html = Views.Of(new MoveIn());
        var plan = html.ReactivePlan<MoveIn>();
        MoveInPlans.Box(html, plan, m => m.Room);
        using (html.BeginRequestContainer(plan, "intake"))
        {
            using (html.BeginRequestContainer(plan, "kin"))
                MoveInPlans.Box(html, plan, m => m.Nights);
        }
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo(MoveInPlans.Head + "\"" + """Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Nights":{"name":"Nights","path":["nights"],"converted":{"kind":"as","expr":{"kind":"get","path":["value"],"members":"required","target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Nights"}},"shape":{"kind":"nullable","inner":{"kind":"number","of":"integer","text":"view","overflow":"nothing"}}},"blank":{"kind":"lit","value":5},"sent":""" + MoveInPlans.NumberSent + ""","committed":"change","invalidClass":"alis-has-error","message":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Nights_message"},"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room":{"name":"Room","path":["room"],"converted":{"kind":"as","expr":{"kind":"get","path":["value"],"members":"required","target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room"}},"shape":{"kind":"nullable","inner":{"kind":"string","date":"none","negativeZero":"0"}}},"blank":{"kind":"lit","value":""},"sent":""" + MoveInPlans.TextSent + ""","committed":"change","invalidClass":"alis-has-error","message":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room_message"}},"containers":{"intake":{"model":"Alis.Reactive.ModuleTests.InputsAndIds.MoveIn","prefix":{"name":"","path":[]},"summary":"intake_summary","live":[]},"kin":{"model":"Alis.Reactive.ModuleTests.InputsAndIds.MoveIn","prefix":{"name":"","path":[]},"summary":"kin_summary","live":[]}}}},"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Nights":{"kind":"element","id":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Nights","vendor":"native"},"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room":{"kind":"element","id":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room","vendor":"native"}""" + MoveInPlans.Tail));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_container_id_rendered_twice_is_refused_but_another_id_renders()
    {
        var html = Views.Of(new Family());
        var plan = html.ReactivePlan<Family>();
        using (html.BeginRequestContainer(plan, "intake")) { }
        Assert.That(() => html.BeginRequestContainer(plan, "intake"), Throws.InvalidOperationException.With.Message.EqualTo("Input 'intake' renders element id 'intake' a second time in this plan: render the field once, or each copy for its own member with Html.PartialFor."));
        using (html.BeginRequestContainer(plan, "kin")) { }
        html.RenderPlan(plan);
    }

    [Test]
    public void a_container_cannot_reuse_an_input_id_or_a_summary_id()
    {
        var html = Views.Of(new MoveIn());
        var plan = html.ReactivePlan<MoveIn>();
        MoveInPlans.Box(html, plan, m => m.Room);
        Assert.That(() => html.BeginRequestContainer(plan, "Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room"), Throws.InvalidOperationException.With.Message.EqualTo("Input 'Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room' renders element id 'Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room' a second time in this plan: render the field once, or each copy for its own member with Html.PartialFor."));
        using (html.BeginRequestContainer(plan, "kin")) { }
        Assert.That(() => html.BeginRequestContainer(plan, "kin_summary"), Throws.InvalidOperationException.With.Message.EqualTo("Input 'kin_summary' renders element id 'kin_summary' a second time in this plan: render the field once, or each copy for its own member with Html.PartialFor."));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_container_cannot_reuse_an_inputs_message_id()
    {
        var html = Views.Of(new MoveIn());
        var plan = html.ReactivePlan<MoveIn>();
        MoveInPlans.Box(html, plan, m => m.Room);
        Assert.That(() => html.BeginRequestContainer(plan, "Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room_message"), Throws.InvalidOperationException.With.Message.EqualTo("Input 'Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room_message' renders element id 'Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room_message' a second time in this plan: render the field once, or each copy for its own member with Html.PartialFor."));
        using (html.BeginRequestContainer(plan, "kin")) { }
        html.RenderPlan(plan);
    }

    [Test]
    public void an_inputs_message_cannot_reuse_a_container_id()
    {
        var html = Views.Of(new MoveIn());
        var plan = html.ReactivePlan<MoveIn>();
        using (html.BeginRequestContainer(plan, "Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room_message")) { }
        Assert.That(() => MoveInPlans.Box(html, plan, m => m.Room), Throws.InvalidOperationException.With.Message.EqualTo("Input 'Room' renders element id 'Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room_message' a second time in this plan: render the field once, or each copy for its own member with Html.PartialFor."));
        MoveInPlans.Box(html, plan, m => m.Nights);
        html.RenderPlan(plan);
    }

    [Test]
    public void a_hidden_input_leaves_its_message_id_available_to_a_container()
    {
        var html = Views.Of(new MoveIn());
        var plan = html.ReactivePlan<MoveIn>();
        var field = html.InputField(plan, m => m.Room, options => options.Hidden());
        var rendered = field.Render<IntakeBox>(new HtmlString("<input>"));
        using (html.BeginRequestContainer(plan, "Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room_message")) { }
        Assert.That(Views.TextOf(rendered), Is.EqualTo("<input>"));
        Assert.That(html.ViewContext.Writer.ToString(), Is.EqualTo("""<fieldset id="Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room_message"><div id="Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room_message_summary" class="validation-summary-valid" hidden><ul></ul></div></fieldset>"""));
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo(MoveInPlans.Head + "\"" + """Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room":{"name":"Room","path":["room"],"converted":{"kind":"as","expr":{"kind":"get","path":["value"],"members":"required","target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room"}},"shape":{"kind":"nullable","inner":{"kind":"string","date":"none","negativeZero":"0"}}},"blank":{"kind":"lit","value":""},"sent":""" + MoveInPlans.TextSent + ""","committed":"change","invalidClass":"alis-has-error","message":""}},"containers":{"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room_message":{"model":"Alis.Reactive.ModuleTests.InputsAndIds.MoveIn","prefix":{"name":"","path":[]},"summary":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room_message_summary","live":[]}}}},"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room":{"kind":"element","id":"Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room","vendor":"native"}""" + MoveInPlans.Tail));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void an_input_cannot_reuse_a_container_id()
    {
        var html = Views.Of(new MoveIn());
        var plan = html.ReactivePlan<MoveIn>();
        using (html.BeginRequestContainer(plan, "Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room")) { }
        Assert.That(() => MoveInPlans.Box(html, plan, m => m.Room), Throws.InvalidOperationException.With.Message.EqualTo("Input 'Room' renders element id 'Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room' a second time in this plan: render the field once, or each copy for its own member with Html.PartialFor."));
        MoveInPlans.Box(html, plan, m => m.Nights);
        html.RenderPlan(plan);
    }

    [Test]
    public void a_block_refuses_a_prefix_taken_after_its_plan_started()
    {
        var html = Views.Of(new Family());
        html.ViewContext.ExecutingFilePath = "/InputsAndIds/Family.cshtml";
        var plan = html.ReactivePlan<Family>();
        using var block = html.BeginRequestContainer(plan, "intake");
        html.ViewData.TemplateInfo.HtmlFieldPrefix = "Kin";
        Assert.That(() => block.Dispose(), Throws.InvalidOperationException.With.Message.EqualTo("The field prefix of /InputsAndIds/Family.cshtml changed from none to 'Kin' inside its plan, so what the view renders after the change posts under names its plan does not send or check: render a member's view with Html.PartialFor, which leaves the view's prefix as it is."));
        html.ViewData.TemplateInfo.HtmlFieldPrefix = "";
        block.Dispose();
        html.RenderPlan(plan);
    }

    [Test]
    public void a_block_accepts_the_prefix_its_plan_started_with_and_encodes_its_element_id()
    {
        var html = Views.Of(new Family());
        html.ViewData.TemplateInfo.HtmlFieldPrefix = "Family";
        var plan = html.ReactivePlan<Family>();
        using (html.BeginRequestContainer(plan, "kin&care")) { }
        Assert.That(html.ViewContext.Writer.ToString(), Is.EqualTo("""<fieldset id="kin&amp;care"><div id="kin&amp;care_summary" class="validation-summary-valid" hidden><ul></ul></div></fieldset>"""));
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo(Head + "\"" + """kin\u0026care":{"model":"Alis.Reactive.ModuleTests.InputsAndIds.Family","prefix":{"name":"Family","path":["family"]},"summary":"kin\u0026care_summary","live":[]}""" + Tail));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }
}
