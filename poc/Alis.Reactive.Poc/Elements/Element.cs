using System.Diagnostics.CodeAnalysis;
using Alis.Reactive.Components;
using Alis.Reactive.Pipelines;
using Alis.Reactive.Values;

namespace Alis.Reactive.Elements;

[Mirrors("lib.dom", "HTMLElement")]
public sealed class Element : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public Property<string?> Text => Content<string?>("text", name: "textContent");

    [SuppressMessage("Alis.Reactive.Contract", "ALIS019", Justification = "The HTML standard's innerHTML setter is [LegacyNullToEmptyString]: null writes the empty text, which lib.dom does not type.")]
    public Property<string?> Html => Content<string?>("HTML", name: "innerHTML");

    public ClassList Classes => LiveObject<ClassList>(name: "classList");
}

public sealed class ClassList : JsObject
{
    public void Add(string? name) => MethodWhenHeld((Arg<string?>)name, "add");

    public void Add(ITypedSource<string?> name) => MethodWhenHeld(Arg<string?>.Of(name), "add");

    public void Remove(string? name) => MethodWhenHeld((Arg<string?>)name, "remove");

    public void Remove(ITypedSource<string?> name) => MethodWhenHeld(Arg<string?>.Of(name), "remove");

    public void Toggle(string? name) => MethodWhenHeld((Arg<string?>)name, "toggle");

    public void Toggle(ITypedSource<string?> name) => MethodWhenHeld(Arg<string?>.Of(name), "toggle");
}

public static class ElementSteps
{
    public static Element Element<TModel>(this Pipeline<TModel> pipeline, string id) => pipeline.Component<Element>(id);
}
