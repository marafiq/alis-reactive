using System;
using System.Linq.Expressions;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc.Rendering;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.InputsAndIds;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAPipelineReachesAnInput
{
    [Test]
    public void an_input_is_reached_under_its_views_prefix_with_its_declared_vendor()
    {
        var html = Views.Of(new Family());
        html.ViewData.TemplateInfo.HtmlFieldPrefix = "Family";
        var plan = html.ReactivePlan<Family>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Input(m => m.Kin.Name).Component<CardBox>().Value.Set("Ready")));
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.InputsAndIds.Family","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"card":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"get","path":["control"],"members":"required","target":{"kind":"ref","name":"element"}}},"listen":{"kind":"method","subscribe":"on","unsubscribe":"off"}}},"objects":{"$document":{"kind":"plugin","name":"alis.document","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}},"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Kin_Name":{"kind":"element","id":"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Kin_Name","vendor":"card"}},"handlers":[{"on":"$document","path":[],"event":"ready","payload":[],"members":"required","policy":"overlap","do":[{"kind":"set","path":["value"],"members":"required","content":false,"target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Kin_Name"},"value":{"kind":"lit","value":"Ready"}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void an_input_without_a_prefix_is_reached_by_the_same_id_as_its_field()
    {
        var html = Views.Of(new Family());
        html.ViewData.TemplateInfo.HtmlFieldPrefix = "";
        var plan = html.ReactivePlan<Family>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Input(m => m.Kin.Name).Component<IntakeBox>().Value.Set("Ready")));
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.InputsAndIds.Family","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"$document":{"kind":"plugin","name":"alis.document","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}},"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Kin_Name":{"kind":"element","id":"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Kin_Name","vendor":"native"}},"handlers":[{"on":"$document","path":[],"event":"ready","payload":[],"members":"required","policy":"overlap","do":[{"kind":"set","path":["value"],"members":"required","content":false,"target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Kin_Name"},"value":{"kind":"lit","value":"Ready"}}]}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void reaching_an_input_after_the_views_prefix_moved_is_refused()
    {
        var html = Views.Of(new Family());
        html.ViewContext.ExecutingFilePath = "/InputsAndIds/Family.cshtml";
        var plan = html.ReactivePlan<Family>();
        html.ViewData.TemplateInfo.HtmlFieldPrefix = "Kin";
        Assert.That(() => html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Input(m => m.Room))), Throws.InvalidOperationException.With.Message.EqualTo("The field prefix of /InputsAndIds/Family.cshtml changed from none to 'Kin' after its plan started, so input 'Kin.Room' would post under a name its plan does not send or check."));
        html.ViewData.TemplateInfo.HtmlFieldPrefix = "";
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Input(m => m.Room).Component<IntakeBox>().Value.Set("Ready")));
        html.RenderPlan(plan);
    }

    [Test]
    public void an_input_read_refuses_a_lambda_mvc_names_only_in_part()
        => Assert.That(() => Reach(m => m.Room.Trim()), Throws.InvalidOperationException.With.Message.EqualTo("'m => m.Room.Trim()' binds no member of the view's model. An input is bound to a member the model reaches through members, list positions and indexers, as m => m.Contacts[0].Phone."));

    [Test]
    public void an_input_read_refuses_a_value_the_view_holds()
    {
        var held = new NextOfKin();
        Assert.That(() => Reach(_ => held.Name), Throws.InvalidOperationException.With.Message.EqualTo("An input's lambda reads its member from a value the view holds, not from the view's model, as m => visitor.Name over a foreach's item does, so MVC names it by that value and the form posts it under no member: read the member through the model, as m => m.Visitors[i].Name in a for loop."));
    }

    [Test]
    public void an_input_read_refuses_a_lambda_that_names_no_member()
        => Assert.That(() => Reach(m => m), Throws.InvalidOperationException.With.Message.EqualTo("Input 'm => m' names no member of Family: bind a member of the view's model, or render the member with Html.EditorFor from the view that holds it."));

    [Test]
    public void an_input_read_refuses_a_member_an_interface_declares()
        => Assert.That(() => Reach(m => m.Visitor.Name), Throws.InvalidOperationException.With.Message.EqualTo("Input 'Visitor.Name' binds a member that INamedVisitor declares. An input's id is scoped by the class that declares its member, so the page's rules, sends and reads of it would not find it: type the view to the class, or a base class, that declares it."));

    [Test]
    public void an_input_read_refuses_a_member_a_nullable_declares()
        => Assert.That(() => Reach(m => m.Dose!.Value), Throws.InvalidOperationException.With.Message.EqualTo("Input 'm => m.Dose.Value' binds Value, which decimal? declares, not a member of Family: bind 'm => m.Dose', which takes a decimal?."));

    [Test]
    public void an_input_read_refuses_a_member_of_a_list_or_one_value()
    {
        Assert.That(() => Reach(m => m.Members.Count), Throws.InvalidOperationException.With.Message.EqualTo("Input 'm => m.Members.Count' binds Count, which List<NextOfKin> declares, and the app's JSON options write List<NextOfKin> as a list, so the input has no body path: bind a member of a class they write as an object."));
        Assert.That(() => Reach(m => m.Room.Length), Throws.InvalidOperationException.With.Message.EqualTo("Input 'm => m.Room.Length' binds Length, which string declares, and the app's JSON options write string as one value, so the input has no body path: bind a member of a class they write as an object."));
    }

    private static void Reach<TValue>(Expression<Func<Family, TValue>> member)
    {
        var html = Views.Of(new Family());
        var plan = html.ReactivePlan<Family>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Input(member)));
        html.RenderPlan(plan);
    }
}
