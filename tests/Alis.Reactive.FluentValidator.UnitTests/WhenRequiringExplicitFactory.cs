namespace Alis.Reactive.FluentValidator.UnitTests;

[TestFixture]
public class WhenRequiringExplicitFactory
{
    [Test]
    public void Null_factory_throws()
    {
        Assert.Throws<ArgumentException>(() => new FluentValidationAdapter(null!));
    }

    [Test]
    public void Explicit_factory_works()
    {
        var adapter = new FluentValidationAdapter(type =>
            Activator.CreateInstance(type) as FluentValidation.IValidator);

        var desc = adapter.ExtractRules(typeof(RequiredValidator), "testForm");

        Assert.That(desc, Is.Not.Null);
        Assert.That(desc!.Fields[0].FieldName, Is.EqualTo("Name"));
    }
}
