using System;
using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.Elements;

/// <summary>
/// An element of the page, found by the id the view gave it: its text, its HTML and its classes.
/// </summary>
/// <remarks>
/// An element the page lacks stops the pipeline that reaches it.
/// </remarks>
[Mirrors("lib.dom", "HTMLElement")]
public sealed class Element : Component
{
    /// <inheritdoc />
    public override ComponentVendor Vendor => throw new NotImplementedException();

    /// <summary>Gets the element's text, <c>textContent</c>: a source of it that a step also sets.</summary>
    /// <remarks>
    /// A value that cannot be read shows nothing when it is set. A set is refused on an element a slot loads
    /// into, since it would remove what the slot loaded while that content's plan still runs.
    /// </remarks>
    public Property<string?> Text => throw new NotImplementedException();

    /// <summary>Gets the element's HTML, <c>innerHTML</c>: a source of it that a step also sets.</summary>
    /// <remarks>
    /// A value that cannot be read shows nothing when it is set. A set is refused on an element a slot loads
    /// into, since it would remove what the slot loaded while that content's plan still runs.
    /// </remarks>
    public Property<string?> Html => throw new NotImplementedException();

    /// <summary>Gets the element's classes, <c>classList</c>.</summary>
    public ClassList Classes => throw new NotImplementedException();
}

/// <summary>
/// An element's classes: a step adds, removes or toggles one by its name.
/// </summary>
public sealed class ClassList : JsObject
{
    /// <summary>Adds a class to the element.</summary>
    /// <param name="name">The class's name; null changes no class.</param>
    public void Add(string? name) => throw new NotImplementedException();

    /// <summary>Adds the class a source names when the step runs.</summary>
    /// <param name="name">The source of the class's name; one that holds null or cannot be read changes no class.</param>
    public void Add(ITypedSource<string?> name) => throw new NotImplementedException();

    /// <summary>Removes a class from the element.</summary>
    /// <param name="name">The class's name; null changes no class.</param>
    public void Remove(string? name) => throw new NotImplementedException();

    /// <summary>Removes the class a source names when the step runs.</summary>
    /// <param name="name">The source of the class's name; one that holds null or cannot be read changes no class.</param>
    public void Remove(ITypedSource<string?> name) => throw new NotImplementedException();

    /// <summary>Adds a class the element lacks, or removes one it has.</summary>
    /// <param name="name">The class's name; null changes no class.</param>
    public void Toggle(string? name) => throw new NotImplementedException();

    /// <summary>Adds or removes the class a source names when the step runs.</summary>
    /// <param name="name">The source of the class's name; one that holds null or cannot be read changes no class.</param>
    public void Toggle(ITypedSource<string?> name) => throw new NotImplementedException();
}
