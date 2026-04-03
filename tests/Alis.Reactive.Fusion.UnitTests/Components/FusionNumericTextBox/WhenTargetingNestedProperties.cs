using Alis.Reactive.Builders;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

/// <summary>
/// Tests that nested model property expressions produce correct element IDs
/// and that the vendor field appears in the plan JSON.
///
/// For m => m.Address.PostalCode:
///   - Element ID (target/componentId): "Address_PostalCode" (underscores)
///   - Binding path: "Address.PostalCode" (dots — for future HTTP gather)
///   - Vendor: "fusion"
/// </summary>
[TestFixture]
public class WhenTargetingNestedProperties : FusionTestBase
{
    [Test]
    public Task Nested_property_target_uses_underscores()
    {
        var plan = CreatePlan();
        WireObjectEvent(
            plan,
            "Address_PostalCode",
            "fusion",
            "Address.PostalCode",
            "value",
            FusionNumericTextBoxEvents.Instance.Changed.EventName,
            pb =>
            {
                pb.Component<FusionNumericTextBox>(m => m.Address!.PostalCode).SetValue(12345);
                pb.Element("echo").SetText("PostalCode set");
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
            "Amount",
            "fusion",
            "Amount",
            "value",
            FusionNumericTextBoxEvents.Instance.Changed.EventName,
            pb => pb.Element("echo").SetText("changed"));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public void Component_expression_resolves_to_element_id()
    {
        var pb = CreateDomReadyPipeline();
        var compRef = pb.Component<FusionNumericTextBox>(m => m.Address!.PostalCode);
        compRef.SetValue(99);

        var action = pb.BuildAction();
        var json = System.Text.Json.JsonSerializer.Serialize(action,
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });

        Assert.That(json, Does.Contain("\"object\":\"component::Alis_Reactive_Fusion_UnitTests_FusionTestModel__Address_PostalCode\""));
    }
}
