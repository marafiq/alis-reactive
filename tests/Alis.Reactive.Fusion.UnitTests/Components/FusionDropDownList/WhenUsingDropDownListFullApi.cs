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
/// Tests the full JS API surface of FusionDropDownList:
///   Events:   Focus, Blur (void payload)
///   Methods:  FocusOut, ShowPopup, HidePopup (void, no args)
///   Prop reads:  Value() → TypedComponentSource&lt;string&gt;
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
    public void Value_returns_typed_component_source()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var source = p.Component<FusionDropDownList>(m => m.Status).Value();
            Assert.That(source, Is.TypeOf<TypedComponentSource<string>>());

            var bindSource = source.ToBindSource();
            Assert.That(bindSource, Is.TypeOf<ComponentSource>());

            var cs = (ComponentSource)bindSource;
            Assert.That(cs.ComponentId, Is.EqualTo("Alis_Reactive_Fusion_UnitTests_FusionTestModel__Status"));
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
    public void Focus_descriptor_has_correct_js_event()
    {
        var descriptor = FusionDropDownListEvents.Instance.Focus;
        Assert.That(descriptor.JsEvent, Is.EqualTo("focus"));
    }

    [Test]
    public void Focus_descriptor_provides_args_instance()
    {
        var descriptor = FusionDropDownListEvents.Instance.Focus;
        Assert.That(descriptor.Args, Is.Not.Null);
        Assert.That(descriptor.Args, Is.TypeOf<FusionDropDownListFocusArgs>());
    }

    [Test]
    public void Blur_descriptor_has_correct_js_event()
    {
        var descriptor = FusionDropDownListEvents.Instance.Blur;
        Assert.That(descriptor.JsEvent, Is.EqualTo("blur"));
    }

    [Test]
    public void Blur_descriptor_provides_args_instance()
    {
        var descriptor = FusionDropDownListEvents.Instance.Blur;
        Assert.That(descriptor.Args, Is.Not.Null);
        Assert.That(descriptor.Args, Is.TypeOf<FusionDropDownListBlurArgs>());
    }

    [Test]
    public Task Focus_event_wires_component_trigger()
    {
        var plan = CreatePlan();
        var descriptor = FusionDropDownListEvents.Instance.Focus;

        var pb = new PipelineBuilder<FusionTestModel>();
        pb.Element("status").SetText("Focused");

        var trigger = new ComponentEventTrigger("Status", descriptor.JsEvent, "fusion", "Status", "value");
        plan.AddEntry(new Entry(trigger, pb.BuildReaction()));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Blur_event_wires_component_trigger()
    {
        var plan = CreatePlan();
        var descriptor = FusionDropDownListEvents.Instance.Blur;

        var pb = new PipelineBuilder<FusionTestModel>();
        pb.Element("status").SetText("Blurred");

        var trigger = new ComponentEventTrigger("Status", descriptor.JsEvent, "fusion", "Status", "value");
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
            p.Component<FusionDropDownList>(m => m.Status).SetValue("US").ShowPopup();
            p.Element("echo").SetText("Initialized");
        });
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
