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
    public void Value_returns_typed_component_source_of_TProp()
    {
        var plan = CreatePlan();
        RegisterStringInPlaceEditor(plan, "PhoneNumber", "Alis_Reactive_Fusion_UnitTests_FusionTestModel__PhoneNumber");

        Trigger(plan).DomReady(p =>
        {
            var source = p.Component<FusionInPlaceEditor>(m => m.PhoneNumber).Value<string>();
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
            p.Element("echo").SetText(comp.Value<string>());
        });

        var json = plan.RenderFormatted();
        AssertSchemaValid(json);

        Assert.That(json, Does.Contain("\"value\""), "Must reference the 'value' member");
    }

    // End-to-end proof: a DateTime?-bound InPlaceEditor's Value<DateTime?>() must
    // emit "shape": { "kind": "nullable", "inner": { "kind": "date" } } — NOT the
    // hardcoded "string" that the old API produced regardless of binding.
    [Test]
    public void Value_for_nullable_date_binding_emits_nullable_date_shape_in_plan_json()
    {
        var plan = CreatePlan();
        var component = new FusionInPlaceEditor();

        var elementId = "Alis_Reactive_Fusion_UnitTests_FusionTestModel__AppointmentTime";
        var registration = new ComponentRegistration(
            elementId, component.Vendor, "AppointmentTime", component.ValueMember,
            "inplace-editor", Shape.FromClrType(typeof(DateTime?)));
        plan.AddToComponentsMap("AppointmentTime", registration);

        Trigger(plan).DomReady(p =>
        {
            var comp = p.Component<FusionInPlaceEditor>(m => m.AppointmentTime);
            p.Element("echo").SetText(comp.Value<DateTime?>());
        });

        var json = plan.RenderFormatted();
        AssertSchemaValid(json);

        // The inline ReadProducer inside the SetText reaction must carry the honest
        // Nullable(Date) shape, not the old hardcoded Shape.String.
        Assert.That(json, Does.Contain("\"kind\": \"nullable\""),
            "Read producer must serialize nullable wrapper for DateTime? binding");
        Assert.That(json, Does.Contain("\"kind\": \"date\""),
            "Read producer's inner shape must be date for DateTime? binding");
        // Note: the destination element's 'text' property legitimately carries Shape.String
        // as the write destination (SetText writes to innerText). That's separate from the
        // source read. The positive nullable/date assertions above prove the source read's
        // shape is honest for the DateTime? binding.
    }

    [Test]
    public void Value_for_decimal_binding_emits_number_shape_in_plan_json()
    {
        var plan = CreatePlan();
        var component = new FusionInPlaceEditor();

        var elementId = "Alis_Reactive_Fusion_UnitTests_FusionTestModel__Amount";
        var registration = new ComponentRegistration(
            elementId, component.Vendor, "Amount", component.ValueMember,
            "inplace-editor", Shape.FromClrType(typeof(decimal)));
        plan.AddToComponentsMap("Amount", registration);

        Trigger(plan).DomReady(p =>
        {
            var comp = p.Component<FusionInPlaceEditor>(m => m.Amount);
            p.Element("echo").SetText(comp.Value<decimal>());
        });

        var json = plan.RenderFormatted();
        AssertSchemaValid(json);

        Assert.That(json, Does.Contain("\"kind\": \"number\""),
            "Read producer must carry Shape.Number for decimal binding");
    }

    // Mismatch-throw regression: .Value<WrongTProp>() must throw at plan build
    // with a pointing message naming the expected CLR type.
    [Test]
    public void Value_with_mismatched_TProp_throws_at_plan_build()
    {
        var plan = CreatePlan();
        var component = new FusionInPlaceEditor();

        var elementId = "Alis_Reactive_Fusion_UnitTests_FusionTestModel__AppointmentTime";
        var registration = new ComponentRegistration(
            elementId, component.Vendor, "AppointmentTime", component.ValueMember,
            "inplace-editor", Shape.FromClrType(typeof(DateTime?)));
        plan.AddToComponentsMap("AppointmentTime", registration);

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Component<FusionInPlaceEditor>(m => m.AppointmentTime).Value<string>();
            });
        });

        Assert.That(ex!.Message, Does.Contain("FusionInPlaceEditor"));
        Assert.That(ex.Message, Does.Contain("Value"));
        Assert.That(ex.Message, Does.Contain("expected shape"));
    }

    // Note: the "not registered + no ExpressionClrType" runtime-throw path is no longer
    // reachable from the DSL because ID-based factories (p.Component<T>("id")) return the
    // base ComponentRef<T, TModel> which does NOT expose Value<TProp>(). The C# compiler
    // forbids the call before plan-build runs. Only expression-based factories yield
    // InputComponentRef<T, TModel> with Value<TProp>(), and those always have an
    // ExpressionClrType captured from the lambda — so the cross-check always has a
    // source of expected shape. The runtime throw survives as defense-in-depth but is
    // structurally unreachable through supported DSL usage.

    private static void RegisterStringInPlaceEditor(ReactivePlan<FusionTestModel> plan, string bindingPath, string elementId)
    {
        var component = new FusionInPlaceEditor();
        var registration = new ComponentRegistration(
            elementId, component.Vendor, bindingPath, component.ValueMember,
            "inplace-editor", Shape.String);
        plan.AddToComponentsMap(bindingPath, registration);
    }
}
