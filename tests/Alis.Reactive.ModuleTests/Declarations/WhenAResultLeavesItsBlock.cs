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
public sealed class WhenAResultLeavesItsBlock
{
    private MethodResult<int> _carried = null!;
    private TypedSource<bool> _condition = null!;
    private TypedSource<int> _field = null!;

    [Test]
    public void a_field_cannot_carry_a_result_out_of_its_branch()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pipeline.When(pump.FlowRate.Eq(0)).Then(pipeline => _carried = pump.Total());
            pump.Rate.Set(_carried);
        }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "A method's result is read outside the block that called the method. Call the method in the block that reads its result, or in a block around it."));
    }

    [Test]
    public void a_field_cannot_carry_a_result_into_a_second_handler()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline => _carried = pipeline.Component<InfusionPump>("pump").Total()));
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Component<InfusionPump>("pump").Rate.Set(_carried)));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "A method's result is read outside the block that called the method. Call the method in the block that reads its result, or in a block around it."));
    }

    [Test]
    public void a_condition_cannot_carry_a_result_into_a_nested_handler()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            _condition = pump.Total().Eq(100);
            trigger.DomReady(pipeline => pipeline.When(_condition).Then(pipeline => pump.Rate.Set(999)));
        }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "A method's result is read outside the block that called the method. Call the method in the block that reads its result, or in a block around it."));
    }

    [Test]
    public void another_plans_result_with_the_same_name_does_not_belong_to_this_plan()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline => _condition = pipeline.Component<InfusionPump>("pump").Total().Eq(100)));
        html.RenderPlan(plan);
        var otherHtml = Views.Of(new Infusion("South"));
        var otherPlan = otherHtml.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            otherHtml.On(otherPlan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Total();
            pipeline.When(_condition).Then(pipeline => pump.Rate.Set(999));
        }));
            otherHtml.RenderPlan(otherPlan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "A method's result is read outside the block that called the method. Call the method in the block that reads its result, or in a block around it."));
    }

    [Test]
    public void a_result_name_this_plan_never_bound_is_refused()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline => _condition = pipeline.Component<InfusionPump>("pump").Total().Eq(100)));
        html.RenderPlan(plan);
        var otherHtml = Views.Of(new Infusion("South"));
        var otherPlan = otherHtml.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            otherHtml.On(otherPlan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pipeline.When(_condition).Then(pipeline => pump.Rate.Set(999));
        }));
            otherHtml.RenderPlan(otherPlan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "A method's result is read outside the block that called the method. Call the method in the block that reads its result, or in a block around it."));
    }

    [Test]
    public void a_result_is_refused_as_soon_as_it_is_read_after_its_handler_closes()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline => _carried = pipeline.Component<InfusionPump>("pump").Total()));
        html.RenderPlan(plan);
        Assert.That(() => _carried.Eq(100), Throws.InvalidOperationException.With.Message.EqualTo(
            "A method's result is read outside the block that called the method. Call the method in the block that reads its result, or in a block around it."));
    }

    [Test]
    public void a_results_data_read_cannot_leave_its_branch()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pipeline.When(pump.FlowRate.Eq(0)).Then(pipeline => _field = pump.Summary().Read(summary => summary.MaxDailyDoses));
            pump.Rate.Set(_field);
        }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "A method's result is read outside the block that called the method. Call the method in the block that reads its result, or in a block around it."));
    }
}
