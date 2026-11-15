using System;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.RenderingAndDelivery;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.InputsAndIds;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAFieldIsRefused
{
    [Test]
    public void a_field_a_plan_renders_twice_is_refused()
    {
        var html = Views.Of(new MoveIn());
        var plan = html.ReactivePlan<MoveIn>();
        MoveInPlans.Box(html, plan, m => m.Room);
        Assert.That(() => MoveInPlans.Box(html, plan, m => m.Room), Throws.InvalidOperationException.With.Message.EqualTo("Input 'Room' renders element id 'Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room' a second time in this plan: render the field once, or each copy for its own member with Html.PartialFor."));
        Assert.That(() => MoveInPlans.Box(html, plan, m => m.Nights), Throws.Nothing);
        html.RenderPlan(plan);
    }

    [Test]
    public void a_field_after_the_views_prefix_moved_is_refused()
    {
        var html = Views.Of(new MoveIn());
        html.ViewContext.ExecutingFilePath = "/Refusals/InputsAndIds/MoveInPrefixAfterItsPlan.cshtml";
        var plan = html.ReactivePlan<MoveIn>();
        html.ViewData.TemplateInfo.HtmlFieldPrefix = "Kin";
        Assert.That(() => html.InputField(plan, m => m.Room), Throws.InvalidOperationException.With.Message.EqualTo("The field prefix of /Refusals/InputsAndIds/MoveInPrefixAfterItsPlan.cshtml changed from none to 'Kin' after its plan started, so input 'Kin.Room' would post under a name its plan does not send or check."));
        html.ViewData.TemplateInfo.HtmlFieldPrefix = "";
        html.RenderPlan(plan);
    }

    [Test]
    public void a_field_under_the_prefix_its_plan_started_with_binds()
    {
        var html = Views.Of(new MoveIn());
        html.ViewData.TemplateInfo.HtmlFieldPrefix = "Kin";
        var plan = html.ReactivePlan<MoveIn>();
        Assert.That(html.InputField(plan, m => m.Room).Name, Is.EqualTo("Kin.Room"));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_plan_rendered_after_its_views_prefix_moved_is_refused()
    {
        var html = Views.Of(new MoveIn());
        html.ViewContext.ExecutingFilePath = "/Refusals/InputsAndIds/FamilyPrefixMovedInsideItsPlan.cshtml";
        var plan = html.ReactivePlan<MoveIn>();
        html.ViewData.TemplateInfo.HtmlFieldPrefix = "Kin";
        Assert.That(() => html.RenderPlan(plan), Throws.InvalidOperationException.With.Message.EqualTo("The field prefix of /Refusals/InputsAndIds/FamilyPrefixMovedInsideItsPlan.cshtml changed from none to 'Kin' inside its plan, so what the view renders after the change posts under names its plan does not send or check: render a member's view with Html.PartialFor, which leaves the view's prefix as it is."));
    }

    [Test]
    public void a_plan_rendered_under_the_prefix_it_started_with_renders()
    {
        var html = Views.Of(new MoveIn());
        html.ViewData.TemplateInfo.HtmlFieldPrefix = "Kin";
        var plan = html.ReactivePlan<MoveIn>();
        Assert.That(() => html.RenderPlan(plan), Throws.Nothing);
    }

    [Test]
    public void a_member_of_a_list_or_of_one_value_has_no_body_path()
    {
        var html = Views.Of(new MoveIn());
        var plan = html.ReactivePlan<MoveIn>();
        Assert.That(() => MoveInPlans.Box(html, plan, m => m.Items.Count), Throws.InvalidOperationException.With.Message.EqualTo("Input 'm => m.Items.Count' binds Count, which List<String> declares, and the app's JSON options write List<String> as a list, so the input has no body path: bind a member of a class they write as an object."));
        Assert.That(() => MoveInPlans.Box(html, plan, m => m.Room.Length), Throws.InvalidOperationException.With.Message.EqualTo("Input 'm => m.Room.Length' binds Length, which String declares, and the app's JSON options write String as one value, so the input has no body path: bind a member of a class they write as an object."));
        Assert.That(() => MoveInPlans.Box(html, plan, m => m.Kin.Name), Throws.Nothing);
        html.RenderPlan(plan);
    }

    [Test]
    public void a_lambda_that_names_no_member_is_refused()
    {
        var html = Views.Of(new MoveIn());
        var plan = html.ReactivePlan<MoveIn>();
        Assert.That(() => MoveInPlans.Box(html, plan, m => m), Throws.InvalidOperationException.With.Message.EqualTo("Input 'm => m' names no member of MoveIn: bind a member of the view's model, or render the member with Html.EditorFor from the view that holds it."));
        Assert.That(() => MoveInPlans.Box(html, plan, m => m.Room), Throws.Nothing);
        html.RenderPlan(plan);
    }

    [Test]
    public void a_member_an_interface_declares_is_refused()
    {
        var html = Views.Of(new MoveIn());
        var plan = html.ReactivePlan<MoveIn>();
        Assert.That(() => MoveInPlans.Box(html, plan, m => m.Visitor.Name), Throws.InvalidOperationException.With.Message.EqualTo("Input 'Visitor.Name' binds a member that INamedVisitor declares. An input's id is scoped by the class that declares its member, so the page's rules, sends and reads of it would not find it: type the view to the class, or a base class, that declares it."));
        Assert.That(IdGenerator.For<Visitor, string>(m => m.Name), Is.EqualTo("Alis_Reactive_ModuleTests_InputsAndIds_Visitor__Name"));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_member_a_nullable_declares_is_refused_naming_the_member_to_bind()
    {
        var html = Views.Of(new MoveIn());
        var plan = html.ReactivePlan<MoveIn>();
        Assert.That(() => MoveInPlans.Box(html, plan, m => m.Strength!.Value), Throws.InvalidOperationException.With.Message.EqualTo("Input 'm => m.Strength.Value' binds Value, which Decimal? declares, not a member of MoveIn: bind 'm => m.Strength', which takes a Decimal?."));
        Assert.That(() => MoveInPlans.Box(html, plan, m => m.Strength), Throws.Nothing);
        html.RenderPlan(plan);
    }

    [Test]
    public void a_lambda_mvc_names_only_in_part_is_refused()
    {
        var html = Views.Of(new MoveIn { Doses = [new Dose()] });
        var plan = html.ReactivePlan<MoveIn>();
        Assert.That(() => MoveInPlans.Box(html, plan, m => m.Room.Trim()), Throws.InvalidOperationException.With.Message.EqualTo("'m => m.Room.Trim()' binds no member of the view's model. An input is bound to a member the model reaches through members, list positions and indexers, as m => m.Contacts[0].Phone."));
        Assert.That(() => MoveInPlans.Box(html, plan, m => m.Doses[0].Amount), Throws.Nothing);
        html.RenderPlan(plan);
    }

    [Test]
    public void a_lambda_that_reads_a_value_the_view_holds_is_refused()
    {
        var html = Views.Of(new MoveIn());
        var plan = html.ReactivePlan<MoveIn>();
        var held = new NextOfKin { Name = "Ada" };
        Assert.That(() => MoveInPlans.Box(html, plan, _ => held.Name), Throws.InvalidOperationException.With.Message.EqualTo("An input's lambda reads its member from a value the view holds, not from the view's model, as m => visitor.Name over a foreach's item does, so MVC names it by that value and the form posts it under no member: read the member through the model, as m => m.Visitors[i].Name in a for loop."));
        Assert.That(() => MoveInPlans.Box(html, plan, m => m.Kin.Name), Throws.Nothing);
        html.RenderPlan(plan);
    }

    [Test]
    public void a_member_of_a_class_bound_by_the_type_its_data_names_has_no_start_and_is_refused()
    {
        var html = Views.Of(new Arrival());
        var plan = html.ReactivePlan<Arrival>();
        Assert.That(() => html.InputField(plan, m => m.Check.Initials), Throws.InvalidOperationException.With.Message.EqualTo("Input 'm => m.Check.Initials' binds a member the app's JSON options make only from the data the endpoint is sent: an abstract class, or a class they bind by the type the data names. A blank input has no start its plan can carry: bind a member of a class they make from an empty object."));
        Assert.That(() => html.InputField(plan, m => m.Guardian.Name), Throws.Nothing);
        html.RenderPlan(plan);
    }

    [Test]
    public void a_member_of_an_abstract_class_has_no_start_and_is_refused()
    {
        var html = Views.Of(new Arrival());
        var plan = html.ReactivePlan<Arrival>();
        Assert.That(() => html.InputField(plan, m => m.Log.Initials), Throws.InvalidOperationException.With.Message.EqualTo("Input 'm => m.Log.Initials' binds a member the app's JSON options make only from the data the endpoint is sent: an abstract class, or a class they bind by the type the data names. A blank input has no start its plan can carry: bind a member of a class they make from an empty object."));
        Assert.That(() => html.InputField(plan, m => m.Guardian.Name), Throws.Nothing);
        html.RenderPlan(plan);
    }
}
