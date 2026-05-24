using System;
using Alis.Reactive;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Fusion.Components;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenReadingFusionInPlaceEditorValue : FusionTestBase
{
    [Test]
    public void Value_returns_typed_component_source_of_string()
    {
        var plan = CreatePlan();
        RegisterStringInPlaceEditor(plan, "PhoneNumber", "Alis_Reactive_Fusion_UnitTests_FusionTestModel__PhoneNumber");

        Trigger(plan).DomReady(p =>
        {
            var source = p.Component<FusionInPlaceEditor>(m => m.PhoneNumber).Value();
            Assert.That(source, Is.TypeOf<TypedComponentSource<string>>());
        });
    }

    [Test]
    public void Value_read_is_reflected_in_rendered_plan()
    {
        var plan = CreatePlan();
        RegisterStringInPlaceEditor(plan, "PhoneNumber", "Alis_Reactive_Fusion_UnitTests_FusionTestModel__PhoneNumber");

        Trigger(plan).DomReady(p =>
        {
            var comp = p.Component<FusionInPlaceEditor>(m => m.PhoneNumber);
            p.Element("echo").SetText(comp.Value());
        });

        var json = plan.RenderFormatted();

        Assert.That(json, Does.Contain("\"value\""), "Must reference the 'value' member");
    }

    // Regression: PR #117 reviewer flagged that Value() hardcoded Shape.String, clashing with the
    // non-string shape registered by HtmlExtensions for Date/Numeric inner types.
    // Calling Value() after registration must honor the registered shape and not throw.
    [Test]
    public void Value_uses_registered_shape_for_date_bound_editor_without_shape_conflict()
    {
        var plan = CreatePlan();

        // Simulate HtmlExtensions registering the component with Date shape (DateTime? binding).
        var elementId = "Alis_Reactive_Fusion_UnitTests_FusionTestModel__AppointmentTime";
        var registration = ModelBoundInputComponentSlot
            .For<System.DateTime?>(elementId, "AppointmentTime")
            .Register(FusionInPlaceEditor.Registration);
        plan.RegisterInputComponent(registration);

        // Value() on the Date-shaped component must not throw a shape-conflict.
        Assert.DoesNotThrow(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                var source = p.Component<FusionInPlaceEditor>(m => m.AppointmentTime).Value();
                Assert.That(source, Is.TypeOf<TypedComponentSource<string>>());
            });
        });
    }

    [Test]
    public void Value_uses_registered_shape_for_decimal_bound_editor_without_shape_conflict()
    {
        var plan = CreatePlan();

        var elementId = "Alis_Reactive_Fusion_UnitTests_FusionTestModel__Amount";
        var registration = ModelBoundInputComponentSlot
            .For<decimal>(elementId, "Amount")
            .Register(FusionInPlaceEditor.Registration);
        plan.RegisterInputComponent(registration);

        Assert.DoesNotThrow(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                var source = p.Component<FusionInPlaceEditor>(m => m.Amount).Value();
                Assert.That(source, Is.TypeOf<TypedComponentSource<string>>());
            });
        });
    }

    // Regression: PR #117 codex P2 flagged `?? Shape.String` fallback as a Rule 3 violation.
    // When a plan calls .Value() before the editor has been registered via HtmlExtensions,
    // we must throw with context instead of inventing a shape.
    [Test]
    public void Value_throws_when_editor_is_not_registered()
    {
        var plan = CreatePlan();

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Component<FusionInPlaceEditor>(m => m.PhoneNumber).Value();
            });
        });

        Assert.That(ex!.Message, Does.Contain("FusionInPlaceEditor"));
        Assert.That(ex.Message, Does.Contain("not registered"));
        Assert.That(ex.Message, Does.Contain("Html.InputField"));
    }

    private static void RegisterStringInPlaceEditor(ReactivePlan<FusionTestModel> plan, string bindingPath, string elementId)
    {
        var registration = ModelBoundInputComponentSlot
            .For<string>(elementId, bindingPath)
            .Register(FusionInPlaceEditor.Registration);
        plan.RegisterInputComponent(registration);
    }
}
