using System;
using Alis.Reactive.Conditions;
using Alis.Reactive.Declarations;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.ValuesAndSources;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Declarations;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenACallbackAnswersItsVendor
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";
    private const string OnLoad =
        """{"version":7,"planId":"Alis.Reactive.ModuleTests.Declarations.Infusion","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"$document":{"kind":"plugin","name":"alis.document","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}},"pump":{"kind":"element","id":"pump","vendor":"native"}},"handlers":[{"on":"$document","path":[],"event":"ready","payload":[],"members":"required","policy":"overlap","do":[""";
    private const string Ends = "]}]}";

    [Test]
    public void a_required_callback_answers_from_the_first_arguments_member()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.ReadingLabel.Set(reading => reading.Label);
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["readingLabel"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"answer","arguments":"answer1","body":{"kind":"get","path":["label"],"members":"required","target":{"kind":"get","path":[0],"members":"required","target":{"kind":"ref","name":"answer1"}}}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void an_optional_callback_keeps_its_declared_name_and_presence()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.BackupLabel.Set(reading => reading.Label);
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["label-format"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"answer","arguments":"answer1","body":{"kind":"get","path":["label"],"members":"required","target":{"kind":"get","path":[0],"members":"required","target":{"kind":"ref","name":"answer1"}}}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_method_receives_the_answer_as_its_argument()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.SortReadings(reading => reading.Rate);
        }));
        const string expected = OnLoad +
            """{"kind":"do","await":false,"expr":{"kind":"call","path":["sortReadings"],"members":"required","target":{"kind":"obj","id":"pump"},"args":[{"kind":"answer","arguments":"answer1","body":{"kind":"get","path":["rate"],"members":"required","target":{"kind":"get","path":[0],"members":"required","target":{"kind":"ref","name":"answer1"}}}}]}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_value_accessor_reads_the_row_at_position_one()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.ReadingLabel.Set(reading => reading.Row.Read(row => row.Drug));
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["readingLabel"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"answer","arguments":"answer1","body":{"kind":"data","path":[{"name":"drug","requires":[],"absent":{"kind":"start","value":"none"}}],"target":{"kind":"get","path":[1],"members":"required","target":{"kind":"ref","name":"answer1"}}}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_method_result_is_called_inside_each_answer()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.AdjustedRate.Set(reading => reading.Measure());
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["adjustedRate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"answer","arguments":"answer1","body":{"kind":"call","path":["measure"],"members":"required","target":{"kind":"get","path":[0],"members":"required","target":{"kind":"ref","name":"answer1"}},"args":[]}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_repeated_result_is_called_once_per_answer()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.CanDeliver.Set(reading =>
            {
                var measured = reading.Measure();
                return measured.Gt(0).And(measured.Lt(100));
            });
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["canDeliver"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"answer","arguments":"answer1","body":{"kind":"call","path":[],"members":"required","target":{"kind":"fn","params":["result1"],"body":{"kind":"op","op":"and","args":[{"kind":"op","op":"gt","args":[{"kind":"ref","name":"result1"},{"kind":"lit","value":0}]},{"kind":"op","op":"lt","args":[{"kind":"ref","name":"result1"},{"kind":"lit","value":100}]}]}},"args":[{"kind":"call","path":["measure"],"members":"required","target":{"kind":"get","path":[0],"members":"required","target":{"kind":"ref","name":"answer1"}},"args":[]}]}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void nested_answers_keep_the_outer_arguments_visible()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.AdjustedRate.Set(reading => pump.SelectRate(child => child.Rate.Eq(reading.Rate).Choose(1, 0)));
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["adjustedRate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"answer","arguments":"answer1","body":{"kind":"call","path":["selectRate"],"members":"required","target":{"kind":"obj","id":"pump"},"args":[{"kind":"answer","arguments":"answer2","body":{"kind":"op","op":"cond","args":[{"kind":"op","op":"eq","args":[{"kind":"get","path":["rate"],"members":"required","target":{"kind":"get","path":[0],"members":"required","target":{"kind":"ref","name":"answer2"}}},{"kind":"get","path":["rate"],"members":"required","target":{"kind":"get","path":[0],"members":"required","target":{"kind":"ref","name":"answer1"}}}]},{"kind":"lit","value":1},{"kind":"lit","value":0}]}}]}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void an_inner_answer_restores_the_outer_result_scope()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.CanDeliver.Set(reading =>
            {
                var selected = pump.SelectRate(child => child.Measure());
                return selected.Eq(reading.Measure());
            });
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["canDeliver"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"answer","arguments":"answer1","body":{"kind":"op","op":"eq","args":[{"kind":"call","path":["selectRate"],"members":"required","target":{"kind":"obj","id":"pump"},"args":[{"kind":"answer","arguments":"answer2","body":{"kind":"call","path":["measure"],"members":"required","target":{"kind":"get","path":[0],"members":"required","target":{"kind":"ref","name":"answer2"}},"args":[]}}]},{"kind":"call","path":["measure"],"members":"required","target":{"kind":"get","path":[0],"members":"required","target":{"kind":"ref","name":"answer1"}},"args":[]}]}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void an_outer_result_captured_by_an_inner_answer_runs_in_the_outer_answer()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.AdjustedRate.Set(reading =>
            {
                var measured = reading.Measure();
                return pump.SelectRate(child => child.Rate.Eq(measured).Choose(1, 0));
            });
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["adjustedRate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"answer","arguments":"answer1","body":{"kind":"call","path":[],"members":"required","target":{"kind":"fn","params":["result1"],"body":{"kind":"call","path":["selectRate"],"members":"required","target":{"kind":"obj","id":"pump"},"args":[{"kind":"answer","arguments":"answer2","body":{"kind":"op","op":"cond","args":[{"kind":"op","op":"eq","args":[{"kind":"get","path":["rate"],"members":"required","target":{"kind":"get","path":[0],"members":"required","target":{"kind":"ref","name":"answer2"}}},{"kind":"ref","name":"result1"}]},{"kind":"lit","value":1},{"kind":"lit","value":0}]}}]}},"args":[{"kind":"call","path":["measure"],"members":"required","target":{"kind":"get","path":[0],"members":"required","target":{"kind":"ref","name":"answer1"}},"args":[]}]}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_handlers_result_in_a_later_term_of_the_answer_keeps_its_line()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var total = pump.Total();
            pump.CanDeliver.Set(reading => reading.Rate.Gt(0).And(total.Gt(0)));
        }));
        const string expected = OnLoad +
            """{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["total"],"members":"required","target":{"kind":"obj","id":"pump"},"args":[]}},{"kind":"set","path":["canDeliver"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"answer","arguments":"answer1","body":{"kind":"op","op":"and","args":[{"kind":"op","op":"gt","args":[{"kind":"get","path":["rate"],"members":"required","target":{"kind":"get","path":[0],"members":"required","target":{"kind":"ref","name":"answer1"}}},{"kind":"lit","value":0}]},{"kind":"op","op":"gt","args":[{"kind":"ref","name":"result1"},{"kind":"lit","value":0}]}]}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void an_async_result_awaited_before_the_answer_can_be_read_inside_it()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var total = pump.TotalAsync();
            pump.CanDeliver.Set(reading => reading.Rate.Eq(total));
        }));
        const string expected = OnLoad +
            """{"kind":"let","name":"result1","await":true,"binds":"line","expr":{"kind":"call","path":["totalAsync"],"members":"required","target":{"kind":"obj","id":"pump"},"args":[]}},{"kind":"set","path":["canDeliver"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"answer","arguments":"answer1","body":{"kind":"op","op":"eq","args":[{"kind":"get","path":["rate"],"members":"required","target":{"kind":"get","path":[0],"members":"required","target":{"kind":"ref","name":"answer1"}}},{"kind":"ref","name":"result1"}]}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_page_method_is_read_inside_the_answer()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.AdjustedRate.Set(reading => pump.Total());
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["adjustedRate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"answer","arguments":"answer1","body":{"kind":"call","path":["total"],"members":"required","target":{"kind":"obj","id":"pump"},"args":[]}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_returned_objects_member_is_read_inside_the_answer()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.CanDeliver.Set(reading => pump.OpenPanel().Caption.Eq("Ready"));
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["canDeliver"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"answer","arguments":"answer1","body":{"kind":"op","op":"eq","args":[{"kind":"get","path":["caption"],"members":"required","target":{"kind":"call","path":["openPanel"],"members":"required","target":{"kind":"obj","id":"pump"},"args":[]}},{"kind":"lit","value":"Ready"}]}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void successive_answers_have_distinct_names_and_restore_the_pipeline()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.ReadingLabel.Set(reading => reading.Label);
            pump.AdjustedRate.Set(reading => reading.Rate);
            pump.Rate.Set(pump.Total());
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["readingLabel"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"answer","arguments":"answer1","body":{"kind":"get","path":["label"],"members":"required","target":{"kind":"get","path":[0],"members":"required","target":{"kind":"ref","name":"answer1"}}}}},{"kind":"set","path":["adjustedRate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"answer","arguments":"answer2","body":{"kind":"get","path":["rate"],"members":"required","target":{"kind":"get","path":[0],"members":"required","target":{"kind":"ref","name":"answer2"}}}}},{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["total"],"members":"required","target":{"kind":"obj","id":"pump"},"args":[]}},{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"ref","name":"result1"}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void the_answers_delegate_runs_once_where_the_view_renders()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        var calls = 0;
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.AdjustedRate.Set(reading =>
            {
                calls++;
                return reading.Rate;
            });
        }));
        const string expected = OnLoad +
            """{"kind":"set","path":["adjustedRate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"answer","arguments":"answer1","body":{"kind":"get","path":["rate"],"members":"required","target":{"kind":"get","path":[0],"members":"required","target":{"kind":"ref","name":"answer1"}}}}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
        Assert.That(calls, Is.EqualTo(1));
    }

    [Test]
    public void a_throwing_answer_restores_the_pipeline_scope()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            Assert.That(() => pump.AdjustedRate.Set(reading => throw new InvalidOperationException("The view could not choose a rate.")),
                Throws.InvalidOperationException.With.Message.EqualTo("The view could not choose a rate."));
            pump.Rate.Set(pump.Total());
        }));
        const string expected = OnLoad +
            """{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["total"],"members":"required","target":{"kind":"obj","id":"pump"},"args":[]}},{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"ref","name":"result1"}}""" + Ends;
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_property_write_inside_an_answer_is_refused()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                Action aHelpersStep = () => pump.Rate.Set(20);
                pump.AdjustedRate.Set(reading =>
                {
                    aHelpersStep();
                    return reading.Rate;
                });
            }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "A step is written inside a callback's answer, which answers its vendor with one value. Write the step in a pipeline."));
    }

    [Test]
    public void a_void_method_inside_an_answer_is_refused()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                Action aHelpersStep = () => pump.Start();
                pump.AdjustedRate.Set(reading =>
                {
                    aHelpersStep();
                    return reading.Rate;
                });
            }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "A step is written inside a callback's answer, which answers its vendor with one value. Write the step in a pipeline."));
    }

    [Test]
    public void a_positioned_method_inside_an_answer_is_refused()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                Action<PumpReading> aHelpersStep = held => held.Continue();
                pump.AdjustedRate.Set(reading =>
                {
                    aHelpersStep(reading);
                    return reading.Rate;
                });
            }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "A step is written inside a callback's answer, which answers its vendor with one value. Write the step in a pipeline."));
    }

    [Test]
    public void a_positioned_method_with_arguments_inside_an_answer_is_refused()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                Action<PumpReading> aHelpersStep = held => held.ContinueWith(held.Rate);
                pump.AdjustedRate.Set(reading =>
                {
                    aHelpersStep(reading);
                    return reading.Rate;
                });
            }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "A step is written inside a callback's answer, which answers its vendor with one value. Write the step in a pipeline."));
    }

    [Test]
    public void an_async_void_method_inside_an_answer_is_refused()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                Action aHelpersStep = () => pump.FlushAsync();
                pump.AdjustedRate.Set(reading =>
                {
                    aHelpersStep();
                    return reading.Rate;
                });
            }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "A callback answers its vendor at once, so its answer cannot wait for an async method or a request."));
    }

    [Test]
    public void an_async_void_method_with_arguments_inside_an_answer_is_refused()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                Action<PumpReading> aHelpersStep = held => pump.DeliverAsync(held.Rate);
                pump.AdjustedRate.Set(reading =>
                {
                    aHelpersStep(reading);
                    return reading.Rate;
                });
            }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "A callback answers its vendor at once, so its answer cannot wait for an async method or a request."));
    }

    [Test]
    public void an_async_result_inside_an_answer_is_refused()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                Action aHelpersStep = () => pump.TotalAsync();
                pump.AdjustedRate.Set(reading =>
                {
                    aHelpersStep();
                    return reading.Rate;
                });
            }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "A callback answers its vendor at once, so its answer cannot wait for an async method or a request."));
    }

    [Test]
    public void an_async_result_with_arguments_inside_an_answer_is_refused()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                Action<PumpReading> aHelpersStep = held => pump.EstimateAsync(held.Rate);
                pump.AdjustedRate.Set(reading =>
                {
                    aHelpersStep(reading);
                    return reading.Rate;
                });
            }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "A callback answers its vendor at once, so its answer cannot wait for an async method or a request."));
    }

    [Test]
    public void an_answers_result_cannot_be_read_after_the_answer()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        MethodResult<int> measured = null!;
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                pump.AdjustedRate.Set(reading =>
                {
                    measured = reading.Measure();
                    return measured;
                });
                pump.Rate.Set(measured);
            }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "A method's result is read outside the block that called the method. Call the method in the block that reads its result, or in a block around it."));
    }
}
