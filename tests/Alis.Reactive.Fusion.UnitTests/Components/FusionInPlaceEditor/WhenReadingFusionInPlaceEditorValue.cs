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
        Trigger(plan).DomReady(p =>
        {
            var comp = p.Component<FusionInPlaceEditor>(m => m.PhoneNumber);
            p.Element("echo").SetText(comp.Value());
        });

        var json = plan.RenderFormatted();
        AssertSchemaValid(json);

        Assert.That(json, Does.Contain("\"value\""), "Must reference the 'value' member");
    }

    // Regression: PR #117 reviewer flagged that Value() hardcoded Shape.String, clashing with the
    // non-string shape registered by HtmlExtensions for Date/Numeric inner types.
    // Calling Value() after registration must honor the registered shape and not throw.
    [Test]
    public void Value_uses_registered_shape_for_date_bound_editor_without_shape_conflict()
    {
        var plan = CreatePlan();
        var component = new FusionInPlaceEditor();

        // Simulate HtmlExtensions registering the component with Date shape (DateTime? binding).
        var elementId = "Alis_Reactive_Fusion_UnitTests_FusionTestModel__AppointmentTime";
        var registration = new ComponentRegistration(
            elementId, component.Vendor, "AppointmentTime", component.ValueMember,
            "inplace-editor", Shape.FromClrType(typeof(System.DateTime?)));
        plan.AddToComponentsMap("AppointmentTime", registration);

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
        var component = new FusionInPlaceEditor();

        var elementId = "Alis_Reactive_Fusion_UnitTests_FusionTestModel__Amount";
        var registration = new ComponentRegistration(
            elementId, component.Vendor, "Amount", component.ValueMember,
            "inplace-editor", Shape.FromClrType(typeof(decimal)));
        plan.AddToComponentsMap("Amount", registration);

        Assert.DoesNotThrow(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                var source = p.Component<FusionInPlaceEditor>(m => m.Amount).Value();
                Assert.That(source, Is.TypeOf<TypedComponentSource<string>>());
            });
        });
    }
}
