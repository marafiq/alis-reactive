using System.Text.Json.Serialization;

namespace Alis.Reactive.Plan;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(ElementLocator), "element")]
[JsonDerivedType(typeof(PluginLocator), "plugin")]
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

internal sealed class PluginLocator : Locator
{
    internal PluginLocator(string name, Listening listen)
    {
        Name = name;
        Listen = listen;
    }

    public string Name { get; }

    public Listening Listen { get; }
}
