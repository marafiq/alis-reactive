using System;
using System.Collections.Generic;
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
    /// A value that cannot be read shows nothing when it is set. A set over an element that is or holds a slot a
    /// plan still holds is refused where the view renders when the same plan loads that slot, and stops its
    /// pipeline otherwise, since it would remove what the slot loaded while that content's plan runs.
    /// </remarks>
    public ElementContent Text => new(this, "textContent");

    /// <summary>Gets the element's HTML, <c>innerHTML</c>: a source of it that a step also sets.</summary>
    /// <remarks>
    /// A value that cannot be read shows nothing when it is set. A set over an element that is or holds a slot a
    /// plan still holds is refused where the view renders when the same plan loads that slot, and stops its
    /// pipeline otherwise, since it would remove what the slot loaded while that content's plan runs.
    /// </remarks>
    public ElementContent Html => throw new NotImplementedException();

    /// <summary>Gets the element's classes, <c>classList</c>.</summary>
    public ClassList Classes => throw new NotImplementedException();

    /// <summary>Gets the element's child elements, <c>children</c>, which the list operations take as a list.</summary>
    public ElementChildren Children => throw new NotImplementedException();

    /// <summary>Gets whether the element is hidden, <c>hidden</c>: a source of it that a step also sets.</summary>
    public Property<bool> Hidden => throw new NotImplementedException();

    /// <summary>Shows the element when the step runs, as setting <see cref="Hidden"/> to false does.</summary>
    /// <remarks>An element that already shows keeps showing.</remarks>
    public void Show() => throw new NotImplementedException();

    /// <summary>Hides the element when the step runs, as setting <see cref="Hidden"/> to true does.</summary>
    /// <remarks>An element that is already hidden stays hidden.</remarks>
    public void Hide() => throw new NotImplementedException();
}

/// <summary>
/// An element's text or its HTML: a source of it that a step also sets or adds to, with a text or a value shown as
/// text.
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

    /// <summary>Adds a text after the content when the step runs.</summary>
    /// <remarks>
    /// Nothing the element holds is removed: an element's text gains the text as a text of its own, and its HTML
    /// gains the elements the text parses to, so its child elements, a value a person typed and a component inside
    /// it stay. A slot a plan holds takes it too, and what it adds there leaves at that slot's next load or unload.
    /// </remarks>
    /// <param name="text">The text, written into the plan where the view renders; null or an empty text adds nothing.</param>
    public void Append(string? text) => throw new NotImplementedException();

    /// <summary>Adds what a source reads after the content when the step runs, shown as text.</summary>
    /// <remarks>
    /// A number shows as the server writes it under the view's culture, and a date as its day, with its time when
    /// that is not midnight. A source that cannot be read adds nothing. The value is added as a text is, and nothing
    /// the element holds is removed.
    /// </remarks>
    /// <param name="source">The source of the value to add.</param>
    /// <typeparam name="TValue">The type of the value the source reads.</typeparam>
    public void Append<TValue>(ITypedSource<TValue> source) => throw new NotImplementedException();
}

/// <summary>
/// An element's classes: a step adds, removes or toggles one by its name.
/// </summary>
public sealed class ClassList : JsObject, IDataIterable<string>
{
    TypedSource<IReadOnlyList<string>> IDataIterable<string>.Items => DataIterator<string>();

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

/// <summary>
/// An element's child elements, which the list operations read as the elements it holds when they run.
/// </summary>
[Mirrors("lib.dom", "HTMLCollection")]
public sealed class ElementChildren : JsObject, ILiveIterable<ChildElement>
{
    LiveObjects<ChildElement> ILiveIterable<ChildElement>.Items => LiveIterator<ChildElement>();
}

/// <summary>
/// A child element of an element, whose attributes a list operation reads.
/// </summary>
[Mirrors("lib.dom", "Element")]
public sealed class ChildElement : JsObject
{
    /// <summary>Reads the value of an attribute.</summary>
    /// <param name="name">The attribute's name.</param>
    /// <returns>The attribute's value: null when the element holds no such attribute, and an empty text for one written with no value.</returns>
    public MethodResult<string?> GetAttribute(Arg<string> name) => Method<string?>([name]);

    /// <summary>Tests that the element holds an attribute.</summary>
    /// <param name="name">The attribute's name.</param>
    /// <returns>A source that is true when the element holds the attribute, with a value or with none.</returns>
    public MethodResult<bool> HasAttribute(Arg<string> name) => Method<bool>([name]);
}
