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
public sealed class WhenALiveLambdaReadsAnItem
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";
    private const string OnLoad =
        """{"version":7,"planId":"Alis.Reactive.ModuleTests.Declarations.Infusion","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"$document":{"kind":"plugin","name":"alis.document","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}},"pump":{"kind":"element","id":"pump","vendor":"native"}},"handlers":[{"on":"$document","path":[],"event":"ready","payload":[],"members":"required","policy":"overlap","do":[""";
    private const string Ends = "]}]}";

    [Test]
    public void running_channels_are_called_once_per_item_without_a_statement()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Rate.Set(pump.Panel.Channels.Count(channel => channel.IsRunning()));
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"get","path":["length"],"members":"requiredIfHeld","target":{"kind":"call","path":["filter"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"call","path":["isRunning"],"members":"required","target":{"kind":"ref","name":"liveItem1"},"args":[]}}]}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_repeated_result_is_bound_once_per_item()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Ready.Set(pump.Panel.Channels.Any(channel =>
            {
                var measured = channel.Measure();
                return measured.Gt(0).And(measured.Lt(100));
            }));
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["ready"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"call","path":["some"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"call","path":[],"members":"required","target":{"kind":"fn","params":["result1"],"body":{"kind":"op","op":"and","args":[{"kind":"op","op":"gt","args":[{"kind":"ref","name":"result1"},{"kind":"lit","value":0}]},{"kind":"op","op":"lt","args":[{"kind":"ref","name":"result1"},{"kind":"lit","value":100}]}]}},"args":[{"kind":"call","path":["measure"],"members":"required","target":{"kind":"ref","name":"liveItem1"},"args":[]}]}}]}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void one_read_in_a_later_term_keeps_the_call_in_that_term()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Ready.Set(pump.Panel.Channels.Any(channel => channel.Rate.Gt(0).And(channel.IsRunning())));
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["ready"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"call","path":["some"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"op","op":"and","args":[{"kind":"op","op":"gt","args":[{"kind":"get","path":["rate"],"members":"required","target":{"kind":"ref","name":"liveItem1"}},{"kind":"lit","value":0}]},{"kind":"call","path":["isRunning"],"members":"required","target":{"kind":"ref","name":"liveItem1"},"args":[]}]}}]}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_call_with_an_argument_keeps_its_call_in_the_lambda()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Readings.Set(pump.Panel.Channels.Select(channel => channel.Echo(channel.Rate)));
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["readings"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"call","path":["map"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"op","op":"nonNull","args":[{"kind":"call","path":["echo"],"members":"required","target":{"kind":"ref","name":"liveItem1"},"args":[{"kind":"get","path":["rate"],"members":"required","target":{"kind":"ref","name":"liveItem1"}}]}]}}]}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void nested_live_lambdas_keep_the_outer_item_visible()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Ready.Set(pump.Panel.Channels.Any(channel => channel.Channels.Any(child => child.Rate.Eq(channel.Rate))));
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["ready"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"call","path":["some"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"call","path":["some"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"ref","name":"liveItem1"}},"args":[{"kind":"fn","params":["liveItem2"],"body":{"kind":"op","op":"eq","args":[{"kind":"get","path":["rate"],"members":"required","target":{"kind":"ref","name":"liveItem2"}},{"kind":"get","path":["rate"],"members":"required","target":{"kind":"ref","name":"liveItem1"}}]}}]}}]}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_live_item_and_a_data_item_have_different_names()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Ready.Set(pump.Panel.Channels.Any(channel => channel.Prescriptions.Any(prescription => prescription.MaxDailyDoses > 0)));
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["ready"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"call","path":["some"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"call","path":["some"],"members":"requiredIfHeld","target":{"kind":"get","path":["prescriptions"],"members":"required","target":{"kind":"ref","name":"liveItem1"}},"args":[{"kind":"fn","params":["item1"],"body":{"kind":"op","op":"gt","args":[{"kind":"data","path":[{"name":"maxDailyDoses","requires":[],"absent":{"kind":"start","value":4}}],"target":{"kind":"ref","name":"item1"}},{"kind":"lit","value":0}]}}]}}]}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void each_live_lambda_gets_a_name_in_the_plan()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Ready.Set(pump.Panel.Channels.Any());
            pump.Ready.Set(pump.Panel.Channels.All(channel => channel.IsRunning()));
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["ready"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"call","path":["some"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"lit","value":true}}]}},{"kind":"set","path":["ready"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"call","path":["every"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem2"],"body":{"kind":"call","path":["isRunning"],"members":"required","target":{"kind":"ref","name":"liveItem2"},"args":[]}}]}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void an_inner_lambda_restores_the_outer_result_scope()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Ready.Set(pump.Panel.Channels.Any(channel =>
            {
                var childRuns = channel.Channels.Any(child => child.IsRunning());
                return childRuns.And(channel.IsRunning());
            }));
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["ready"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"call","path":["some"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"op","op":"and","args":[{"kind":"call","path":["some"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"ref","name":"liveItem1"}},"args":[{"kind":"fn","params":["liveItem2"],"body":{"kind":"call","path":["isRunning"],"members":"required","target":{"kind":"ref","name":"liveItem2"},"args":[]}}]},{"kind":"call","path":["isRunning"],"members":"required","target":{"kind":"ref","name":"liveItem1"},"args":[]}]}}]}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_pipeline_result_captured_by_a_lambda_keeps_its_line()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var total = pump.Total();
            pump.Ready.Set(pump.Panel.Channels.Any(channel => channel.Rate.Eq(total)));
        }));
        const string expected = OnLoad +
            """{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["total"],"members":"required","target":{"kind":"obj","id":"pump"},"args":[]}},{"kind":"set","path":["ready"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"call","path":["some"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"op","op":"eq","args":[{"kind":"get","path":["rate"],"members":"required","target":{"kind":"ref","name":"liveItem1"}},{"kind":"ref","name":"result1"}]}}]}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void an_async_result_called_before_the_lambda_is_readable_inside_it()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var total = pump.TotalAsync();
            pump.Ready.Set(pump.Panel.Channels.Any(channel => channel.Rate.Eq(total)));
        }));
        const string expected = OnLoad +
            """{"kind":"let","name":"result1","await":true,"binds":"line","expr":{"kind":"call","path":["totalAsync"],"members":"required","target":{"kind":"obj","id":"pump"},"args":[]}},{"kind":"set","path":["ready"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"call","path":["some"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"op","op":"eq","args":[{"kind":"get","path":["rate"],"members":"required","target":{"kind":"ref","name":"liveItem1"}},{"kind":"ref","name":"result1"}]}}]}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_returned_object_inside_a_lambda_is_the_items_own_call()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Ready.Set(pump.Panel.Channels.Any(channel => channel.OpenPanel().Caption.Eq("Ready")));
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["ready"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"call","path":["some"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"op","op":"eq","args":[{"kind":"get","path":["caption"],"members":"required","target":{"kind":"call","path":["openPanel"],"members":"required","target":{"kind":"ref","name":"liveItem1"},"args":[]}},{"kind":"lit","value":"Ready"}]}}]}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void an_outer_result_captured_by_an_inner_lambda_is_called_per_outer_item()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Ready.Set(pump.Panel.Channels.Any(channel =>
            {
                var rate = channel.Measure();
                return channel.Channels.Any(child => child.Rate.Eq(rate));
            }));
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["ready"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"call","path":["some"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"call","path":[],"members":"required","target":{"kind":"fn","params":["result1"],"body":{"kind":"call","path":["some"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"ref","name":"liveItem1"}},"args":[{"kind":"fn","params":["liveItem2"],"body":{"kind":"op","op":"eq","args":[{"kind":"get","path":["rate"],"members":"required","target":{"kind":"ref","name":"liveItem2"}},{"kind":"ref","name":"result1"}]}}]}},"args":[{"kind":"call","path":["measure"],"members":"required","target":{"kind":"ref","name":"liveItem1"},"args":[]}]}}]}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_dependent_result_used_twice_keeps_both_calls_once()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Ready.Set(pump.Panel.Channels.Any(channel =>
            {
                var measured = channel.Measure();
                var echoed = channel.Echo(measured);
                return measured.Eq(echoed).And(echoed.Gt(0));
            }));
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["ready"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"call","path":["some"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"call","path":[],"members":"required","target":{"kind":"fn","params":["result1"],"body":{"kind":"call","path":[],"members":"required","target":{"kind":"fn","params":["result2"],"body":{"kind":"op","op":"and","args":[{"kind":"op","op":"eq","args":[{"kind":"ref","name":"result1"},{"kind":"ref","name":"result2"}]},{"kind":"op","op":"gt","args":[{"kind":"ref","name":"result2"},{"kind":"lit","value":0}]}]}},"args":[{"kind":"call","path":["echo"],"members":"required","target":{"kind":"ref","name":"liveItem1"},"args":[{"kind":"ref","name":"result1"}]}]}},"args":[{"kind":"call","path":["measure"],"members":"required","target":{"kind":"ref","name":"liveItem1"},"args":[]}]}}]}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void the_delegate_runs_once_when_the_view_renders()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        var calls = 0;
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Ready.Set(pump.Panel.Channels.Any(channel =>
            {
                calls++;
                return channel.IsRunning();
            }));
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["ready"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"call","path":["some"],"members":"requiredIfHeld","target":{"kind":"get","path":["channels"],"members":"required","target":{"kind":"get","path":["panel"],"members":"required","target":{"kind":"obj","id":"pump"}}},"args":[{"kind":"fn","params":["liveItem1"],"body":{"kind":"call","path":["isRunning"],"members":"required","target":{"kind":"ref","name":"liveItem1"},"args":[]}}]}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
        Assert.That(calls, Is.EqualTo(1));
    }

    [Test]
    public void a_throwing_delegate_restores_the_pipeline_scope()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var channels = pump.Panel.Channels;
            Assert.That(() => channels.Any(channel => throw new InvalidOperationException("The view could not choose a channel.")),
                Throws.InvalidOperationException.With.Message.EqualTo("The view could not choose a channel."));
            pump.Rate.Set(pump.Total());
        }));
        const string expected = OnLoad +
            """{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["total"],"members":"required","target":{"kind":"obj","id":"pump"},"args":[]}},{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"ref","name":"result1"}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }
}
