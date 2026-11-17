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
public sealed class WhenAListHoldsLiveObjects
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";
    private const string OnLoad =
        """{"version":7,"planId":"Alis.Reactive.ModuleTests.Declarations.Infusion","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"$document":{"kind":"plugin","name":"alis.document","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}},"pump":{"kind":"element","id":"pump","vendor":"native"}},"handlers":[{"on":"$document","path":[],"event":"ready","payload":[],"members":"required","policy":"overlap","do":[""";
    private const string Ends = "]}]}";

    [Test]
    public void count_reads_the_lists_length()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Rate.Set(pump.Panel.Channels.Count());
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["length"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void where_keeps_live_items_for_later_operations()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Rate.Set(pump.Panel.Channels.Where(channel => channel.Rate.Gt(0)).Count());
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["length"],"members":"requiredIfHeld","target":{"kind":"call","path":["filter"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"op","op":"gt","args":[{"kind":"get","path":["rate"],"members":"required","target":{"kind":"ref","name":"liveItem1"}},{"kind":"lit","value":0}]}}]}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void count_with_a_predicate_counts_the_filtered_items()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Rate.Set(pump.Panel.Channels.Count(channel => channel.Rate.Gt(0)));
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["length"],"members":"requiredIfHeld","target":{"kind":"call","path":["filter"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"op","op":"gt","args":[{"kind":"get","path":["rate"],"members":"required","target":{"kind":"ref","name":"liveItem1"}},{"kind":"lit","value":0}]}}]}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void any_without_a_predicate_asks_whether_some_item_exists()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Ready.Set(pump.Panel.Channels.Any());
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["ready"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"call","path":["some"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"lit","value":true}}]}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void any_reads_the_predicate_for_some_item()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Ready.Set(pump.Panel.Channels.Any(channel => channel.Rate.Gt(0)));
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["ready"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"call","path":["some"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"op","op":"gt","args":[{"kind":"get","path":["rate"],"members":"required","target":{"kind":"ref","name":"liveItem1"}},{"kind":"lit","value":0}]}}]}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void all_reads_the_predicate_for_every_item()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Ready.Set(pump.Panel.Channels.All(channel => channel.Rate.Gt(0)));
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["ready"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"call","path":["every"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"op","op":"gt","args":[{"kind":"get","path":["rate"],"members":"required","target":{"kind":"ref","name":"liveItem1"}},{"kind":"lit","value":0}]}}]}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void select_keeps_non_nullable_items_present()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Readings.Set(pump.Panel.Channels.Select(channel => channel.Rate));
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["readings"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"call","path":["map"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"op","op":"nonNull","args":[{"kind":"get","path":["rate"],"members":"required","target":{"kind":"ref","name":"liveItem1"}}]}}]}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void select_preserves_a_selected_fields_own_number_form()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Readings.Set(pump.Panel.Channels.Select(channel => channel.Prescription.Read(prescription => prescription.Refills)));
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["readings"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"as","expr":{"kind":"call","path":["map"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"op","op":"nonNull","args":[{"kind":"data","path":[{"name":"refills","requires":[],"absent":{"kind":"start","value":"2"}}],"target":{"kind":"get","path":["prescription"],"members":"required","target":{"kind":"ref","name":"liveItem1"}}}]}}]},"shape":{"kind":"array","item":{"kind":"number","of":"integer","text":"invariant","overflow":"nothing"}}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void select_allows_a_nullable_selected_value()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Label.Set(pump.Panel.Channels.Select(channel => channel.Label).Find(label => label == "Ready"));
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"call","path":["find"],"members":"requiredIfHeld","target":{"kind":"call","path":["map"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"get","path":["label"],"members":"optionalMember","target":{"kind":"ref","name":"liveItem1"}}}]},"args":[{"kind":"fn","params":["item1"],"body":{"kind":"op","op":"eq","args":[{"kind":"ref","name":"item1"},{"kind":"lit","value":"Ready"}]}}]}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void find_reaches_a_strict_item()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Rate.Set(pump.Panel.Channels.Find(channel => channel.Rate.Gt(0)).Rate);
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["rate"],"members":"required","target":{"kind":"call","path":["find"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"op","op":"gt","args":[{"kind":"get","path":["rate"],"members":"required","target":{"kind":"ref","name":"liveItem1"}},{"kind":"lit","value":0}]}}]}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void an_optional_member_of_find_still_requires_its_item()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Label.Set(pump.Panel.Channels.Find(channel => channel.Rate.Gt(0)).Label);
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["label"],"members":"optionalMember","target":{"kind":"call","path":["find"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"op","op":"gt","args":[{"kind":"get","path":["rate"],"members":"required","target":{"kind":"ref","name":"liveItem1"}},{"kind":"lit","value":0}]}}]}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void find_with_a_field_allows_no_match_but_requires_the_field()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Rate.Set(pump.Panel.Channels.Find(channel => channel.Rate.Gt(0), channel => channel.Rate));
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["rate"],"members":"requiredIfHeld","target":{"kind":"call","path":["find"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"op","op":"gt","args":[{"kind":"get","path":["rate"],"members":"required","target":{"kind":"ref","name":"liveItem1"}},{"kind":"lit","value":0}]}}]}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void find_with_an_optional_field_allows_both_absences()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Label.Set(pump.Panel.Channels.Find(channel => channel.Rate.Gt(0), channel => channel.Label));
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["label"],"members":"optional","target":{"kind":"call","path":["find"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"op","op":"gt","args":[{"kind":"get","path":["rate"],"members":"required","target":{"kind":"ref","name":"liveItem1"}},{"kind":"lit","value":0}]}}]}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_found_items_method_runs_as_a_step()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Panel.Channels.Find(channel => channel.Rate.Gt(0)).Stop();
        }));
        const string expected = OnLoad +
            """{"kind":"do","await":false,"expr":{"kind":"call","path":["stop"],"members":"required","target":{"kind":"call","path":["find"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"op","op":"gt","args":[{"kind":"get","path":["rate"],"members":"required","target":{"kind":"ref","name":"liveItem1"}},{"kind":"lit","value":0}]}}]},"args":[]}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_found_items_result_keeps_the_call_at_its_line()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Ready.Set(pump.Panel.Channels.Find(channel => channel.Rate.Gt(0)).IsRunning());
        }));
        const string expected = OnLoad +
            """{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["isRunning"],"members":"required","target":{"kind":"call","path":["find"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"op","op":"gt","args":[{"kind":"get","path":["rate"],"members":"required","target":{"kind":"ref","name":"liveItem1"}},{"kind":"lit","value":0}]}}]},"args":[]}},{"kind":"set","path":["ready"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"ref","name":"result1"}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void at_reaches_an_index_and_a_strict_item()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Rate.Set(pump.Panel.Channels.At(0).Rate);
            pump.Panel.Channels.At(2).Rate.Set(40);
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["rate"],"members":"required","target":{"kind":"get","path":[0],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}}}}},{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"get","path":[2],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}}},"value":{"kind":"lit","value":40}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void backupchannels_operations_allow_an_absent_list()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Rate.Set(pump.Panel.BackupChannels.Count());
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["length"],"members":"requiredIfHeld","target":{"kind":"get","path":["backupChannels"],"members":"optionalMember","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void detachedchannels_operations_allow_an_absent_list()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Rate.Set(pump.Panel.DetachedChannels.Count());
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["length"],"members":"requiredIfHeld","target":{"kind":"get","path":["detachedChannels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void sparechannels_operations_allow_an_absent_list()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Rate.Set(pump.Panel.SpareChannels.Count());
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["length"],"members":"requiredIfHeld","target":{"kind":"get","path":["spare-channels"],"members":"optionalMember","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void select_reads_the_object_every_item_holds_through_its_declaration()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var displays = pump.Panel.Channels.Select(channel => channel.Display);
            pump.Panel.Caption.Set(displays.At(0).Caption);
            pump.Captions.Set(displays.Select(display => display.Caption));
            pump.Panel.Caption.Set(displays.Find(display => display.Caption.Eq("Ready")).Caption);
            pump.Panel.Caption.Set(displays.Where(display => display.Caption.Eq("Ready")).At(0).Caption);
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["caption"],"members":"required","content":false,"target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}},"value":{"kind":"get","path":["caption"],"members":"required","target":{"kind":"get","path":[0],"members":"requiredIfHeld","target":{"kind":"call","path":["map"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"get","path":["display"],"members":"required","target":{"kind":"ref","name":"liveItem1"}}}]}}}},{"kind":"set","path":["captions"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"call","path":["map"],"members":"requiredIfHeld","target":{"kind":"call","path":["map"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"get","path":["display"],"members":"required","target":{"kind":"ref","name":"liveItem1"}}}]},"args":[{"kind":"fn","params":["liveItem2"],"body":{"kind":"get","path":["caption"],"members":"required","target":{"kind":"ref","name":"liveItem2"}}}]}},{"kind":"set","path":["caption"],"members":"required","content":false,"target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}},"value":{"kind":"get","path":["caption"],"members":"required","target":{"kind":"call","path":["find"],"members":"requiredIfHeld","target":{"kind":"call","path":["map"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"get","path":["display"],"members":"required","target":{"kind":"ref","name":"liveItem1"}}}]},"args":[{"kind":"fn","params":["liveItem3"],"body":{"kind":"op","op":"eq","args":[{"kind":"get","path":["caption"],"members":"required","target":{"kind":"ref","name":"liveItem3"}},{"kind":"lit","value":"Ready"}]}}]}}},{"kind":"set","path":["caption"],"members":"required","content":false,"target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}},"value":{"kind":"get","path":["caption"],"members":"required","target":{"kind":"get","path":[0],"members":"requiredIfHeld","target":{"kind":"call","path":["filter"],"members":"requiredIfHeld","target":{"kind":"call","path":["map"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"get","path":["display"],"members":"required","target":{"kind":"ref","name":"liveItem1"}}}]},"args":[{"kind":"fn","params":["liveItem4"],"body":{"kind":"op","op":"eq","args":[{"kind":"get","path":["caption"],"members":"required","target":{"kind":"ref","name":"liveItem4"}},{"kind":"lit","value":"Ready"}]}}]}}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void select_of_a_nullable_object_reads_its_members_as_nothing_where_an_item_holds_none()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var displays = pump.Panel.Channels.Select(channel => channel.DetachedDisplay);
            pump.Panel.Caption.Set(displays.At(0).Caption);
            pump.Captions.Set(displays.Select(display => display.Caption));
            pump.Panel.Caption.Set(displays.Find(display => display.Caption.Eq("Ready")).Caption);
            pump.Panel.Caption.Set(displays.Where(display => display.Caption.Eq("Ready")).At(0).Caption);
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["caption"],"members":"required","content":false,"target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}},"value":{"kind":"get","path":["caption"],"members":"requiredIfHeld","target":{"kind":"get","path":[0],"members":"requiredIfHeld","target":{"kind":"call","path":["map"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"get","path":["detachedDisplay"],"members":"required","target":{"kind":"ref","name":"liveItem1"}}}]}}}},{"kind":"set","path":["captions"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"call","path":["map"],"members":"requiredIfHeld","target":{"kind":"call","path":["map"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"get","path":["detachedDisplay"],"members":"required","target":{"kind":"ref","name":"liveItem1"}}}]},"args":[{"kind":"fn","params":["liveItem2"],"body":{"kind":"get","path":["caption"],"members":"requiredIfHeld","target":{"kind":"ref","name":"liveItem2"}}}]}},{"kind":"set","path":["caption"],"members":"required","content":false,"target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}},"value":{"kind":"get","path":["caption"],"members":"requiredIfHeld","target":{"kind":"call","path":["find"],"members":"requiredIfHeld","target":{"kind":"call","path":["map"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"get","path":["detachedDisplay"],"members":"required","target":{"kind":"ref","name":"liveItem1"}}}]},"args":[{"kind":"fn","params":["liveItem3"],"body":{"kind":"op","op":"eq","args":[{"kind":"get","path":["caption"],"members":"requiredIfHeld","target":{"kind":"ref","name":"liveItem3"}},{"kind":"lit","value":"Ready"}]}}]}}},{"kind":"set","path":["caption"],"members":"required","content":false,"target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}},"value":{"kind":"get","path":["caption"],"members":"requiredIfHeld","target":{"kind":"get","path":[0],"members":"requiredIfHeld","target":{"kind":"call","path":["filter"],"members":"requiredIfHeld","target":{"kind":"call","path":["map"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"get","path":["detachedDisplay"],"members":"required","target":{"kind":"ref","name":"liveItem1"}}}]},"args":[{"kind":"fn","params":["liveItem4"],"body":{"kind":"op","op":"eq","args":[{"kind":"get","path":["caption"],"members":"requiredIfHeld","target":{"kind":"ref","name":"liveItem4"}},{"kind":"lit","value":"Ready"}]}}]}}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }
}
