using System.Collections.Generic;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Html;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.InputsAndIds;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAnInputBindsMoreThanOneText
{
    private const string Cure = "and an input binds a member the browser sends as one text, a list of texts or files: bind an input to each value it holds, render its view with Html.PartialFor, or Set it in a JSON body from the component that holds it.";

    [Test]
    public void an_input_bound_to_an_object_is_refused_and_one_bound_to_its_member_is_bound()
    {
        var html = Views.Of(new MoveIn());
        var plan = html.ReactivePlan<MoveIn>();
        Assert.That(() => MoveInPlans.Box(html, plan, m => m.Kin), Throws.InvalidOperationException.With.Message.EqualTo(
            "Input 'Kin' binds NextOfKin, which the app's JSON options write as an object, " + Cure));
        Assert.That(() => MoveInPlans.Box(html, plan, m => m.Kin.Name), Throws.Nothing);
        html.RenderPlan(plan);
    }

    [Test]
    public void an_input_bound_to_an_item_of_a_list_of_objects_is_refused_and_one_bound_to_its_member_is_bound()
    {
        var html = Views.Of(new MoveIn { Doses = [new Dose()] });
        var plan = html.ReactivePlan<MoveIn>();
        Assert.That(() => MoveInPlans.Box(html, plan, m => m.Doses[0]), Throws.InvalidOperationException.With.Message.EqualTo(
            "Input 'Doses[0]' binds Dose, which the app's JSON options write as an object, " + Cure));
        Assert.That(() => MoveInPlans.Box(html, plan, m => m.Doses[0].Amount), Throws.Nothing);
        html.RenderPlan(plan);
    }

    [Test]
    public void an_input_bound_to_a_list_of_objects_is_refused_and_one_bound_to_a_list_of_texts_is_bound()
    {
        var html = Views.Of(new MoveIn());
        var plan = html.ReactivePlan<MoveIn>();
        Assert.That(() => MoveInPlans.Box(html, plan, m => m.Doses), Throws.InvalidOperationException.With.Message.EqualTo(
            "Input 'Doses' binds List<Dose>, which the app's JSON options write as a list of objects, " + Cure));
        Assert.That(() => MoveInPlans.Box(html, plan, m => m.Items), Throws.Nothing);
        html.RenderPlan(plan);
    }

    [Test]
    public void an_input_bound_to_a_dictionary_is_refused_and_one_bound_to_an_entry_is_bound()
    {
        var html = Views.Of(new MoveIn { Settled = { [true] = 2 } });
        var plan = html.ReactivePlan<MoveIn>();
        Assert.That(() => MoveInPlans.Box(html, plan, m => m.Settled), Throws.InvalidOperationException.With.Message.EqualTo(
            "Input 'Settled' binds Dictionary<bool, int>, which the app's JSON options write as a dictionary, " + Cure));
        Assert.That(() => MoveInPlans.Box(html, plan, m => m.Settled[true]), Throws.Nothing);
        html.RenderPlan(plan);
    }

    [Test]
    public void an_input_bound_to_a_dictionary_of_objects_is_refused_and_one_bound_to_a_member_of_an_entry_is_bound()
    {
        var html = Views.Of(new MoveIn { Rounds = { ["morning"] = new Dose() } });
        var plan = html.ReactivePlan<MoveIn>();
        Assert.That(() => MoveInPlans.Box(html, plan, m => m.Rounds), Throws.InvalidOperationException.With.Message.EqualTo(
            "Input 'Rounds' binds Dictionary<string, Dose>, which the app's JSON options write as a dictionary, " + Cure));
        Assert.That(() => MoveInPlans.Box(html, plan, m => m.Rounds["morning"].Amount), Throws.Nothing);
        html.RenderPlan(plan);
    }

    [Test]
    public void an_input_bound_to_a_list_of_lists_is_refused_and_one_bound_to_an_item_of_an_inner_list_is_bound()
    {
        var html = Views.Of(new MoveIn { Grid = [[1, 2], [3]] });
        var plan = html.ReactivePlan<MoveIn>();
        Assert.That(() => MoveInPlans.Box(html, plan, m => m.Grid), Throws.InvalidOperationException.With.Message.EqualTo(
            "Input 'Grid' binds List<List<int>>, which the app's JSON options write as a list of lists, " + Cure));
        Assert.That(() => MoveInPlans.Box(html, plan, m => m.Grid[0][1]), Throws.Nothing);
        html.RenderPlan(plan);
    }

    [Test]
    public void an_input_bound_to_a_list_of_dictionaries_is_refused_and_one_bound_to_an_entry_of_an_item_is_bound()
    {
        var html = Views.Of(new ShiftTallies { Tallies = [new Dictionary<string, int> { ["night"] = 3 }] });
        var plan = html.ReactivePlan<ShiftTallies>();
        Assert.That(() => html.InputField(plan, m => m.Tallies).Render<IntakeBox>(new HtmlString("<input>")), Throws.InvalidOperationException.With.Message.EqualTo(
            "Input 'Tallies' binds List<Dictionary<string, int>>, which the app's JSON options write as a list of dictionaries, " + Cure));
        Assert.That(() => html.InputField(plan, m => m.Tallies[0]["night"]).Render<IntakeBox>(new HtmlString("<input>")), Throws.Nothing);
        html.RenderPlan(plan);
    }

    [Test]
    public void a_pipelines_reach_to_an_input_bound_to_an_object_is_refused_and_one_to_its_member_is_reached()
    {
        var html = Views.Of(new MoveIn());
        var plan = html.ReactivePlan<MoveIn>();
        Assert.That(() => html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Input(m => m.Kin))), Throws.InvalidOperationException.With.Message.EqualTo(
            "Input 'Kin' binds NextOfKin, which the app's JSON options write as an object, " + Cure));
        Assert.That(() => html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Input(m => m.Kin.Name).Component<IntakeBox>().Value.Set("Ready"))), Throws.Nothing);
        html.RenderPlan(plan);
    }
}

public sealed class ShiftTallies
{
    public List<Dictionary<string, int>> Tallies { get; set; } = [];
}
