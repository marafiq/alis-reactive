using Alis.Reactive;
using Alis.Reactive.Fusion.Components;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.UnitTests;

[TestFixture]
public class WhenRegisteringAFusionInPlaceEditor : FusionTestBase
{
    [Test]
    public void Component_declares_fusion_vendor_and_value_member()
    {
        var component = new FusionInPlaceEditor();
        Assert.That(component.Vendor, Is.EqualTo("fusion"));
        Assert.That(component.ValueMember, Is.EqualTo("value"));
    }

    [Test]
    public void Registration_for_string_property_has_string_shape()
    {
        var plan = CreatePlan();
        var registration = ModelBoundInputComponentSlot
            .For<string>("phoneNumber", "PhoneNumber")
            .Register(FusionInPlaceEditor.Registration);

        plan.RegisterInputComponent(registration);

        Assert.That(plan.RegisteredInputComponents, Contains.Key("PhoneNumber"));
        var reg = plan.RegisteredInputComponents["PhoneNumber"];
        Assert.That(reg.Vendor, Is.EqualTo("fusion"));
        Assert.That(reg.ValueMember, Is.EqualTo("value"));
        Assert.That(reg.ComponentType, Is.EqualTo("inplace-editor"));
        Assert.That(reg.BindingPath, Is.EqualTo("PhoneNumber"));
        Assert.That(reg.Shape.Kind, Is.EqualTo(Shape.String.Kind));
    }

    [Test]
    public void Registration_for_decimal_property_has_number_shape()
    {
        var plan = CreatePlan();
        var registration = ModelBoundInputComponentSlot
            .For<decimal>("amount", "Amount")
            .Register(FusionInPlaceEditor.Registration);

        plan.RegisterInputComponent(registration);

        var reg = plan.RegisteredInputComponents["Amount"];
        Assert.That(reg.ComponentType, Is.EqualTo("inplace-editor"));
        Assert.That(reg.Shape.Kind, Is.EqualTo(Shape.Number.Kind));
    }

    [Test]
    public void Registration_for_nullable_datetime_property_has_nullable_date_shape()
    {
        var plan = CreatePlan();
        var registration = ModelBoundInputComponentSlot
            .For<DateTime?>("appointmentTime", "AppointmentTime")
            .Register(FusionInPlaceEditor.Registration);

        plan.RegisterInputComponent(registration);

        var reg = plan.RegisteredInputComponents["AppointmentTime"];
        Assert.That(reg.ComponentType, Is.EqualTo("inplace-editor"));
        Assert.That(reg.Shape.Kind, Is.EqualTo("nullable"));
        Assert.That(reg.Shape.IsNullableOf(Shape.Date), Is.True);
    }

    [Test]
    public void Registration_for_non_nullable_datetime_property_has_date_shape()
    {
        var plan = CreatePlan();
        var registration = ModelBoundInputComponentSlot
            .For<DateTime>("appointmentTime", "AppointmentTime")
            .Register(FusionInPlaceEditor.Registration);

        plan.RegisterInputComponent(registration);

        var reg = plan.RegisteredInputComponents["AppointmentTime"];
        Assert.That(reg.ComponentType, Is.EqualTo("inplace-editor"));
        Assert.That(reg.Shape.Kind, Is.EqualTo(Shape.Date.Kind));
    }
}
