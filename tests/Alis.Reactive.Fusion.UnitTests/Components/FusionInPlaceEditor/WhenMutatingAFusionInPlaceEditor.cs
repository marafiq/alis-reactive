using Alis.Reactive.Builders;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenMutatingAFusionInPlaceEditor : FusionTestBase
{
    [Test]
    public void SetValue_with_string_produces_set_reaction()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<FusionInPlaceEditor>(m => m.PhoneNumber).SetValue("555-0123"));

        var json = plan.RenderFormatted();
        AssertSchemaValid(json);

        Assert.That(json, Does.Contain("\"set\""));
        Assert.That(json, Does.Contain("\"value\""));
        Assert.That(json, Does.Contain("555-0123"));
    }

    [Test]
    public void SetValue_with_null_produces_set_reaction()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<FusionInPlaceEditor>(m => m.PhoneNumber).SetValue(null));

        var json = plan.RenderFormatted();
        AssertSchemaValid(json);

        Assert.That(json, Does.Contain("\"set\""));
        Assert.That(json, Does.Contain("\"value\""));
    }

    [Test]
    public void Enable_emits_call_to_disable_with_false_arg()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<FusionInPlaceEditor>(m => m.PhoneNumber).Enable());

        var json = plan.RenderFormatted();
        AssertSchemaValid(json);

        Assert.That(json, Does.Contain("\"call\""));
        Assert.That(json, Does.Contain("\"disable\""));
        Assert.That(json, Does.Contain("false"));
    }

    [Test]
    public void Disable_emits_call_to_disable_with_true_arg()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<FusionInPlaceEditor>(m => m.PhoneNumber).Disable());

        var json = plan.RenderFormatted();
        AssertSchemaValid(json);

        Assert.That(json, Does.Contain("\"call\""));
        Assert.That(json, Does.Contain("\"disable\""));
        Assert.That(json, Does.Contain("true"));
    }

    [Test]
    public void Save_produces_call_reaction_to_save()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<FusionInPlaceEditor>(m => m.PhoneNumber).Save());

        var json = plan.RenderFormatted();
        AssertSchemaValid(json);

        Assert.That(json, Does.Contain("\"call\""));
        Assert.That(json, Does.Contain("\"save\""));
    }

    [Test]
    public void Focus_produces_call_reaction_to_setFocus()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<FusionInPlaceEditor>(m => m.PhoneNumber).Focus());

        var json = plan.RenderFormatted();
        AssertSchemaValid(json);

        Assert.That(json, Does.Contain("\"call\""));
        Assert.That(json, Does.Contain("\"setFocus\""));
    }

    [Test]
    public void AddClass_produces_call_reaction_to_classList_add()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<FusionInPlaceEditor>(m => m.PhoneNumber).AddClass("alis-editor-saved"));

        var json = plan.RenderFormatted();
        AssertSchemaValid(json);

        Assert.That(json, Does.Contain("\"call\""));
        Assert.That(json, Does.Contain("\"classAdd\""));
        // Path walks through ej.element (SF wraps the DOM element there) then classList.add.
        Assert.That(json, Does.Contain("\"element\""));
        Assert.That(json, Does.Contain("\"classList\""));
        Assert.That(json, Does.Contain("\"add\""));
        Assert.That(json, Does.Contain("alis-editor-saved"));
    }

    [Test]
    public void RemoveClass_produces_call_reaction_to_classList_remove()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
            p.Component<FusionInPlaceEditor>(m => m.PhoneNumber).RemoveClass("alis-editor-saved"));

        var json = plan.RenderFormatted();
        AssertSchemaValid(json);

        Assert.That(json, Does.Contain("\"call\""));
        Assert.That(json, Does.Contain("\"classRemove\""));
        Assert.That(json, Does.Contain("\"element\""));
        Assert.That(json, Does.Contain("\"classList\""));
        Assert.That(json, Does.Contain("\"remove\""));
        Assert.That(json, Does.Contain("alis-editor-saved"));
    }

}
