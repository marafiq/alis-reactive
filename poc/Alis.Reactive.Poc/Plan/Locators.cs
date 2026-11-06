using System.Text.Json.Serialization;

namespace Alis.Reactive.Plan;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(ElementLocator), "element")]
internal abstract class Locator
{
    private protected Locator()
    {
    }
}

internal sealed class ElementLocator : Locator
{
    internal ElementLocator(string id, string vendor)
    {
        Id = id;
        Vendor = vendor;
    }

    public string Id { get; }

    public string Vendor { get; }
}
