using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Conditions;
using Alis.Reactive.RenderingAndDelivery;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.InputsAndIds;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAContainerReadsAMember
{
    private const string Sent = """{"kind":"call","path":["sent"],"members":"required","target":{"kind":"obj","id":"$model"},"args":[{"kind":"lit","value":"intake"},{"kind":"lit","value":"Alis.Reactive.ModuleTests.InputsAndIds.ReadCard"}]}""";

    [TestCase("form")]
    [TestCase("element")]
    [TestCase("reached")]
    public void each_handle_reads_the_sent_body_by_wire_name_in_its_step(string handle)
    {
        var html = Views.Of(new ReadCard());
        var plan = html.ReactivePlan<ReadCard>();
        using var form = handle == "form" ? html.BeginRequestForm(plan, "intake") : null;
        using var element = handle == "element" ? html.BeginRequestContainer(plan, "intake") : null;
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var container = form?.Form ?? element?.Container ?? pipeline.Container<ReadCard>("intake");
            pipeline.Component<IntakeBox>("status").Value.Set(container.Read(m => m.Kin.Name));
        }));
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        using var document = JsonDocument.Parse(json);
        Assert.That(document.RootElement.GetProperty("handlers")[0].GetProperty("do")[0].GetProperty("value").GetRawText(),
            Is.EqualTo("""{"kind":"data","path":[{"name":"contact","requires":[],"absent":{"kind":"start","value":""}},{"name":"name","requires":[],"absent":{"kind":"start","value":""}}],"target":""" + Sent + "}"));
        Assert.That(document.RootElement.GetProperty("objects").GetProperty("$model").GetProperty("name").GetString(), Is.EqualTo("alis.model"));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_reached_handle_names_its_model_even_in_another_models_plan()
    {
        var html = Views.Of(new MoveIn());
        var plan = html.ReactivePlan<MoveIn>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
            pipeline.Component<IntakeBox>("status").Value.Set(pipeline.Container<ReadCard>("intake").Read(m => m.Kin.Name))));
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        using var document = JsonDocument.Parse(json);
        Assert.That(document.RootElement.GetProperty("handlers")[0].GetProperty("do")[0].GetProperty("value").GetProperty("target").GetRawText(), Is.EqualTo(Sent));
        Assert.That(document.RootElement.GetProperty("objects").GetProperty("$model").GetProperty("data").GetRawText(), Is.EqualTo("""{"kind":"model","inputs":{},"containers":{}}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_list_position_reads_nothing_when_the_sent_body_has_no_item()
        => Assert.That(Read(m => m.Contacts[2].Name),
            Is.EqualTo("""{"kind":"data","path":[{"name":"name","requires":[],"absent":{"kind":"start","value":""}}],"target":{"kind":"get","path":[2],"members":"optional","target":{"kind":"data","path":[{"name":"contacts","requires":[],"absent":{"kind":"start","value":[]}}],"target":""" + Sent + "}}}"));

    [Test]
    public void a_dictionary_entry_reads_nothing_when_the_sent_body_has_no_key()
    {
        var key = "night";
        Assert.That(Read(m => m.Rounds[key].Name),
            Is.EqualTo("""{"kind":"data","path":[{"name":"name","requires":[],"absent":{"kind":"start","value":""}}],"target":{"kind":"data","path":[{"name":"night","requires":[],"absent":{"kind":"nothing"}}],"target":{"kind":"data","path":[{"name":"rounds","requires":[],"absent":{"kind":"start","value":{}}}],"target":""" + Sent + "}}}"));
    }

    [Test]
    public void a_null_start_and_an_unbound_start_keep_their_declared_absences()
    {
        Assert.That(Read(m => m.Absent!.Name),
            Is.EqualTo("""{"kind":"data","path":[{"name":"absent","requires":[],"absent":{"kind":"nothing"}},{"name":"name","requires":[],"absent":{"kind":"start","value":""}}],"target":""" + Sent + "}"));
        Assert.That(Read(m => m.Log.Initials),
            Is.EqualTo("""{"kind":"data","path":[{"name":"log","requires":[],"absent":{"kind":"start","value":""}},{"name":"initials","requires":[],"absent":{"kind":"unbound"}}],"target":""" + Sent + "}"));
    }

    [Test]
    public void the_read_keeps_the_live_form_under_the_apps_number_handling()
    {
        var html = Views.Of(new ReadCard(), options => options.NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString);
        var plan = html.ReactivePlan<ReadCard>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
            pipeline.Component<IntakeNumber>("status").Value.Set(pipeline.Container<ReadCard>("intake").Read(m => m.Nights))));
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        using var document = JsonDocument.Parse(json);
        Assert.That(document.RootElement.GetProperty("handlers")[0].GetProperty("do")[0].GetProperty("value").GetRawText(),
            Is.EqualTo("""{"kind":"data","path":[{"name":"nights","requires":[],"absent":{"kind":"start","value":5}}],"target":""" + Sent + "}"));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_number_members_own_number_handling_compares_in_the_inputs_live_form()
    {
        var html = Views.Of(new NumberCard());
        var plan = html.ReactivePlan<NumberCard>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
            pipeline.When(pipeline.Container<NumberCard>("intake").Read(m => m.Extra).Eq(7))
                .Then(pipeline => pipeline.Component<IntakeBox>("status").Value.Set("ready"))));
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        using var document = JsonDocument.Parse(json);
        Assert.That(document.RootElement.GetProperty("handlers")[0].GetProperty("do")[0].GetProperty("when").GetRawText(),
            Is.EqualTo("""{"kind":"op","op":"eq","args":[{"kind":"as","expr":{"kind":"data","path":[{"name":"extra","requires":[],"absent":{"kind":"start","value":"7"}}],"target":{"kind":"call","path":["sent"],"members":"required","target":{"kind":"obj","id":"$model"},"args":[{"kind":"lit","value":"intake"},{"kind":"lit","value":"Alis.Reactive.ModuleTests.InputsAndIds.NumberCard"}]}},"shape":{"kind":"number","of":"integer","text":"invariant","overflow":"nothing"}},{"kind":"lit","value":7}]}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    private static string Read(Expression<Func<ReadCard, string>> member)
    {
        var html = Views.Of(new ReadCard());
        var plan = html.ReactivePlan<ReadCard>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
            pipeline.Component<IntakeBox>("status").Value.Set(pipeline.Container<ReadCard>("intake").Read(member))));
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        using var document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty("handlers")[0].GetProperty("do")[0].GetProperty("value").GetRawText();
    }
}

public sealed class ReadCard
{
    [JsonPropertyName("contact")]
    public NextOfKin Kin { get; set; } = new();
    public List<NextOfKin> Contacts { get; set; } = [];
    public Dictionary<string, NextOfKin> Rounds { get; set; } = [];
    public NextOfKin? Absent { get; set; }
    public RoundLog Log { get; set; } = new NightLog();
    public int Nights { get; set; } = 5;
}
