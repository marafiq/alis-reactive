namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenProjectingRequiredRules
{
    private readonly FluentValidationAdapter _adapter = AdapterFactory.Create();

    [Test]
    public void NotEmpty_produces_required_rule()
    {
        var desc = _adapter.ProjectRules(typeof(RequiredValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc, Has.Count.EqualTo(1));
        Assert.That(desc[0].FieldName, Is.EqualTo("Name"));
        Assert.That(desc[0].Rules, Has.Count.EqualTo(1));
        Assert.That(desc[0].Rules[0].Kind, Is.EqualTo(ValidationRuleKind.Required));
        Assert.That(desc[0].Rules[0].Message, Is.EqualTo("'Name' is required."));
        Assert.That(desc[0].Rules[0].ConstraintValue, Is.Null);
    }

    [Test]
    public void NotNull_produces_required_rule()
    {
        var desc = _adapter.ProjectRules(typeof(NotNullValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc[0].Rules[0].Kind, Is.EqualTo(ValidationRuleKind.Required));
    }

    [Test]
    public void Custom_message_overrides_default()
    {
        var desc = _adapter.ProjectRules(typeof(RequiredWithCustomMessageValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc[0].Rules[0].Message, Is.EqualTo("Name cannot be blank."));
    }
}
