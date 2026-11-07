using Alis.Reactive.Components;
using Alis.Reactive.Pipelines;

namespace Alis.Reactive.Elements;

public sealed class Element : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public Property<string?> Text => Property<string?>(name: "textContent");
}

public static class ElementSteps
{
    public static Element Element(this Pipeline pipeline, string id) => pipeline.Component<Element>(id);
}
