namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenFormIsEmpty
{
    private readonly FluentValidationAdapter _adapter = AdapterFactory.Create();

    [Test]
    public void Empty_validator_returns_empty_list()
    {
        var fields = _adapter.ProjectRules(typeof(EmptyValidator), "testForm");

        Assert.That(fields, Is.Empty);
    }
}
