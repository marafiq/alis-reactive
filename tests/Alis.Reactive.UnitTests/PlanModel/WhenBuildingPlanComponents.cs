using Alis.Reactive;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.UnitTests.PlanModel;

[TestFixture]
public sealed class WhenBuildingPlanComponents
{
    [Test]
    public void component_entry_key_must_match_component_id_at_the_document_boundary()
    {
        var components = new Dictionary<string, Component>
        {
            ["declared-key"] = Component.Element(
                "actual-id",
                ComponentVendor.Native.Value,
                "native.element.actual-id")
        };

        var exception = Assert.Throws<ArgumentException>(() => PlanComponents.From(components));

        Assert.That(exception!.Message, Does.Contain("Plan component key 'declared-key'"));
        Assert.That(exception.Message, Does.Contain("component 'actual-id'"));
        Assert.That(exception.Message, Does.Contain("deterministic runtime join keys"));
    }

    [Test]
    public void component_catalog_rejects_a_replacement_under_a_different_key()
    {
        var plan = new ReactivePlan<PlanComponentModel>();
        var key = plan.Context.EnsureElement("declared-key");

        var replacement = Component.Element(
            "actual-id",
            ComponentVendor.Native.Value,
            "native.element.actual-id");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            plan.Context.SetComponent(key, replacement));

        Assert.That(exception!.Message, Does.Contain("Plan component key 'declared-key'"));
        Assert.That(exception.Message, Does.Contain("component 'actual-id'"));
        Assert.That(exception.Message, Does.Contain("deterministic runtime join keys"));
    }
}

internal sealed class PlanComponentModel
{
}
