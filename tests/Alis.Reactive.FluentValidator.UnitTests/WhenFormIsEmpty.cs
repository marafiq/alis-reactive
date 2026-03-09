namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenFormIsEmpty
{
    private readonly FluentValidationAdapter _adapter = new();

    [Test]
    public void Empty_validator_returns_null()
    {
        var desc = _adapter.ExtractRules(typeof(EmptyValidator), "testForm");

        Assert.That(desc, Is.Null);
    }

    [Test]
    public void FormId_flows_through()
    {
        var desc = _adapter.ExtractRules(typeof(RequiredValidator), "mySpecialForm");

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc!.FormId, Is.EqualTo("mySpecialForm"));
    }
}
