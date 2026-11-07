using Alis.Reactive.Components;
using Alis.Reactive.Pipelines;
using Alis.Reactive.Values;

namespace Alis.Reactive.Elements;

public sealed class Element : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public Property<string?> Text => Property<string?>(name: "textContent");

    public Property<string?> Html => Property<string?>(name: "innerHTML");

    public ClassList Classes => LiveObject<ClassList>(name: "classList");
}

public sealed class ClassList : JsObject
{
    public void Add(Arg<string?> name) => MethodWhenHeld(name, "add");

    public void Remove(Arg<string?> name) => MethodWhenHeld(name, "remove");

    public void Toggle(Arg<string?> name) => MethodWhenHeld(name, "toggle");
}

public static class ElementSteps
{
    public static Element Element<TModel>(this Pipeline<TModel> pipeline, string id) => pipeline.Component<Element>(id);
}
