namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenExtractingEqualToRules
{
    private readonly FluentValidationAdapter _adapter = AdapterFactory.Create();

    [Test]
    public void Equal_to_other_field_extracts_equalTo_rule()
    {
        var desc = _adapter.ExtractRules(typeof(EqualToValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var field = desc!.Fields.First(f => f.FieldName == "ConfirmEmail");
        var equalRule = field.Rules.First(r => r.Rule == "equalTo");
        Assert.That(equalRule.Constraint, Is.EqualTo("Email"));
    }

    [Test]
    public void Equal_to_with_custom_message_uses_custom_message()
    {
        var desc = _adapter.ExtractRules(typeof(EqualToWithCustomMessageValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var field = desc!.Fields.First(f => f.FieldName == "ConfirmEmail");
        var equalRule = field.Rules.First(r => r.Rule == "equalTo");
        Assert.That(equalRule.Message, Is.EqualTo("Emails must match."));
        Assert.That(equalRule.Constraint, Is.EqualTo("Email"));
    }
}
