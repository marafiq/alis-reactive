namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenExtractingEmailRule
{
    private readonly FluentValidationRuleExtractor _ruleExtractor = RuleExtractorFactory.Create();

    [Test]
    public void EmailAddress_produces_email_rule()
    {
        var desc = _ruleExtractor.ExtractRules(typeof(EmailValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc!.Fields[0].Rules[0].Rule, Is.EqualTo("email"));
        Assert.That(desc.Fields[0].Rules[0].Constraint, Is.Null);
    }

    [Test]
    public void Custom_message_flows_through()
    {
        var desc = _ruleExtractor.ExtractRules(typeof(EmailWithCustomMessageValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc!.Fields[0].Rules[0].Message, Is.EqualTo("Invalid email format."));
    }
}
