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
        var component = new FusionInPlaceEditor();
        var registration = new ComponentRegistration(
            "phoneNumber", component.Vendor, "PhoneNumber", component.ValueMember,
            "inplace-editor", Shape.FromClrType(typeof(string)));

        plan.AddToComponentsMap("PhoneNumber", registration);

        Assert.That(plan.ComponentsMap, Contains.Key("PhoneNumber"));
        var reg = plan.ComponentsMap["PhoneNumber"];
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
        var component = new FusionInPlaceEditor();
        var registration = new ComponentRegistration(
            "amount", component.Vendor, "Amount", component.ValueMember,
            "inplace-editor", Shape.FromClrType(typeof(decimal)));

        plan.AddToComponentsMap("Amount", registration);

        var reg = plan.ComponentsMap["Amount"];
        Assert.That(reg.ComponentType, Is.EqualTo("inplace-editor"));
        Assert.That(reg.Shape.Kind, Is.EqualTo(Shape.Number.Kind));
    }

    [Test]
    public void Registration_for_nullable_datetime_property_has_nullable_date_shape()
    {
        var plan = CreatePlan();
        var component = new FusionInPlaceEditor();
        var registration = new ComponentRegistration(
            "appointmentTime", component.Vendor, "AppointmentTime", component.ValueMember,
            "inplace-editor", Shape.FromClrType(typeof(DateTime?)));

        plan.AddToComponentsMap("AppointmentTime", registration);

        var reg = plan.ComponentsMap["AppointmentTime"];
        Assert.That(reg.ComponentType, Is.EqualTo("inplace-editor"));
        Assert.That(reg.Shape.Kind, Is.EqualTo("nullable"));
        Assert.That(reg.Shape.Inner, Is.Not.Null);
        Assert.That(reg.Shape.Inner!.Kind, Is.EqualTo(Shape.Date.Kind));
    }

    [Test]
    public void Registration_for_non_nullable_datetime_property_has_date_shape()
    {
        var plan = CreatePlan();
        var component = new FusionInPlaceEditor();
        var registration = new ComponentRegistration(
            "appointmentTime", component.Vendor, "AppointmentTime", component.ValueMember,
            "inplace-editor", Shape.FromClrType(typeof(DateTime)));

        plan.AddToComponentsMap("AppointmentTime", registration);

        var reg = plan.ComponentsMap["AppointmentTime"];
        Assert.That(reg.ComponentType, Is.EqualTo("inplace-editor"));
        Assert.That(reg.Shape.Kind, Is.EqualTo(Shape.Date.Kind));
    }
}
