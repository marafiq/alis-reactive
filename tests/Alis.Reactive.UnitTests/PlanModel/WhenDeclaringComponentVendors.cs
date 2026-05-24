using Alis.Reactive.PlanModel;

namespace Alis.Reactive.UnitTests.PlanModel;

[TestFixture]
public sealed class WhenDeclaringComponentVendors
{
    [Test]
    public void component_vendors_are_open_tokens_for_new_component_slices()
    {
        var registration = ComponentRegistration.RegisteredInput(
            RegisteredComponentIdentity.For("resident-status", "acme-widget"),
            RegisteredComponentBinding.For("Status", "value"),
            ComponentKind.Of("statuspicker"),
            Shape.String);

        Assert.That(registration.Vendor, Is.EqualTo("acme-widget"));
    }

    [Test]
    public void component_type_keys_are_namespaced_away_from_native_element_type_keys()
    {
        var elementType = TypeKey.NativeElement(ComponentId.Of("address"));
        var componentType = TypeKey.Component(
            ComponentVendor.Native,
            ComponentId.Of("element.address"));

        Assert.That(componentType, Is.Not.EqualTo(elementType));
        Assert.That(componentType.Value, Is.EqualTo("native.component.element.address"));
    }

    [Test]
    public void component_type_keys_keep_vendor_prefix_for_component_discovery()
    {
        var typeKey = TypeKey.Component(
            ComponentVendor.From("acme-widget"),
            ComponentId.Of("resident-status"));

        Assert.That(typeKey.Value, Does.StartWith("acme-widget."));
        Assert.That(typeKey.Value, Is.EqualTo("acme-widget.component.resident-status"));
    }

    [Test]
    public void component_vendor_tokens_reject_ambiguous_type_key_separators()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            ComponentVendor.From("acme.widget"));

        Assert.That(ex!.Message, Does.Contain("Vendor tokens must start with a letter"));
    }
}
