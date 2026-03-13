using System.Reflection;

namespace Alis.Reactive.UnitTests;

[TestFixture]
public class WhenEnforcingArchitectureRules
{
    [Test]
    public void All_descriptor_classes_are_sealed()
    {
        var assembly = typeof(IReactivePlan<>).Assembly;
        var descriptorNamespace = "Alis.Reactive.Descriptors";

        var unsealed = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsNested
                        && t.Namespace != null && t.Namespace.StartsWith(descriptorNamespace))
            .Where(t => !t.IsSealed)
            .Select(t => t.FullName)
            .ToList();

        Assert.That(unsealed, Is.Empty,
            $"Unsealed descriptor classes: {string.Join(", ", unsealed)}");
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

    private class DummyExtractor : Validation.IValidationExtractor
    {
        public Validation.ValidationDescriptor? ExtractRules(Type validatorType, string formId)
        {
            return null;
        }
    }
}
