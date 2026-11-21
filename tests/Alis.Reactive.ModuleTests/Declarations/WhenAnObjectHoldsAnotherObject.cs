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
public sealed class WhenAnObjectHoldsAnotherObject
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";
    private const string OnLoad =
        """{"version":7,"planId":"Alis.Reactive.ModuleTests.Declarations.Infusion","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"$document":{"kind":"plugin","name":"alis.document","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}},"pump":{"kind":"element","id":"pump","vendor":"native"}},"handlers":[{"on":"$document","path":[],"event":"ready","payload":[],"members":"required","policy":"overlap","do":[""";
    private const string Ends = "]}]}";

    [Test]
    public void panel_members_keep_their_own_presence()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var panel = pump.Panel;
            pump.Label.Set(panel.Caption);
            pump.Label.Set(panel.Hint);
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["caption"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}}},{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["hint"],"members":"optionalMember","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void backuppanel_members_keep_their_own_presence()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var panel = pump.BackupPanel;
            pump.Label.Set(panel.Caption);
            pump.Label.Set(panel.Hint);
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["caption"],"members":"requiredIfHeld","target":{"kind":"get","path":["backupPanel"],"members":"optionalMember","target":{"kind":"obj","id":"pump"}}}},{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["hint"],"members":"optional","target":{"kind":"get","path":["backupPanel"],"members":"optionalMember","target":{"kind":"obj","id":"pump"}}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void detachedpanel_members_keep_their_own_presence()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var panel = pump.DetachedPanel;
            pump.Label.Set(panel.Caption);
            pump.Label.Set(panel.Hint);
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["caption"],"members":"requiredIfHeld","target":{"kind":"get","path":["detachedPanel"],"members":"required","target":{"kind":"obj","id":"pump"}}}},{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["hint"],"members":"optional","target":{"kind":"get","path":["detachedPanel"],"members":"required","target":{"kind":"obj","id":"pump"}}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void sparepanel_members_keep_their_own_presence()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var panel = pump.SparePanel;
            pump.Label.Set(panel.Caption);
            pump.Label.Set(panel.Hint);
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["caption"],"members":"requiredIfHeld","target":{"kind":"get","path":["spare-panel"],"members":"optionalMember","target":{"kind":"obj","id":"pump"}}}},{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["hint"],"members":"optional","target":{"kind":"get","path":["spare-panel"],"members":"optionalMember","target":{"kind":"obj","id":"pump"}}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void display_at_depth_three_uses_only_its_immediate_holder()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var display = pump.BackupPanel.Display;
            pump.Label.Set(display.Caption);
            pump.Label.Set(display.Hint);
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["caption"],"members":"required","target":{"kind":"get","path":["display"],"members":"requiredIfHeld","target":{"kind":"get","path":["backupPanel"],"members":"optionalMember","target":{"kind":"obj","id":"pump"}}}}},{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["hint"],"members":"optionalMember","target":{"kind":"get","path":["display"],"members":"requiredIfHeld","target":{"kind":"get","path":["backupPanel"],"members":"optionalMember","target":{"kind":"obj","id":"pump"}}}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void backupdisplay_at_depth_three_uses_only_its_immediate_holder()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var display = pump.BackupPanel.BackupDisplay;
            pump.Label.Set(display.Caption);
            pump.Label.Set(display.Hint);
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["caption"],"members":"requiredIfHeld","target":{"kind":"get","path":["backupDisplay"],"members":"optional","target":{"kind":"get","path":["backupPanel"],"members":"optionalMember","target":{"kind":"obj","id":"pump"}}}}},{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["hint"],"members":"optional","target":{"kind":"get","path":["backupDisplay"],"members":"optional","target":{"kind":"get","path":["backupPanel"],"members":"optionalMember","target":{"kind":"obj","id":"pump"}}}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void detacheddisplay_at_depth_three_uses_only_its_immediate_holder()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var display = pump.BackupPanel.DetachedDisplay;
            pump.Label.Set(display.Caption);
            pump.Label.Set(display.Hint);
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["caption"],"members":"requiredIfHeld","target":{"kind":"get","path":["detachedDisplay"],"members":"requiredIfHeld","target":{"kind":"get","path":["backupPanel"],"members":"optionalMember","target":{"kind":"obj","id":"pump"}}}}},{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["hint"],"members":"optional","target":{"kind":"get","path":["detachedDisplay"],"members":"requiredIfHeld","target":{"kind":"get","path":["backupPanel"],"members":"optionalMember","target":{"kind":"obj","id":"pump"}}}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void sparedisplay_at_depth_three_uses_only_its_immediate_holder()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var display = pump.BackupPanel.SpareDisplay;
            pump.Label.Set(display.Caption);
            pump.Label.Set(display.Hint);
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["caption"],"members":"requiredIfHeld","target":{"kind":"get","path":["spareDisplay"],"members":"optional","target":{"kind":"get","path":["backupPanel"],"members":"optionalMember","target":{"kind":"obj","id":"pump"}}}}},{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["hint"],"members":"optional","target":{"kind":"get","path":["spareDisplay"],"members":"optional","target":{"kind":"get","path":["backupPanel"],"members":"optionalMember","target":{"kind":"obj","id":"pump"}}}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_write_at_depth_keeps_only_the_leafs_presence()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.BackupPanel.Caption.Set("Delivery");
            pump.BackupPanel.Hint.Set("Check");
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["caption"],"members":"required","content":false,"target":{"kind":"get","path":["backupPanel"],"members":"optionalMember","target":{"kind":"obj","id":"pump"}},"value":{"kind":"lit","value":"Delivery"}},{"kind":"set","path":["hint"],"members":"optional","content":false,"target":{"kind":"get","path":["backupPanel"],"members":"optionalMember","target":{"kind":"obj","id":"pump"}},"value":{"kind":"lit","value":"Check"}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_write_only_member_and_a_call_keep_their_nested_target()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Panel.Display.Message.Set("Delivery");
            pump.Panel.Display.Show("Ready");
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["message"],"members":"optional","content":false,"target":{"kind":"get","path":["display"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"value":{"kind":"lit","value":"Delivery"}},{"kind":"do","await":false,"expr":{"kind":"call","path":["show"],"members":"required","target":{"kind":"get","path":["display"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"lit","value":"Ready"}]}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void data_past_a_nested_declaration_stays_data()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Rate.Set(pump.Panel.Prescription.Read(prescription => prescription.MaxDailyDoses));
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"data","path":[{"name":"maxDailyDoses","requires":[],"absent":{"kind":"start","value":4}}],"target":{"kind":"get","path":["prescription"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void panel_calls_use_the_holder_and_method_marks()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Panel.Refresh();
            pump.Panel.Silence();
            pump.Ready.Set(pump.Panel.IsReady());
            pump.Ready.Set(pump.Panel.CanStart());
        }));
        const string expected = OnLoad +
            """{"kind":"do","await":false,"expr":{"kind":"call","path":["refresh"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}},"args":[]}},{"kind":"do","await":false,"expr":{"kind":"call","path":["silence"],"members":"optionalMember","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}},"args":[]}},{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["isReady"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}},"args":[]}},{"kind":"set","path":["ready"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"ref","name":"result1"}},{"kind":"let","name":"result2","await":false,"binds":"line","expr":{"kind":"call","path":["canStart"],"members":"optionalMember","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}},"args":[]}},{"kind":"set","path":["ready"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"ref","name":"result2"}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void backuppanel_calls_use_the_holder_and_method_marks()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.BackupPanel.Refresh();
            pump.BackupPanel.Silence();
            pump.Ready.Set(pump.BackupPanel.IsReady());
            pump.Ready.Set(pump.BackupPanel.CanStart());
        }));
        const string expected = OnLoad +
            """{"kind":"do","await":false,"expr":{"kind":"call","path":["refresh"],"members":"requiredIfHeld","target":{"kind":"get","path":["backupPanel"],"members":"optionalMember","target":{"kind":"obj","id":"pump"}},"args":[]}},{"kind":"do","await":false,"expr":{"kind":"call","path":["silence"],"members":"optional","target":{"kind":"get","path":["backupPanel"],"members":"optionalMember","target":{"kind":"obj","id":"pump"}},"args":[]}},{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["isReady"],"members":"requiredIfHeld","target":{"kind":"get","path":["backupPanel"],"members":"optionalMember","target":{"kind":"obj","id":"pump"}},"args":[]}},{"kind":"set","path":["ready"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"ref","name":"result1"}},{"kind":"let","name":"result2","await":false,"binds":"line","expr":{"kind":"call","path":["canStart"],"members":"optional","target":{"kind":"get","path":["backupPanel"],"members":"optionalMember","target":{"kind":"obj","id":"pump"}},"args":[]}},{"kind":"set","path":["ready"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"ref","name":"result2"}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }
}
