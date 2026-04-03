namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenFailingFastOnBrokenNestedValidators
{
    [Test]
    public void Throws_when_nested_validator_cannot_be_created()
    {
        // Factory that returns null for nested validators
        var extractor = new FluentValidationRuleExtractor(type =>
        {
            if (type == typeof(BrokenNestedValidator))
                return new BrokenNestedValidator();
            return null; // Nested validator factory fails
        });

        Assert.Throws<InvalidOperationException>(() =>
            extractor.ExtractRules(typeof(BrokenNestedValidator), "testForm"));
    }

    [Test]
    public void Throws_when_factory_throws_for_nested_validator()
    {
        var extractor = new FluentValidationRuleExtractor(type =>
        {
            if (type == typeof(NestedValidator))
                return new NestedValidator();
            if (type == typeof(TestAddressValidator))
                throw new Exception("DI container missing registration");
            return null;
        });

        Assert.Throws<InvalidOperationException>(() =>
            extractor.ExtractRules(typeof(NestedValidator), "testForm"));
    }
}
