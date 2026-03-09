using Alis.Reactive;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;
using Alis.Reactive.Fusion.Components;
using static VerifyNUnit.Verifier;

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
        var descriptor = FusionNumericTextBoxEvents.Instance.Changed;

        var pb = new PipelineBuilder<FusionTestModel>();
        pb.Component<FusionNumericTextBox>(m => m.Address!.PostalCode).SetValue(12345);
        pb.Element("echo").SetText("PostalCode set");

        var trigger = new ComponentEventTrigger("Address_PostalCode", descriptor.JsEvent, "fusion", "Address.PostalCode");
        plan.AddEntry(new Entry(trigger, pb.BuildReaction()));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Vendor_field_serialized_in_trigger()
    {
        var plan = CreatePlan();
        var descriptor = FusionNumericTextBoxEvents.Instance.Changed;

        var pb = new PipelineBuilder<FusionTestModel>();
        pb.Element("echo").SetText("changed");

        var trigger = new ComponentEventTrigger("Amount", descriptor.JsEvent, "fusion", "Amount");
        plan.AddEntry(new Entry(trigger, pb.BuildReaction()));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public void Component_expression_resolves_to_element_id()
    {
        var pb = new PipelineBuilder<FusionTestModel>();
        var compRef = pb.Component<FusionNumericTextBox>(m => m.Address!.PostalCode);
        compRef.SetValue(99);

        var reaction = pb.BuildReaction();
        var json = System.Text.Json.JsonSerializer.Serialize(reaction,
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });

        Assert.That(json, Does.Contain("\"target\":\"Address_PostalCode\""));
    }
}
