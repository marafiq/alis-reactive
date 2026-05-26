namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenProjectingEmailRule
{
    private readonly FluentValidationAdapter _adapter = AdapterFactory.Create();

    [Test]
    public void EmailAddress_produces_email_rule()
    {
        var desc = _adapter.ProjectRules(typeof(EmailValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc[0].Rules[0].Kind, Is.EqualTo(ValidationRuleKind.Email));
        Assert.That(desc[0].Rules[0].ConstraintValue, Is.Null);
    }

    [Test]
    public void Custom_message_flows_through()
    {
        var desc = _adapter.ProjectRules(typeof(EmailWithCustomMessageValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc[0].Rules[0].Message, Is.EqualTo("Invalid email format."));
    }
}
