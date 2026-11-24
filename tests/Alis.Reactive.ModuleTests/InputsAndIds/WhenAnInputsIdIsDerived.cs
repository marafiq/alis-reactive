using System.Collections.Generic;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Html;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.InputsAndIds;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAnInputsIdIsDerived
{
    [Test]
    public void the_id_is_the_class_that_declares_the_member_then_its_path()
    {
        Assert.That(IdGenerator.For<MoveIn, string>(m => m.Room), Is.EqualTo("Alis_Reactive_ModuleTests_InputsAndIds_MoveIn__Room"));
        Assert.That(IdGenerator.For<MoveIn, string>(m => m.Kin.Name), Is.EqualTo("Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Kin_Name"));
    }

    [Test]
    public void a_list_position_is_named_as_mvc_names_it()
        => Assert.That(IdGenerator.For<MoveIn, decimal>(m => m.Doses[1].Amount), Is.EqualTo("Alis_Reactive_ModuleTests_InputsAndIds_Dose__Doses_1__Amount"));

    [Test]
    public void a_generic_declaring_class_is_named_with_its_type_arguments()
    {
        Assert.That(IdGenerator.For<ChartEntry<FluidsReading>, string>(m => m.Initials), Is.EqualTo("Alis_Reactive_ModuleTests_InputsAndIds_ChartEntry_FluidsReading___Initials"));
        Assert.That(IdGenerator.For<ChartEntry<PainReading>, string>(m => m.Initials), Is.EqualTo("Alis_Reactive_ModuleTests_InputsAndIds_ChartEntry_PainReading___Initials"));
    }

    [Test]
    public void a_type_argument_csharp_writes_as_a_keyword_is_named_by_its_type_name()
    {
        Assert.That(IdGenerator.For<ChartEntry<int>, string>(m => m.Initials), Is.EqualTo("Alis_Reactive_ModuleTests_InputsAndIds_ChartEntry_Int32___Initials"));
        Assert.That(IdGenerator.For<ChartEntry<string>, string>(m => m.Initials), Is.EqualTo("Alis_Reactive_ModuleTests_InputsAndIds_ChartEntry_String___Initials"));
    }

    [Test]
    public void an_array_type_argument_is_named_by_its_own_type_name()
    {
        Assert.That(IdGenerator.For<ChartEntry<int[]>, string>(m => m.Initials), Is.EqualTo("Alis_Reactive_ModuleTests_InputsAndIds_ChartEntry_Int32_____Initials"));
        Assert.That(IdGenerator.For<ChartEntry<List<string>[]>, string>(m => m.Initials), Is.EqualTo("Alis_Reactive_ModuleTests_InputsAndIds_ChartEntry_List_1_____Initials"));
    }

    [Test]
    public void a_member_a_base_class_declares_takes_the_base_class_id_in_a_view_of_the_derived_class()
    {
        Assert.That(IdGenerator.For<Resident, string>(m => m.Name), Is.EqualTo("Alis_Reactive_ModuleTests_InputsAndIds_Person__Name"));
        Assert.That(IdGenerator.For<Person, string>(m => m.Name), Is.EqualTo("Alis_Reactive_ModuleTests_InputsAndIds_Person__Name"));
        Assert.That(IdGenerator.For<Resident, string>(m => m.Room), Is.EqualTo("Alis_Reactive_ModuleTests_InputsAndIds_Resident__Room"));
    }

    [Test]
    public void a_field_with_no_prefix_takes_the_id_a_view_names_it_by()
    {
        var html = Views.Of(new MoveIn());
        var plan = html.ReactivePlan<MoveIn>();
        var field = html.InputField(plan, m => m.Kin.Name);
        Assert.That(field.Id, Is.EqualTo(IdGenerator.For<MoveIn, string>(m => m.Kin.Name)));
        Assert.That(field.Name, Is.EqualTo("Kin.Name"));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_field_under_a_prefix_carries_the_prefix_in_its_name_its_id_and_its_body_path()
    {
        var html = Views.Of(new NextOfKin { Name = "Ada" });
        html.ViewData.TemplateInfo.HtmlFieldPrefix = "Kin";
        var plan = html.ReactivePlan<NextOfKin>();
        var field = html.InputField(plan, m => m.Name);
        field.Render<IntakeBox>(new HtmlString("<input>"));
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(field.Name, Is.EqualTo("Kin.Name"));
        Assert.That(field.Id, Is.EqualTo("Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Kin_Name"));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.InputsAndIds.NextOfKin","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"$model":{"kind":"plugin","name":"alis.model","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"model","inputs":{"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Kin_Name":{"name":"Kin.Name","path":["kin","name"],"converted":{"kind":"as","expr":{"kind":"get","path":["value"],"members":"required","target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Kin_Name"}},"shape":{"kind":"nullable","inner":{"kind":"string","date":"none","negativeZero":"0"}}},"blank":{"kind":"lit","value":""},"committed":"change","invalidClass":"alis-has-error","message":"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Kin_Name_message"}},"containers":{}}},"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Kin_Name":{"kind":"element","id":"Alis_Reactive_ModuleTests_InputsAndIds_NextOfKin__Kin_Name","vendor":"native"}},"handlers":[]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_field_in_a_view_of_a_list_item_takes_the_id_and_body_path_its_member_has_in_the_whole_model()
    {
        var html = Views.Of(new Dose { Amount = 0.5m });
        html.ViewData.TemplateInfo.HtmlFieldPrefix = "Doses[1]";
        var plan = html.ReactivePlan<Dose>();
        var field = html.InputField(plan, m => m.Amount);
        field.Render<IntakeBox>(new HtmlString("<input>"));
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        Assert.That(field.Name, Is.EqualTo("Doses[1].Amount"));
        Assert.That(field.Id, Is.EqualTo("Alis_Reactive_ModuleTests_InputsAndIds_Dose__Doses_1__Amount"));
        Assert.That(json, Is.EqualTo("""{"version":7,"planId":"Alis.Reactive.ModuleTests.InputsAndIds.Dose","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"$model":{"kind":"plugin","name":"alis.model","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"model","inputs":{"Alis_Reactive_ModuleTests_InputsAndIds_Dose__Doses_1__Amount":{"name":"Doses[1].Amount","path":["doses",1,"amount"],"converted":{"kind":"as","expr":{"kind":"get","path":["value"],"members":"required","target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_Dose__Doses_1__Amount"}},"shape":{"kind":"nullable","inner":{"kind":"number","of":"fraction","text":"view","overflow":"nothing"}}},"blank":{"kind":"lit","value":0},"committed":"change","invalidClass":"alis-has-error","message":"Alis_Reactive_ModuleTests_InputsAndIds_Dose__Doses_1__Amount_message"}},"containers":{}}},"Alis_Reactive_ModuleTests_InputsAndIds_Dose__Doses_1__Amount":{"kind":"element","id":"Alis_Reactive_ModuleTests_InputsAndIds_Dose__Doses_1__Amount","vendor":"native"}},"handlers":[]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }
}
