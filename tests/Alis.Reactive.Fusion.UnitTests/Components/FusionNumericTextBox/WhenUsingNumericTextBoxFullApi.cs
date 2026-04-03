using Alis.Reactive.Builders;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

/// <summary>
/// Tests the full JS API surface of FusionNumericTextBox:
///   Events:   Focus, Blur (void payload)
///   Methods:  FocusOut, Increment, Decrement (void, no args)
///   Prop reads:  Value() → ComponentValueExpression&lt;decimal&gt;
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
    public Task Value_returns_component_value_expression()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var source = p.Component<FusionNumericTextBox>(m => m.Amount).Value();
            Assert.That(source, Is.TypeOf<ComponentValueExpression<decimal>>());
            Assert.That(source.CoercionType, Is.EqualTo("number"));
            p.Element("echo").SetText(source);
        });

        var json = plan.Render();
        Assert.That(json, Does.Contain("Alis_Reactive_Fusion_UnitTests_FusionTestModel__Amount"));
        Assert.That(json, Does.Contain("value"));
        AssertSchemaValid(json);
        return VerifyJson(json);
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
    public void Focus_event_contract_has_correct_js_event()
    {
        var reactiveEvent = FusionNumericTextBoxEvents.Instance.Focus;
        Assert.That(reactiveEvent.EventName, Is.EqualTo("focus"));
    }

    [Test]
    public void Focus_event_contract_provides_args_instance()
    {
        var reactiveEvent = FusionNumericTextBoxEvents.Instance.Focus;
        Assert.That(reactiveEvent.Payload, Is.Not.Null);
        Assert.That(reactiveEvent.Payload, Is.TypeOf<FusionNumericTextBoxFocusArgs>());
    }

    [Test]
    public void Blur_event_contract_has_correct_js_event()
    {
        var reactiveEvent = FusionNumericTextBoxEvents.Instance.Blur;
        Assert.That(reactiveEvent.EventName, Is.EqualTo("blur"));
    }

    [Test]
    public void Blur_event_contract_provides_args_instance()
    {
        var reactiveEvent = FusionNumericTextBoxEvents.Instance.Blur;
        Assert.That(reactiveEvent.Payload, Is.Not.Null);
        Assert.That(reactiveEvent.Payload, Is.TypeOf<FusionNumericTextBoxBlurArgs>());
    }

    [Test]
    public Task Focus_event_wires_component_trigger()
    {
        var plan = CreatePlan();
        WireObjectEvent(
            plan,
            "Amount",
            "fusion",
            "Amount",
            "value",
            FusionNumericTextBoxEvents.Instance.Focus.EventName,
            pb => pb.Element("status").SetText("Focused"));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Blur_event_wires_component_trigger()
    {
        var plan = CreatePlan();
        WireObjectEvent(
            plan,
            "Amount",
            "fusion",
            "Amount",
            "value",
            FusionNumericTextBoxEvents.Instance.Blur.EventName,
            pb => pb.Element("status").SetText("Blurred"));

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
