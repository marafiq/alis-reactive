using System;
using System.Text.Json;
using Alis.Reactive.ArrayLambdas;
using Alis.Reactive.Conditions;
using Alis.Reactive.Declarations;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.ValuesAndSources;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Declarations;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAMethodReturnsAnObject
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";
    private const string OnLoad =
        """{"version":7,"planId":"Alis.Reactive.ModuleTests.Declarations.Infusion","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"$document":{"kind":"plugin","name":"alis.document","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}},"pump":{"kind":"element","id":"pump","vendor":"native"}},"handlers":[{"on":"$document","path":[],"event":"ready","payload":[],"members":"required","policy":"overlap","do":[""";
    private const string Ends = "]}]}";

    [Test]
    public void openPanel_keeps_one_returned_object_across_a_write()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var panel = pump.OpenPanel();
            pump.Rate.Set(999);
            pump.Label.Set(panel.Caption);
            pump.Label.Set(panel.Caption);
            pump.Label.Set(panel.Hint);
        }));
        const string expected = OnLoad +
            """{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["openPanel"],"members":"required","target":{"kind":"obj","id":"pump"},"args":[]}},{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"lit","value":999}},{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["caption"],"members":"required","target":{"kind":"ref","name":"result1"}}},{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["caption"],"members":"required","target":{"kind":"ref","name":"result1"}}},{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["hint"],"members":"optionalMember","target":{"kind":"ref","name":"result1"}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void locatePanel_keeps_one_returned_object_across_a_write()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var panel = pump.LocatePanel();
            pump.Rate.Set(999);
            pump.Label.Set(panel.Caption);
            pump.Label.Set(panel.Caption);
            pump.Label.Set(panel.Hint);
        }));
        const string expected = OnLoad +
            """{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["locatePanel"],"members":"required","target":{"kind":"obj","id":"pump"},"args":[]}},{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"lit","value":999}},{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["caption"],"members":"requiredIfHeld","target":{"kind":"ref","name":"result1"}}},{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["caption"],"members":"requiredIfHeld","target":{"kind":"ref","name":"result1"}}},{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["hint"],"members":"optional","target":{"kind":"ref","name":"result1"}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void try_panel_keeps_one_returned_object_across_a_write()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var panel = pump.TryPanel();
            pump.Rate.Set(999);
            pump.Label.Set(panel.Caption);
            pump.Label.Set(panel.Caption);
            pump.Label.Set(panel.Hint);
        }));
        const string expected = OnLoad +
            """{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["try-panel"],"members":"optionalMember","target":{"kind":"obj","id":"pump"},"args":[]}},{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"lit","value":999}},{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["caption"],"members":"requiredIfHeld","target":{"kind":"ref","name":"result1"}}},{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["caption"],"members":"requiredIfHeld","target":{"kind":"ref","name":"result1"}}},{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["hint"],"members":"optional","target":{"kind":"ref","name":"result1"}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void open_panel_keeps_one_returned_object_across_a_write()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var panel = pump.OpenPanelAt(2);
            pump.Rate.Set(999);
            pump.Label.Set(panel.Caption);
            pump.Label.Set(panel.Caption);
            pump.Label.Set(panel.Hint);
        }));
        const string expected = OnLoad +
            """{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["open-panel"],"members":"optionalMember","target":{"kind":"obj","id":"pump"},"args":[{"kind":"lit","value":2}]}},{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"lit","value":999}},{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["caption"],"members":"requiredIfHeld","target":{"kind":"ref","name":"result1"}}},{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["caption"],"members":"requiredIfHeld","target":{"kind":"ref","name":"result1"}}},{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["hint"],"members":"optional","target":{"kind":"ref","name":"result1"}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void openChannels_counts_the_returned_list()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var channels = pump.OpenChannels();
            pump.Rate.Set(channels.Count());
        }));
        const string expected = OnLoad +
            """{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["openChannels"],"members":"required","target":{"kind":"obj","id":"pump"},"args":[]}},{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["length"],"members":"requiredIfHeld","target":{"kind":"ref","name":"result1"}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void locate_channels_counts_the_returned_list()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var channels = pump.LocateChannels(2);
            pump.Rate.Set(channels.Count());
        }));
        const string expected = OnLoad +
            """{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["locate-channels"],"members":"optionalMember","target":{"kind":"obj","id":"pump"},"args":[{"kind":"lit","value":2}]}},{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["length"],"members":"requiredIfHeld","target":{"kind":"ref","name":"result1"}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_returned_object_used_by_a_later_term_keeps_the_existing_binds_decision()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pipeline.When(pump.FlowRate.Eq(0).Or(pump.OpenPanel().Caption.Eq("Ready"))).Then(pipeline => pump.Rate.Set(40));
        }));
        const string expected = OnLoad +
            """{"kind":"let","name":"result1","await":false,"binds":"read","expr":{"kind":"call","path":["openPanel"],"members":"required","target":{"kind":"obj","id":"pump"},"args":[]}},{"kind":"if","when":{"kind":"op","op":"or","args":[{"kind":"op","op":"eq","args":[{"kind":"get","path":["flowRate"],"members":"required","target":{"kind":"obj","id":"pump"}},{"kind":"lit","value":0}]},{"kind":"op","op":"eq","args":[{"kind":"get","path":["caption"],"members":"required","target":{"kind":"ref","name":"result1"}},{"kind":"lit","value":"Ready"}]}]},"then":[{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"lit","value":40}}],"else":[]}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }
}
