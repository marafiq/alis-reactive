namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenExtractingEqualToRules
{
    private readonly FluentValidationAdapter _adapter = AdapterFactory.Create();

    [Test]
    public void Equal_to_other_field_extracts_equalTo_with_field()
    {
        var desc = _adapter.ExtractRules(typeof(EqualToValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var field = desc.First(f => f.FieldName == "ConfirmEmail");
        var equalRule = field.Rules.First(r => r.Rule == "equalTo");
        Assert.That(equalRule.PeerFieldName(), Is.EqualTo("Email"));
        Assert.That(equalRule.ConstraintValue(), Is.Null);
        Assert.That(equalRule.Shape.Kind, Is.EqualTo("string"));
    }

    [Test]
    public void Equal_to_with_custom_message_uses_custom_message()
    {
        var desc = _adapter.ExtractRules(typeof(EqualToWithCustomMessageValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var field = desc.First(f => f.FieldName == "ConfirmEmail");
        var equalRule = field.Rules.First(r => r.Rule == "equalTo");
        Assert.That(equalRule.Message, Is.EqualTo("Emails must match."));
        Assert.That(equalRule.PeerFieldName(), Is.EqualTo("Email"));
    }

    [Test]
    public void Equal_to_literal_null_extracts_explicit_null_constraint()
    {
        var desc = _adapter.ExtractRules(typeof(LiteralNullComparisonValidator), "testForm");

        var field = desc.First(f => f.FieldName == "MiddleName");
        var equalRule = field.Rules.First(r => r.Rule == "equalTo");

        Assert.That(equalRule.HasConstraintOperand(), Is.True);
        Assert.That(equalRule.ConstraintValue(), Is.Null);
        Assert.That(equalRule.Shape, Is.EqualTo(Alis.Reactive.PlanModel.Shape.None));
    }

    [Test]
    public void Not_equal_literal_null_extracts_explicit_null_constraint()
    {
        var desc = _adapter.ExtractRules(typeof(LiteralNullComparisonValidator), "testForm");

        var field = desc.First(f => f.FieldName == "JobTitle");
        var notEqualRule = field.Rules.First(r => r.Rule == "notEqual");

        Assert.That(notEqualRule.HasConstraintOperand(), Is.True);
        Assert.That(notEqualRule.ConstraintValue(), Is.Null);
        Assert.That(notEqualRule.Shape, Is.EqualTo(Alis.Reactive.PlanModel.Shape.None));
    }
}
