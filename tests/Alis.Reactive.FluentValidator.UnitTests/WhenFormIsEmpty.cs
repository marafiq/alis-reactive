using Alis.Reactive;

namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenFormIsEmpty
{
    private readonly FluentValidationAdapter _adapter = new();
    private static readonly IReadOnlyDictionary<string, ComponentRegistration> _map = TestComponentsMap.ForTestModel();

    [Test]
    public void Empty_validator_returns_null()
    {
        var desc = _adapter.ExtractRules(typeof(EmptyValidator), "testForm", _map);

        Assert.That(desc, Is.Null);
    }

    [Test]
    public void FormId_flows_through()
    {
        var desc = _adapter.ExtractRules(typeof(RequiredValidator), "mySpecialForm", _map);

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc!.FormId, Is.EqualTo("mySpecialForm"));
    }
}
