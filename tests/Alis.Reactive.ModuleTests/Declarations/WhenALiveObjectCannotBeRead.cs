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
public sealed class WhenALiveObjectCannotBeRead
{
    private PumpPanel _panel = null!;
    private LiveObjects<PumpChannel> _channels = null!;
    private MethodResult<int> _measured = null!;
    private TypedSource<bool> _condition = null!;

    [Test]
    public void a_negative_position_is_refused()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                pump.Panel.Channels.At(-1);
            }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "A live list's position must be a non-negative integer; the first item is at position 0."));
    }

    [Test]
    public void a_void_call_inside_a_lambda_is_refused()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                pump.Panel.Channels.Any(channel =>
                {
                    channel.Stop();
                    return channel.Rate.Gt(0);
                });
            }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "A step is written inside a list's lambda, which reads each item and writes nothing. Write the step in the pipeline."));
    }

    [Test]
    public void a_void_call_with_arguments_inside_a_lambda_is_refused()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                pump.Panel.Channels.Any(channel =>
                {
                    channel.StopWith(1);
                    return channel.Rate.Gt(0);
                });
            }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "A step is written inside a list's lambda, which reads each item and writes nothing. Write the step in the pipeline."));
    }

    [Test]
    public void a_set_inside_a_lambda_is_refused()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                pump.Panel.Channels.Any(channel =>
                {
                    channel.Rate.Set(50);
                    return channel.Rate.Gt(0);
                });
            }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "A step is written inside a list's lambda, which reads each item and writes nothing. Write the step in the pipeline."));
    }

    [Test]
    public void a_write_to_a_captured_object_inside_a_lambda_is_refused()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                pump.Panel.Channels.Any(channel =>
                {
                    pump.Bolus.Set(channel.Rate);
                    return channel.Rate.Gt(0);
                });
            }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "A step is written inside a list's lambda, which reads each item and writes nothing. Write the step in the pipeline."));
    }

    [Test]
    public void an_async_result_inside_a_lambda_is_refused()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                pump.Panel.Channels.Any(channel => channel.IsRunningAsync());
            }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "An async method is called inside a list's lambda, which reads every item in one turn and waits for nothing. Call the method in the pipeline and read its result in the lambda."));
    }

    [Test]
    public void an_async_result_with_arguments_inside_a_lambda_is_refused()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                pump.Panel.Channels.Any(channel => channel.CheckAsync(1));
            }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "An async method is called inside a list's lambda, which reads every item in one turn and waits for nothing. Call the method in the pipeline and read its result in the lambda."));
    }

    [Test]
    public void an_async_void_call_inside_a_lambda_is_refused()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                pump.Panel.Channels.Any(channel =>
                {
                    channel.FlushAsync();
                    return channel.Rate.Gt(0);
                });
            }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "An async method is called inside a list's lambda, which reads every item in one turn and waits for nothing. Call the method in the pipeline and read its result in the lambda."));
    }

    [Test]
    public void an_async_void_call_with_arguments_inside_a_lambda_is_refused()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                pump.Panel.Channels.Any(channel =>
                {
                    channel.FlushWithAsync(1);
                    return channel.Rate.Gt(0);
                });
            }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "An async method is called inside a list's lambda, which reads every item in one turn and waits for nothing. Call the method in the pipeline and read its result in the lambda."));
    }

    [Test]
    public void a_returned_object_cannot_leave_its_branch()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                pipeline.When(pump.FlowRate.Eq(0)).Then(pipeline => _panel = pump.OpenPanel());
                pump.Label.Set(_panel.Caption);
            }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "A method's result is read outside the block that called the method. Call the method in the block that reads its result, or in a block around it."));
    }

    [Test]
    public void a_returned_list_cannot_leave_its_branch()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                pipeline.When(pump.FlowRate.Eq(0)).Then(pipeline => _channels = pump.OpenChannels());
                pump.Rate.Set(_channels.Count());
            }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "A method's result is read outside the block that called the method. Call the method in the block that reads its result, or in a block around it."));
    }

    [Test]
    public void a_lambda_result_cannot_leave_its_lambda()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                pump.Rate.Set(pump.Panel.Channels.Count(channel =>
                {
                    _measured = channel.Measure();
                    return _measured.Gt(0);
                }));
                pump.Rate.Set(_measured);
            }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "A method's result is read outside the block that called the method. Call the method in the block that reads its result, or in a block around it."));
    }

    [Test]
    public void a_computed_lambda_result_cannot_leave_its_lambda()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                pump.Ready.Set(pump.Panel.Channels.Any(channel =>
                {
                    _condition = channel.Measure().Gt(0);
                    return _condition;
                }));
                pump.Ready.Set(_condition);
            }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "A method's result is read outside the block that called the method. Call the method in the block that reads its result, or in a block around it."));
    }

    [Test]
    public void a_returned_object_cannot_enter_another_handler()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                _panel = pump.OpenPanel();
                trigger.DomReady(pipeline => pipeline.Component<InfusionPump>("pump").Label.Set(_panel.Caption));
            }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "A method's result is read outside the block that called the method. Call the method in the block that reads its result, or in a block around it."));
    }

    [Test]
    public void a_lambda_parameter_cannot_hide_another_plans_result_with_the_same_name()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline => _condition = pipeline.Component<InfusionPump>("pump").Total().Gt(0)));
        html.RenderPlan(plan);
        var otherHtml = Views.Of(new Infusion("South"));
        var otherPlan = otherHtml.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            otherHtml.On(otherPlan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                pump.Ready.Set(pump.Panel.Channels.Any(channel =>
                {
                    var measured = channel.Measure();
                    return _condition.And(measured.Gt(0)).And(measured.Lt(100));
                }));
            }));
            otherHtml.RenderPlan(otherPlan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "A method's result is read outside the block that called the method. Call the method in the block that reads its result, or in a block around it."));
    }
}
