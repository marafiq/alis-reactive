using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenExtractingRequiredRules
{
    private readonly FluentValidationAdapter _adapter = new();

    [Test]
    public void NotEmpty_produces_required_rule()
    {
        var desc = _adapter.ExtractRules(typeof(RequiredValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc!.Fields, Has.Count.EqualTo(1));
        Assert.That(desc.Fields[0].FieldName, Is.EqualTo("Name"));
        Assert.That(desc.Fields[0].Rules, Has.Count.EqualTo(1));
        Assert.That(desc.Fields[0].Rules[0].Rule, Is.EqualTo("required"));
        Assert.That(desc.Fields[0].Rules[0].Message, Is.EqualTo("'Name' is required."));
        Assert.That(desc.Fields[0].Rules[0].Constraint, Is.Null);
    }

    [Test]
    public void NotNull_produces_required_rule()
    {
        var desc = _adapter.ExtractRules(typeof(NotNullValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc!.Fields[0].Rules[0].Rule, Is.EqualTo("required"));
    }

    [Test]
    public void Custom_message_overrides_default()
    {
        var desc = _adapter.ExtractRules(typeof(RequiredWithCustomMessageValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc!.Fields[0].Rules[0].Message, Is.EqualTo("Name cannot be blank."));
    }
}
