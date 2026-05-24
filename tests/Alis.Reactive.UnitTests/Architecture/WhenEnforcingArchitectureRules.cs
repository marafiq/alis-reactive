namespace Alis.Reactive.UnitTests;

[TestFixture]
public class WhenEnforcingArchitectureRules
{
    [Test]
    public void All_plan_model_classes_are_sealed()
    {
        var assembly = typeof(ReactivePlan<>).Assembly;
        var planModelNamespace = "Alis.Reactive.PlanModel";

        var unsealed = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsNested
                        && t.Namespace != null && t.Namespace.StartsWith(planModelNamespace))
            .Where(t => !t.IsSealed)
            .Select(t => t.FullName)
            .ToList();

        Assert.That(unsealed, Is.Empty,
            $"Unsealed plan model classes: {string.Join(", ", unsealed)}");
    }

    [Test]
    public void ReactivePlanConfig_rejects_double_registration()
    {
        ReactivePlanConfig.Reset();

        var extractor = new DummyExtractor();
        ReactivePlanConfig.UseValidationExtractor(extractor);

        Assert.Throws<InvalidOperationException>(() =>
            ReactivePlanConfig.UseValidationExtractor(extractor));

        ReactivePlanConfig.Reset();
    }

    [Test]
    public void Render_throws_when_validator_used_without_extractor()
    {
        ReactivePlanConfig.Reset();

        var plan = new ReactivePlan<TestModel>();
        var trigger = new Builders.TriggerBuilder<TestModel>(plan, plan.Context);
        trigger.DomReady(p =>
        {
            p.Post("/api/save")
             .Validate<FakeValidator>("my-form")
             .Response(r => r.OnSuccess(s => s.Dispatch("saved")));
        });

        var ex = Assert.Throws<InvalidOperationException>(() => plan.Render());
        Assert.That(ex!.Message, Does.Contain("UseValidationExtractor"));

        ReactivePlanConfig.Reset();
    }

    private class FakeValidator { }

    private class DummyExtractor : Validation.IValidationExtractor
    {
        public Validation.ValidationExtractionReport Extract(Validation.ValidationExtractionRequest request)
        {
            return Validation.ValidationExtractionReport.ForClientFields(
                request,
                new List<Validation.ValidationField>());
        }
    }
}
