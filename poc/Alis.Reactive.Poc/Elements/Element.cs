using System.Diagnostics.CodeAnalysis;
using Alis.Reactive.Components;
using Alis.Reactive.Pipelines;
using Alis.Reactive.Values;

namespace Alis.Reactive.Elements;

[Mirrors("lib.dom", "HTMLElement")]
public sealed class Element : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public Property<string?> Text => Property<string?>(name: "textContent");

    [SuppressMessage("Alis.Reactive.Contract", "ALIS019", Justification = "The HTML standard's innerHTML setter is [LegacyNullToEmptyString]: null writes the empty text, which lib.dom does not type.")]
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
