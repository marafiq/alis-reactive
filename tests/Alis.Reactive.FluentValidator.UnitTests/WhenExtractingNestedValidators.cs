namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenExtractingNestedValidators
{
    private readonly FluentValidationAdapter _adapter = AdapterFactory.Create();

    [Test]
    public void Nested_properties_use_dotted_field_names()
    {
        var desc = _adapter.ExtractRules(typeof(NestedValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var fieldNames = desc!.Fields.Select(f => f.FieldName).ToList();
        Assert.That(fieldNames, Does.Contain("Name"));
        Assert.That(fieldNames, Does.Contain("Address.Street"));
        Assert.That(fieldNames, Does.Contain("Address.City"));
        Assert.That(fieldNames, Does.Contain("Address.ZipCode"));
    }

    [Test]
    public void Deeply_nested_produces_correct_paths()
    {
        var desc = _adapter.ExtractRules(typeof(DeeplyNestedValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var fieldNames = desc!.Fields.Select(f => f.FieldName).ToList();
        Assert.That(fieldNames, Does.Contain("DeepAddress.Street"));
        Assert.That(fieldNames, Does.Contain("DeepAddress.Country.Code"));
    }
}
