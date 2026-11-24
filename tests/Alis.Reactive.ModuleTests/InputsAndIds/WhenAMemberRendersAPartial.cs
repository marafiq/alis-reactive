using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.InputsAndIds;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAMemberRendersAPartial
{
    [Test]
    public async Task a_member_renders_its_type_named_view_under_its_full_prefix()
    {
        var html = Views.Of(new Family());
        html.ViewData.TemplateInfo.HtmlFieldPrefix = "Family";
        var original = html.ViewData.TemplateInfo;
        var rendered = Views.TextOf(await html.PartialFor(m => m.Kin));
        var scriptAt = rendered.IndexOf("<script", StringComparison.Ordinal);
        var json = MoveInPlans.JsonOf(new HtmlString(rendered[scriptAt..].Trim()));
        Assert.That(rendered[..scriptAt].Trim(), Is.EqualTo("""
            <div class="alis-field"><input id="Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Kin_Name" name="Family.Kin.Name" type="text" value="Ada" /><span id="Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Kin_Name_message" class="alis-field__message" hidden></span></div>
            """));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.InputsAndIds.NextOfKin","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"$model":{"kind":"plugin","name":"alis.model","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"model","inputs":{"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Kin_Name":{"name":"Family.Kin.Name","path":["family","kin","name"],"converted":{"kind":"as","expr":{"kind":"get","path":["value"],"members":"required","target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Kin_Name"}},"shape":{"kind":"nullable","inner":{"kind":"string","date":"none","negativeZero":"0"}}},"blank":{"kind":"lit","value":""},"committed":"change","invalidClass":"alis-has-error","message":"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Kin_Name_message"}},"containers":{}}},"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Kin_Name":{"kind":"element","id":"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Kin_Name","vendor":"native"}},"handlers":[]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
        Assert.That(html.ViewData.TemplateInfo, Is.SameAs(original));
        Assert.That(html.ViewData.TemplateInfo.HtmlFieldPrefix, Is.EqualTo("Family"));
    }

    [Test]
    public async Task a_view_name_selects_the_partial_for_the_same_members_type()
    {
        var html = Views.Of(new Family());
        html.ViewData.TemplateInfo.HtmlFieldPrefix = "Family";
        var original = html.ViewData.TemplateInfo;
        var rendered = Views.TextOf(await html.PartialFor(m => m.Alternate, "ContactCard"));
        var scriptAt = rendered.IndexOf("<script", StringComparison.Ordinal);
        var json = MoveInPlans.JsonOf(new HtmlString(rendered[scriptAt..].Trim()));
        Assert.That(rendered[..scriptAt].Trim(), Is.EqualTo("""
            <h2>Contact</h2>
            <div class="alis-field"><input id="Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Alternate_Name" name="Family.Alternate.Name" type="text" value="Grace" /><span id="Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Alternate_Name_message" class="alis-field__message" hidden></span></div>
            """));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.InputsAndIds.NextOfKin","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"$model":{"kind":"plugin","name":"alis.model","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"model","inputs":{"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Alternate_Name":{"name":"Family.Alternate.Name","path":["family","alternate","name"],"converted":{"kind":"as","expr":{"kind":"get","path":["value"],"members":"required","target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Alternate_Name"}},"shape":{"kind":"nullable","inner":{"kind":"string","date":"none","negativeZero":"0"}}},"blank":{"kind":"lit","value":""},"committed":"change","invalidClass":"alis-has-error","message":"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Alternate_Name_message"}},"containers":{}}},"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Alternate_Name":{"kind":"element","id":"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Alternate_Name","vendor":"native"}},"handlers":[]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
        Assert.That(html.ViewData.TemplateInfo, Is.SameAs(original));
        Assert.That(html.ViewData.TemplateInfo.HtmlFieldPrefix, Is.EqualTo("Family"));
    }

    [Test]
    public async Task a_list_position_renders_that_items_model_and_prefix()
    {
        var html = Views.Of(new Family());
        html.ViewData.TemplateInfo.HtmlFieldPrefix = "Family";
        var original = html.ViewData.TemplateInfo;
        var rendered = Views.TextOf(await html.PartialFor(m => m.Members[1], "NextOfKin"));
        var scriptAt = rendered.IndexOf("<script", StringComparison.Ordinal);
        var json = MoveInPlans.JsonOf(new HtmlString(rendered[scriptAt..].Trim()));
        Assert.That(rendered[..scriptAt].Trim(), Is.EqualTo("""
            <div class="alis-field"><input id="Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Members_1__Name" name="Family.Members[1].Name" type="text" value="Grace" /><span id="Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Members_1__Name_message" class="alis-field__message" hidden></span></div>
            """));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.InputsAndIds.NextOfKin","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"$model":{"kind":"plugin","name":"alis.model","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"model","inputs":{"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Members_1__Name":{"name":"Family.Members[1].Name","path":["family","members",1,"name"],"converted":{"kind":"as","expr":{"kind":"get","path":["value"],"members":"required","target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Members_1__Name"}},"shape":{"kind":"nullable","inner":{"kind":"string","date":"none","negativeZero":"0"}}},"blank":{"kind":"lit","value":""},"committed":"change","invalidClass":"alis-has-error","message":"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Members_1__Name_message"}},"containers":{}}},"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Members_1__Name":{"kind":"element","id":"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Members_1__Name","vendor":"native"}},"handlers":[]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
        Assert.That(html.ViewData.TemplateInfo, Is.SameAs(original));
        Assert.That(html.ViewData.TemplateInfo.HtmlFieldPrefix, Is.EqualTo("Family"));
    }

    [Test]
    public async Task a_dictionary_entry_renders_that_entries_model_and_prefix()
    {
        var html = Views.Of(new Family());
        html.ViewData.TemplateInfo.HtmlFieldPrefix = "Family";
        var original = html.ViewData.TemplateInfo;
        var key = "pm";
        var rendered = Views.TextOf(await html.PartialFor(m => m.Doses[key], "NextOfKin"));
        var scriptAt = rendered.IndexOf("<script", StringComparison.Ordinal);
        var json = MoveInPlans.JsonOf(new HtmlString(rendered[scriptAt..].Trim()));
        Assert.That(rendered[..scriptAt].Trim(), Is.EqualTo("""
            <div class="alis-field"><input id="Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Doses_pm__Name" name="Family.Doses[pm].Name" type="text" value="Evening" /><span id="Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Doses_pm__Name_message" class="alis-field__message" hidden></span></div>
            """));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.InputsAndIds.NextOfKin","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"$model":{"kind":"plugin","name":"alis.model","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"model","inputs":{"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Doses_pm__Name":{"name":"Family.Doses[pm].Name","path":["family","doses","pm","name"],"converted":{"kind":"as","expr":{"kind":"get","path":["value"],"members":"required","target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Doses_pm__Name"}},"shape":{"kind":"nullable","inner":{"kind":"string","date":"none","negativeZero":"0"}}},"blank":{"kind":"lit","value":""},"committed":"change","invalidClass":"alis-has-error","message":"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Doses_pm__Name_message"}},"containers":{}}},"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Doses_pm__Name":{"kind":"element","id":"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Family_Doses_pm__Name","vendor":"native"}},"handlers":[]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
        Assert.That(html.ViewData.TemplateInfo, Is.SameAs(original));
        Assert.That(html.ViewData.TemplateInfo.HtmlFieldPrefix, Is.EqualTo("Family"));
    }

    [Test]
    public async Task a_generic_type_needs_a_view_name_and_the_named_view_renders_it()
    {
        var html = Views.Of(new Family());
        Assert.That(async () => await html.PartialFor(m => m.Chart), Throws.InvalidOperationException.With.Message.EqualTo("The member's type ChartEntry<FluidsReading> is generic and names no partial view: pass the view name to Html.PartialFor."));
        var rendered = Views.TextOf(await html.PartialFor(m => m.Chart, "ReadingCard"));
        var scriptAt = rendered.IndexOf("<script", StringComparison.Ordinal);
        Assert.That(rendered[..scriptAt].Trim(), Is.EqualTo("""<div class="alis-field"><input id="Alis_Reactive_ModuleTests_InputsAndIds_ChartEntry_FluidsReading___Chart_Initials" name="Chart.Initials" type="text" value="AR" /><span id="Alis_Reactive_ModuleTests_InputsAndIds_ChartEntry_FluidsReading___Chart_Initials_message" class="alis-field__message" hidden></span></div>"""));
        var json = MoveInPlans.JsonOf(new HtmlString(rendered[scriptAt..].Trim()));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.InputsAndIds.ChartEntry\u00601[[Alis.Reactive.ModuleTests.InputsAndIds.FluidsReading, Alis.Reactive.ModuleTests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"$model":{"kind":"plugin","name":"alis.model","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"model","inputs":{"Alis_Reactive_ModuleTests_InputsAndIds_ChartEntry_FluidsReading___Chart_Initials":{"name":"Chart.Initials","path":["chart","initials"],"converted":{"kind":"as","expr":{"kind":"get","path":["value"],"members":"required","target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_ChartEntry_FluidsReading___Chart_Initials"}},"shape":{"kind":"nullable","inner":{"kind":"string","date":"none","negativeZero":"0"}}},"blank":{"kind":"lit","value":""},"committed":"change","invalidClass":"alis-has-error","message":"Alis_Reactive_ModuleTests_InputsAndIds_ChartEntry_FluidsReading___Chart_Initials_message"}},"containers":{}}},"Alis_Reactive_ModuleTests_InputsAndIds_ChartEntry_FluidsReading___Chart_Initials":{"kind":"element","id":"Alis_Reactive_ModuleTests_InputsAndIds_ChartEntry_FluidsReading___Chart_Initials","vendor":"native"}},"handlers":[]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public async Task mvc_reports_a_view_name_it_cannot_find()
    {
        var html = Views.Of(new Family());
        Assert.That(async () => await html.PartialFor(m => m.Kin, "MissingCard"), Throws.InvalidOperationException.With.Message.EqualTo("The partial view 'MissingCard' was not found. The following locations were searched:\n/InputsAndIds/MissingCard.cshtml\n/Views/MissingCard.cshtml\n/Views/Shared/MissingCard.cshtml"));
        Assert.That(Views.TextOf(await html.PartialFor(m => m.Room, "ValueCard")).Trim(), Is.EqualTo("<p>A-1</p>"));
    }

    [Test]
    public async Task a_partial_refuses_a_lambda_mvc_names_only_in_part()
    {
        var html = Views.Of(new Family());
        Assert.That(async () => await Partial(html, m => m.Room.Trim()), Throws.InvalidOperationException.With.Message.EqualTo("'m => m.Room.Trim()' binds no member of the view's model. An input is bound to a member the model reaches through members, list positions and indexers, as m => m.Contacts[0].Phone."));
        Assert.That(Views.TextOf(await Partial(html, m => m.Room)).Trim(), Is.EqualTo("<p>A-1</p>"));
    }

    [Test]
    public async Task a_partial_refuses_a_value_the_view_holds()
    {
        var html = Views.Of(new Family());
        var held = new NextOfKin { Name = "Held" };
        Assert.That(async () => await Partial(html, _ => held.Name), Throws.InvalidOperationException.With.Message.EqualTo("An input's lambda reads its member from a value the view holds, not from the view's model, as m => visitor.Name over a foreach's item does, so MVC names it by that value and the form posts it under no member: read the member through the model, as m => m.Visitors[i].Name in a for loop."));
        Assert.That(Views.TextOf(await Partial(html, m => m.Kin.Name)).Trim(), Is.EqualTo("<p>Ada</p>"));
    }

    [Test]
    public async Task a_partial_refuses_a_lambda_that_names_no_member()
    {
        var html = Views.Of(new Family());
        Assert.That(async () => await Partial(html, m => m), Throws.InvalidOperationException.With.Message.EqualTo("Input 'm => m' names no member of Family: bind a member of the view's model, or render the member with Html.EditorFor from the view that holds it."));
        Assert.That(Views.TextOf(await Partial(html, m => m.Room)).Trim(), Is.EqualTo("<p>A-1</p>"));
    }

    [Test]
    public async Task a_partial_refuses_a_member_an_interface_declares()
    {
        var html = Views.Of(new Family());
        Assert.That(async () => await Partial(html, m => m.Visitor.Name), Throws.InvalidOperationException.With.Message.EqualTo("Input 'Visitor.Name' binds a member that INamedVisitor declares. An input's id is scoped by the class that declares its member, so the page's rules, sends and reads of it would not find it: type the view to the class, or a base class, that declares it."));
        Assert.That(Views.TextOf(await Partial(html, m => m.Kin.Name)).Trim(), Is.EqualTo("<p>Ada</p>"));
    }

    [Test]
    public async Task a_partial_refuses_a_member_a_nullable_declares()
    {
        var html = Views.Of(new Family());
        Assert.That(async () => await Partial(html, m => m.Dose!.Value), Throws.InvalidOperationException.With.Message.EqualTo("Input 'm => m.Dose.Value' binds Value, which decimal? declares, not a member of Family: bind 'm => m.Dose', which takes a decimal?."));
        Assert.That(Views.TextOf(await html.PartialFor(m => m.Dose, "DoseCard")).Trim(), Is.EqualTo("<p>0.125</p>"));
    }

    private static Task<IHtmlContent> Partial<TValue>(IHtmlHelper<Family> html, Expression<Func<Family, TValue>> member)
        => html.PartialFor(member, "ValueCard");
}
