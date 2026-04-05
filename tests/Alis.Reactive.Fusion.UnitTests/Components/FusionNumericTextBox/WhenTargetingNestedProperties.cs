using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.Fusion.UnitTests;

/// <summary>
/// Tests that nested model property expressions produce correct element IDs
/// and that the vendor field appears in the plan JSON.
///
/// For m => m.Address.PostalCode:
///   - Element ID (target/componentId): "Address_PostalCode" (underscores)
///   - Binding path: "Address.PostalCode" (dots -- for future HTTP gather)
///   - Vendor: "fusion"
/// </summary>
[TestFixture]
public class WhenTargetingNestedProperties : FusionTestBase
{
    [Test]
    public Task Nested_property_target_uses_underscores()
    {
        var plan = CreatePlan();

        Trigger(plan).DomReady(p =>
        {
            p.Component<FusionNumericTextBox>(m => m.Address!.PostalCode).SetValue(12345);
            p.Element("echo").SetText("PostalCode set");
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
            p.Component<FusionNumericTextBox>(m => m.Address!.PostalCode).SetValue(99);
        });

        var json = plan.Render();
        Assert.That(json, Does.Contain("Address_PostalCode"));
    }
}
