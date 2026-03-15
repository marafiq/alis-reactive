using Alis.Reactive;
using Alis.Reactive.Builders;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Sources;
using Alis.Reactive.Descriptors.Triggers;
using Alis.Reactive.Fusion.Components;
using static VerifyNUnit.Verifier;

namespace Alis.Reactive.Fusion.UnitTests;

/// <summary>
/// Tests the full JS API surface of FusionNumericTextBox:
///   Events:   Focus, Blur (void payload)
///   Methods:  FocusOut, Increment, Decrement (void, no args)
///   Prop reads:  Value() → TypedComponentSource&lt;decimal&gt;
///   Prop writes: SetMin(decimal)
/// </summary>
[TestFixture]
public class WhenUsingNumericTextBoxFullApi : FusionTestBase
{
    // ── Methods ──

    [Test]
    public Task FocusOut_produces_ej2_call()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<FusionNumericTextBox>(m => m.Amount).FocusOut());
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Increment_produces_ej2_call()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<FusionNumericTextBox>(m => m.Amount).Increment());
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Decrement_produces_ej2_call()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<FusionNumericTextBox>(m => m.Amount).Decrement());
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ── Prop writes ──

    [Test]
    public Task SetMin_produces_ej2_mutation()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<FusionNumericTextBox>(m => m.Amount).SetMin(10));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task SetMin_with_decimal_value()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<FusionNumericTextBox>(m => m.Amount).SetMin(0.5m));
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ── Prop reads ──

    [Test]
    public void Value_returns_typed_component_source()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var source = p.Component<FusionNumericTextBox>(m => m.Amount).Value();
            Assert.That(source, Is.TypeOf<TypedComponentSource<decimal>>());

            var bindSource = source.ToBindSource();
            Assert.That(bindSource, Is.TypeOf<ComponentSource>());

            var cs = (ComponentSource)bindSource;
            Assert.That(cs.ComponentId, Is.EqualTo("Alis_Reactive_Fusion_UnitTests_FusionTestModel__Amount"));
            Assert.That(cs.Vendor, Is.EqualTo("fusion"));
            Assert.That(cs.ReadExpr, Is.EqualTo("value"));
        });
    }

    [Test]
    public Task Value_used_in_condition()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.When(p.Component<FusionNumericTextBox>(m => m.Amount).Value()).Gte(100m)
                .Then(then => then.Element("status").SetText("High"))
                .Else(else_ => else_.Element("status").SetText("Low"));
        });
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ── Events ──

    [Test]
    public void Focus_descriptor_has_correct_js_event()
    {
        var descriptor = FusionNumericTextBoxEvents.Instance.Focus;
        Assert.That(descriptor.JsEvent, Is.EqualTo("focus"));
    }

    [Test]
    public void Focus_descriptor_provides_args_instance()
    {
        var descriptor = FusionNumericTextBoxEvents.Instance.Focus;
        Assert.That(descriptor.Args, Is.Not.Null);
        Assert.That(descriptor.Args, Is.TypeOf<FusionNumericTextBoxFocusArgs>());
    }

    [Test]
    public void Blur_descriptor_has_correct_js_event()
    {
        var descriptor = FusionNumericTextBoxEvents.Instance.Blur;
        Assert.That(descriptor.JsEvent, Is.EqualTo("blur"));
    }

    [Test]
    public void Blur_descriptor_provides_args_instance()
    {
        var descriptor = FusionNumericTextBoxEvents.Instance.Blur;
        Assert.That(descriptor.Args, Is.Not.Null);
        Assert.That(descriptor.Args, Is.TypeOf<FusionNumericTextBoxBlurArgs>());
    }

    [Test]
    public Task Focus_event_wires_component_trigger()
    {
        var plan = CreatePlan();
        var descriptor = FusionNumericTextBoxEvents.Instance.Focus;

        var pb = new PipelineBuilder<FusionTestModel>();
        pb.Element("status").SetText("Focused");

        var trigger = new ComponentEventTrigger("Amount", descriptor.JsEvent, "fusion", "Amount", "value");
        plan.AddEntry(new Entry(trigger, pb.BuildReaction()));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Blur_event_wires_component_trigger()
    {
        var plan = CreatePlan();
        var descriptor = FusionNumericTextBoxEvents.Instance.Blur;

        var pb = new PipelineBuilder<FusionTestModel>();
        pb.Element("status").SetText("Blurred");

        var trigger = new ComponentEventTrigger("Amount", descriptor.JsEvent, "fusion", "Amount", "value");
        plan.AddEntry(new Entry(trigger, pb.BuildReaction()));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ── Method chaining ──

    [Test]
    public Task Methods_chain_with_other_commands()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Component<FusionNumericTextBox>(m => m.Amount).SetMin(0).SetValue(50).Increment();
            p.Element("echo").SetText("Initialized");
        });
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
