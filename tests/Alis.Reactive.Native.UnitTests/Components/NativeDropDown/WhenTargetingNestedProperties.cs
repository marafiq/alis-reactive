using Alis.Reactive.Builders;
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
        WireObjectEvent(
            plan,
            "Address_City",
            "native",
            "Address.City",
            "value",
            NativeDropDownEvents.Instance.Changed.EventName,
            pb =>
            {
                pb.Component<NativeDropDown>(m => m.Address!.City).SetValue("Portland");
                pb.Element("echo").SetText("City set");
            });

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Vendor_field_serialized_in_trigger()
    {
        var plan = CreatePlan();
        WireObjectEvent(
            plan,
            "Status",
            "native",
            "Status",
            "value",
            NativeDropDownEvents.Instance.Changed.EventName,
            pb => pb.Element("echo").SetText("changed"));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public void Component_expression_resolves_to_element_id()
    {
        var pb = CreateDomReadyPipeline();
        var compRef = pb.Component<NativeDropDown>(m => m.Address!.City);
        compRef.SetValue("Seattle");

        var action = pb.BuildAction();
        var json = System.Text.Json.JsonSerializer.Serialize(action,
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });

        Assert.That(json, Does.Contain("\"object\":\"component::Alis_Reactive_Native_UnitTests_NativeTestModel__Address_City\""));
    }

    [Test]
    public Task Cross_vendor_mutation_with_nested_target()
    {
        var plan = CreatePlan();
        WireObjectEvent(
            plan,
            "Address_City",
            "native",
            "Address.City",
            "value",
            NativeDropDownEvents.Instance.Changed.EventName,
            pb =>
            {
                pb.Component<NativeDropDown>(m => m.Address!.City).SetValue("Denver");
                pb.Component<NativeDropDown>(m => m.Address!.State).SetValue("CO");
                pb.Element("echo").SetText("Address updated");
            });

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
