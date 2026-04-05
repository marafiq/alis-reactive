using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.UnitTests;

/// <summary>
/// Tests that nested model property expressions produce correct element IDs
/// and that the vendor field appears in the plan JSON.
///
/// For m => m.Address.City:
///   - Element ID (target/componentId): "Address_City" (underscores)
///   - Binding path: "Address.City" (dots — for future HTTP gather)
///   - Vendor: "native"
/// </summary>
[TestFixture]
public class WhenTargetingNestedProperties : NativeTestBase
{
    [Test]
    public Task Nested_property_target_uses_underscores()
    {
        var plan = CreatePlan();

        Trigger(plan).DomReady(p =>
        {
            p.Component<NativeDropDown>(m => m.Address!.City).SetValue("Portland");
            p.Element("echo").SetText("City set");
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Vendor_field_serialized_in_trigger()
    {
        var plan = CreatePlan();

        Trigger(plan).DomReady(p =>
            p.Element("echo").SetText("changed"));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public void Component_expression_resolves_to_element_id()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            var compRef = p.Component<NativeDropDown>(m => m.Address!.City);
            compRef.SetValue("Seattle");
        });

        var json = plan.Render();
        Assert.That(json, Does.Contain("Address_City"));
    }

    [Test]
    public Task Cross_vendor_mutation_with_nested_target()
    {
        var plan = CreatePlan();

        Trigger(plan).DomReady(p =>
        {
            p.Component<NativeDropDown>(m => m.Address!.City).SetValue("Denver");
            p.Component<NativeDropDown>(m => m.Address!.State).SetValue("CO");
            p.Element("echo").SetText("Address updated");
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
