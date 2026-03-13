using Alis.Reactive;

namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenExtractingEmailRule
{
    private readonly FluentValidationAdapter _adapter = new();
    private static readonly IReadOnlyDictionary<string, ComponentRegistration> _map = TestComponentsMap.ForTestModel();

    [Test]
    public void EmailAddress_produces_email_rule()
    {
        var desc = _adapter.ExtractRules(typeof(EmailValidator), "testForm", _map);

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc!.Fields[0].Rules[0].Rule, Is.EqualTo("email"));
        Assert.That(desc.Fields[0].Rules[0].Constraint, Is.Null);
    }

    [Test]
    public void Custom_message_flows_through()
    {
        var desc = _adapter.ExtractRules(typeof(EmailWithCustomMessageValidator), "testForm", _map);

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc!.Fields[0].Rules[0].Message, Is.EqualTo("Invalid email format."));
    }
}
