namespace Alis.Reactive.UnitTests.ValidationEnrichment;

[TestFixture]
public class WhenRegisteringComponents
{
    [Test]
    public void Duplicate_binding_path_with_different_component_throws()
    {
        var plan = new ReactivePlan<EnrichmentTestModel>();

        plan.AddToComponentsMap("Name", new ComponentRegistration("name-input", "native", "Name", "value"));

        Assert.Throws<InvalidOperationException>(() =>
            plan.AddToComponentsMap("Name", new ComponentRegistration("other-input", "fusion", "Name", "value")));
    }

    [Test]
    public void Same_registration_is_idempotent()
    {
        var plan = new ReactivePlan<EnrichmentTestModel>();

        plan.AddToComponentsMap("Name", new ComponentRegistration("name-input", "native", "Name", "value"));
        plan.AddToComponentsMap("Name", new ComponentRegistration("name-input", "native", "Name", "value"));

        Assert.That(plan.ComponentsMap.Count, Is.EqualTo(1));
    }
}
