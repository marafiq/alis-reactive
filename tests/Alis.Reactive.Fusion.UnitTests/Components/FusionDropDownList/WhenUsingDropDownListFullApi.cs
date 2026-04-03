using Alis.Reactive.Builders;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

/// <summary>
/// Tests the full JS API surface of FusionDropDownList:
///   Events:   Focus, Blur (void payload)
///   Methods:  FocusOut, ShowPopup, HidePopup (void, no args)
///   Prop reads:  Value() → ComponentValueExpression&lt;string&gt;
///   Prop writes: SetValue(string?), SetText(string)
/// </summary>
[TestFixture]
public class WhenUsingDropDownListFullApi : FusionTestBase
{
    // ── Methods ──

    [Test]
    public Task FocusOut_produces_ej2_call()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<FusionDropDownList>(m => m.Status).FocusOut());
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
            var source = p.Component<FusionDropDownList>(m => m.Status).Value();
            Assert.That(source, Is.TypeOf<ComponentValueExpression<string>>());
            Assert.That(source.CoercionType, Is.EqualTo("string"));
            p.Element("echo").SetText(source);
        });

        var json = plan.Render();
        Assert.That(json, Does.Contain("Alis_Reactive_Fusion_UnitTests_FusionTestModel__Status"));
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
            p.When(p.Component<FusionDropDownList>(m => m.Status).Value()).Eq("US")
                .Then(then => then.Element("status").SetText("Selected US"))
                .Else(else_ => else_.Element("status").SetText("Other"));
        });
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ── Events ──

    [Test]
    public void Focus_event_contract_has_correct_js_event()
    {
        var reactiveEvent = FusionDropDownListEvents.Instance.Focus;
        Assert.That(reactiveEvent.EventName, Is.EqualTo("focus"));
    }

    [Test]
    public void Focus_event_contract_provides_args_instance()
    {
        var reactiveEvent = FusionDropDownListEvents.Instance.Focus;
        Assert.That(reactiveEvent.Payload, Is.Not.Null);
        Assert.That(reactiveEvent.Payload, Is.TypeOf<FusionDropDownListFocusArgs>());
    }

    [Test]
    public void Blur_event_contract_has_correct_js_event()
    {
        var reactiveEvent = FusionDropDownListEvents.Instance.Blur;
        Assert.That(reactiveEvent.EventName, Is.EqualTo("blur"));
    }

    [Test]
    public void Blur_event_contract_provides_args_instance()
    {
        var reactiveEvent = FusionDropDownListEvents.Instance.Blur;
        Assert.That(reactiveEvent.Payload, Is.Not.Null);
        Assert.That(reactiveEvent.Payload, Is.TypeOf<FusionDropDownListBlurArgs>());
    }

    [Test]
    public Task Focus_event_wires_component_trigger()
    {
        var plan = CreatePlan();
        WireObjectEvent(
            plan,
            "Status",
            "fusion",
            "Status",
            "value",
            FusionDropDownListEvents.Instance.Focus.EventName,
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
            "Status",
            "fusion",
            "Status",
            "value",
            FusionDropDownListEvents.Instance.Blur.EventName,
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
            p.Component<FusionDropDownList>(m => m.Status).SetValue("US").ShowPopup();
            p.Element("echo").SetText("Initialized");
        });
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
