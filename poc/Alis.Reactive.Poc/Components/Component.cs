using Alis.Reactive.Plan;

namespace Alis.Reactive.Components;

public abstract class Component : JsObject
{
    public abstract ComponentVendor Vendor { get; }
}

public sealed class ComponentVendor
{
    private ComponentVendor(string name, Vendor grammar)
    {
        Name = name;
        Grammar = grammar;
    }

    public static ComponentVendor Native { get; } = new(
        "native",
        new Vendor(
            new FnExpression(["element"], new RefExpression("element")),
            new MethodListening("addEventListener", "removeEventListener")));

    public string Name { get; }

    internal Vendor Grammar { get; }

    public override string ToString() => Name;
}
