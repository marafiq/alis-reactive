using Alis.Reactive;

namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenExtractingRegexRule
{
    private readonly FluentValidationAdapter _adapter = new();
    private static readonly IReadOnlyDictionary<string, ComponentRegistration> _map = TestComponentsMap.ForTestModel();

    [Test]
    public void Matches_produces_regex_rule_with_pattern_constraint()
    {
        var desc = _adapter.ExtractRules(typeof(RegexValidator), "testForm", _map);

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc!.Fields[0].Rules[0].Rule, Is.EqualTo("regex"));
        Assert.That(desc.Fields[0].Rules[0].Constraint, Is.EqualTo(@"^\d{3}-\d{3}-\d{4}$"));
        Assert.That(desc.Fields[0].Rules[0].Message, Does.Contain("format is invalid"));
    }
}
