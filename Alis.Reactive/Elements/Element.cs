using System;
using Alis.Reactive.Declarations;
using Alis.Reactive.Schema;
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
    public override ComponentVendor Vendor => ComponentVendor.Native;

    /// <summary>Gets the element's text, <c>textContent</c>: a source of it that a step also sets.</summary>
    /// <remarks>
    /// A value that cannot be read shows nothing when it is set. A set is refused on an element a slot loads
    /// into, since it would remove what the slot loaded while that content's plan still runs.
    /// </remarks>
    public ElementContent Text => new(this, "textContent");

    /// <summary>Gets the element's HTML, <c>innerHTML</c>: a source of it that a step also sets.</summary>
    /// <remarks>
    /// A value that cannot be read shows nothing when it is set. A set is refused on an element a slot loads
    /// into, since it would remove what the slot loaded while that content's plan still runs.
    /// </remarks>
    public ElementContent Html => throw new NotImplementedException();

    /// <summary>Gets the element's classes, <c>classList</c>.</summary>
    public ClassList Classes => throw new NotImplementedException();
}

/// <summary>
/// An element's text or its HTML: a source of it that a step also sets, to a text or to a value shown as text.
/// </summary>
public sealed class ElementContent : ReadOnly<string?>
{
    private readonly Element _element;
    private readonly PathStep _member;

    internal ElementContent(Element element, string member)
    {
        _element = element;
        _member = new MemberStep(member);
    }

    /// <summary>Sets the content to a text when the step runs.</summary>
    /// <param name="text">The text, written into the plan where the view renders; null shows nothing.</param>
    public void Set(string? text)
        => _element.Bound.Writer.Write(new SetStatement([_member], SetMembers.Required, true, _element.Bound.Target, Literals.Of(text)));

    /// <summary>Sets the content to what a source reads when the step runs, shown as text.</summary>
    /// <remarks>
    /// A number shows as the server writes it under the view's culture, and a date as its day, with its time when
    /// that is not midnight. A source that cannot be read shows nothing.
    /// </remarks>
    /// <param name="source">The source of the value to show.</param>
    /// <typeparam name="TValue">The type of the value the source reads.</typeparam>
    public void Set<TValue>(ITypedSource<TValue> source) => throw new NotImplementedException();
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
