using Alis.Reactive;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;
using Alis.Reactive.Native.Components;
using static VerifyNUnit.Verifier;

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
        var descriptor = NativeDropDownEvents.Instance.Changed;

        var pb = new PipelineBuilder<NativeTestModel>();
        pb.Component<NativeDropDown>(m => m.Address!.City).SetValue("Portland");
        pb.Element("echo").SetText("City set");

        var trigger = new ComponentEventTrigger("Address_City", descriptor.JsEvent, "native", "Address.City");
        plan.AddEntry(new Entry(trigger, pb.BuildReaction()));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Vendor_field_serialized_in_trigger()
    {
        var plan = CreatePlan();
        var descriptor = NativeDropDownEvents.Instance.Changed;

        var pb = new PipelineBuilder<NativeTestModel>();
        pb.Element("echo").SetText("changed");

        var trigger = new ComponentEventTrigger("Status", descriptor.JsEvent, "native", "Status");
        plan.AddEntry(new Entry(trigger, pb.BuildReaction()));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public void Component_expression_resolves_to_element_id()
    {
        var pb = new PipelineBuilder<NativeTestModel>();
        var compRef = pb.Component<NativeDropDown>(m => m.Address!.City);
        compRef.SetValue("Seattle");

        var reaction = pb.BuildReaction();
        var json = System.Text.Json.JsonSerializer.Serialize(reaction,
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });

        Assert.That(json, Does.Contain("\"target\":\"Address_City\""));
    }

    [Test]
    public Task Cross_vendor_mutation_with_nested_target()
    {
        var plan = CreatePlan();
        var descriptor = NativeDropDownEvents.Instance.Changed;

        var pb = new PipelineBuilder<NativeTestModel>();
        pb.Component<NativeDropDown>(m => m.Address!.City).SetValue("Denver");
        pb.Component<NativeDropDown>(m => m.Address!.State).SetValue("CO");
        pb.Element("echo").SetText("Address updated");

        var trigger = new ComponentEventTrigger("Address_City", descriptor.JsEvent, "native", "Address.City");
        plan.AddEntry(new Entry(trigger, pb.BuildReaction()));

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
