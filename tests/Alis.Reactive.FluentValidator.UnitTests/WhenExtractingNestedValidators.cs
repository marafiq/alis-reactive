using Alis.Reactive;

namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenExtractingNestedValidators
{
    private readonly FluentValidationAdapter _adapter = new();

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
    public void Nested_field_ids_use_underscore_separator()
    {
        var desc = _adapter.ExtractRules(typeof(NestedValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var streetField = desc!.Fields.First(f => f.FieldName == "Address.Street");
        var scope = IdGenerator.TypeScope(typeof(TestModel));
        Assert.That(streetField.FieldId, Is.EqualTo(scope + "__Address_Street"));
    }

    [Test]
    public void Deeply_nested_produces_correct_paths()
    {
        var desc = _adapter.ExtractRules(typeof(DeeplyNestedValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        var fieldNames = desc!.Fields.Select(f => f.FieldName).ToList();
        Assert.That(fieldNames, Does.Contain("DeepAddress.Street"));
        Assert.That(fieldNames, Does.Contain("DeepAddress.Country.Code"));

        var codeField = desc.Fields.First(f => f.FieldName == "DeepAddress.Country.Code");
        var scope = IdGenerator.TypeScope(typeof(TestModel));
        Assert.That(codeField.FieldId, Is.EqualTo(scope + "__DeepAddress_Country_Code"));
    }
}
