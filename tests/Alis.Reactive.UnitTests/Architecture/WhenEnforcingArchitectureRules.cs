namespace Alis.Reactive.UnitTests;

[TestFixture]
public class WhenEnforcingArchitectureRules
{
    [Test]
    public void Removed_namespace_stays_deleted()
    {
        var assembly = typeof(ReactivePlan<>).Assembly;
        var removedNamespace = "Alis.Reactive.Descriptors";

        var lingeringTypes = assembly.GetTypes()
            .Where(t => t.Namespace != null && t.Namespace.StartsWith(removedNamespace))
            .Select(t => t.FullName)
            .ToList();

        Assert.That(lingeringTypes, Is.Empty,
            $"Removed namespace should stay deleted, but these types remain: {string.Join(", ", lingeringTypes)}");
    }

    [Test]
    public void ReactivePlanConfig_rejects_double_registration()
    {
        ReactivePlanConfig.Reset();

        var extractor = new DummyExtractor();
        ReactivePlanConfig.UseFormValidationExtractor(extractor);

        Assert.Throws<InvalidOperationException>(() =>
            ReactivePlanConfig.UseFormValidationExtractor(extractor));

        ReactivePlanConfig.Reset();
    }

    [Test]
    public void Render_throws_when_validator_used_without_extractor()
    {
        ReactivePlanConfig.Reset();

        var plan = new ReactivePlan<TestModel>();
        var trigger = new Builders.TriggerBuilder<TestModel>(plan);
        trigger.DomReady(p =>
        {
            p.Post("/api/save")
             .Validate<FakeValidator>("my-form")
             .Response(r => r.OnSuccess(s => s.Dispatch("saved")));
        });

        var ex = Assert.Throws<InvalidOperationException>(() => plan.Render());
        Assert.That(ex!.Message, Does.Contain("UseFormValidationExtractor"));

        ReactivePlanConfig.Reset();
    }

    private class FakeValidator { }

    private class DummyExtractor : Validation.IFormValidationExtractor
    {
        public Validation.FormValidation? ExtractRules(Type validatorType, string formId)
        {
            return null;
        }
    }
}
