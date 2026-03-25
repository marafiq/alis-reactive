namespace Alis.Reactive.UnitTests.ValidationEnrichment;

[TestFixture]
public class WhenRegisteringComponents
{
    [Test]
    public void Duplicate_binding_path_with_different_component_throws()
    {
        var plan = new ReactivePlan<EnrichmentTestModel>();

        plan.AddToComponentsMap("Name", new ComponentRegistration("name-input", "native", "Name", "value", "textbox", "string"));

        Assert.Throws<InvalidOperationException>(() =>
            plan.AddToComponentsMap("Name", new ComponentRegistration("other-input", "fusion", "Name", "value", "autocomplete", "string")));
    }

    [Test]
    public void Same_registration_is_idempotent()
    {
        var plan = new ReactivePlan<EnrichmentTestModel>();

        plan.AddToComponentsMap("Name", new ComponentRegistration("name-input", "native", "Name", "value", "textbox", "string"));
        plan.AddToComponentsMap("Name", new ComponentRegistration("name-input", "native", "Name", "value", "textbox", "string"));

        Assert.That(plan.ComponentsMap.Count, Is.EqualTo(1));
    }

    [Test]
    public void Same_path_same_ids_different_coerceAs_throws()
    {
        var plan = new ReactivePlan<EnrichmentTestModel>();

        plan.AddToComponentsMap("Amount", new ComponentRegistration("amount-input", "fusion", "Amount", "value", "numerictextbox", "number"));

        // Same component, same IDs — but CoerceAs differs. This is a plan bug
        // (e.g., two different TProp bindings to the same element). Must throw, not silently ignore.
        Assert.Throws<InvalidOperationException>(() =>
            plan.AddToComponentsMap("Amount", new ComponentRegistration("amount-input", "fusion", "Amount", "value", "numerictextbox", "string")));
    }

    [Test]
    public void Same_registration_including_coerceAs_is_idempotent()
    {
        var plan = new ReactivePlan<EnrichmentTestModel>();

        plan.AddToComponentsMap("Date", new ComponentRegistration("date-input", "fusion", "Date", "value", "datepicker", "date"));
        plan.AddToComponentsMap("Date", new ComponentRegistration("date-input", "fusion", "Date", "value", "datepicker", "date"));

        Assert.That(plan.ComponentsMap.Count, Is.EqualTo(1));
    }
}
