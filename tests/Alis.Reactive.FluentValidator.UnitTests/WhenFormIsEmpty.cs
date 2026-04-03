namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenFormIsEmpty
{
    private readonly FluentValidationRuleExtractor _ruleExtractor = RuleExtractorFactory.Create();

    [Test]
    public void Empty_validator_returns_null()
    {
        var desc = _ruleExtractor.ExtractRules(typeof(EmptyValidator), "testForm");

        Assert.That(desc, Is.Null);
    }

    [Test]
    public void FormId_flows_through()
    {
        var desc = _ruleExtractor.ExtractRules(typeof(RequiredValidator), "mySpecialForm");

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc!.FormId, Is.EqualTo("mySpecialForm"));
    }
}
