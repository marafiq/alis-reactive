namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenProjectingRegexRule
{
    private readonly FluentValidationAdapter _adapter = AdapterFactory.Create();

    [Test]
    public void Matches_produces_regex_rule_with_pattern_constraint()
    {
        var desc = _adapter.ProjectRules(typeof(RegexValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc[0].Rules[0].Kind, Is.EqualTo(ValidationRuleKind.Regex));
        Assert.That(desc[0].Rules[0].LiteralOperand().Value, Is.EqualTo(@"^\d{3}-\d{3}-\d{4}$"));
        Assert.That(desc[0].Rules[0].Message, Does.Contain("format is invalid"));
    }
}
