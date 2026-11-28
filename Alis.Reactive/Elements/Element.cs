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
    internal const string IdMember = "id";

    /// <inheritdoc />
    public override ComponentVendor Vendor => ComponentVendor.Native;

    /// <summary>Gets the element's text, <c>textContent</c>: a source of it that a step also sets.</summary>
    /// <remarks>
    /// A value that cannot be read shows nothing when it is set. A set over an element that is or holds a slot a
    /// plan still holds is refused where the view renders when the same plan loads or unloads that slot, and stops
    /// its pipeline otherwise, since it would remove what the slot loaded while that content's plan runs.
    /// </remarks>
    public ElementContent Text => new(this, "textContent", "insertAdjacentText", nameof(Text), "text");

    /// <summary>Gets the element's HTML, <c>innerHTML</c>: a source of it that a step also sets.</summary>
    /// <remarks>
    /// A value that cannot be read shows nothing when it is set. A set over an element that is or holds a slot a
    /// plan still holds is refused where the view renders when the same plan loads or unloads that slot, and stops
    /// its pipeline otherwise, since it would remove what the slot loaded while that content's plan runs.
    /// </remarks>
    public ElementContent Html => new(this, "innerHTML", "insertAdjacentHTML", nameof(Html), "HTML");

    /// <summary>Gets the element's classes, <c>classList</c>.</summary>
    public ClassList Classes => LiveObject<ClassList>(name: "classList");

    /// <summary>Gets the element's child elements, <c>children</c>, which the list operations take as a list.</summary>
    public ElementChildren Children => LiveObject<ElementChildren>();

    /// <summary>Gets whether the element is hidden, <c>hidden</c>: a source of it that a step also sets.</summary>
    public Property<bool> Hidden => Property<bool>();

    /// <summary>Shows the element when the step runs, as setting <see cref="Hidden"/> to false does.</summary>
    /// <remarks>An element that already shows keeps showing.</remarks>
    public void Show() => Hidden.Set(false);

    /// <summary>Hides the element when the step runs, as setting <see cref="Hidden"/> to true does.</summary>
    /// <remarks>An element that is already hidden stays hidden.</remarks>
    public void Hide() => Hidden.Set(true);
}

/// <summary>
/// An element's text or its HTML: a source of it that a step also sets or adds to, with a text or a value shown as
/// text.
/// </summary>
public sealed partial class ElementContent : ReadOnly<string?>
{
    private const string AtTheEnd = "beforeend";

    private readonly string _addsBy;
    private readonly string _declaredBy;
    private readonly string _contentName;

    internal ElementContent(Element element, string member, string addsBy, string declaredBy, string contentName)
        : base(element, member, optional: false)
    {
        _addsBy = addsBy;
        _declaredBy = declaredBy;
        _contentName = contentName;
    }

    /// <summary>Sets the content to a text when the step runs.</summary>
    /// <param name="text">The text, written into the plan where the view renders; null shows nothing.</param>
    public void Set(string? text) => Written(Literals.Text(text));

    /// <summary>Sets the content to what a source reads when the step runs, shown as text.</summary>
    /// <remarks>
    /// A number shows as the server writes it under the view's culture, and a date as its day, with its time when
    /// that is not midnight. A source that cannot be read shows nothing.
    /// </remarks>
    /// <param name="source">The source of the value to show.</param>
    /// <typeparam name="TValue">The type of the value the source reads.</typeparam>
    public void Set<TValue>(ITypedSource<TValue> source) => Written(TextForms.Shown(source));

    /// <summary>Sets the content to a date and time when the step runs, shown as text.</summary>
    /// <remarks>
    /// It shows the wall time the view holds, whatever its kind, the same in every browser's zone: its day, with its
    /// time when that is not midnight.
    /// </remarks>
    /// <param name="value">The date and time, written into the plan where the view renders.</param>
    public void Set(DateTime value) => Written(Shown(value));

    /// <summary>Adds a text after the content when the step runs.</summary>
    /// <remarks>
    /// Nothing the element holds is removed: an element's text gains the text as a text of its own, and its HTML
    /// gains the elements the text parses to, so its child elements, a value a person typed and a component inside
    /// it stay. A slot a plan holds takes it too, and what it adds there leaves at that slot's next load or unload.
    /// </remarks>
    /// <param name="text">The text, written into the plan where the view renders; null or an empty text adds nothing.</param>
    public void Append(string? text) => Added(TextForms.Part(text));

    /// <summary>Adds what a source reads after the content when the step runs, shown as text.</summary>
    /// <remarks>
    /// A number shows as the server writes it under the view's culture, and a date as its day, with its time when
    /// that is not midnight. A source that cannot be read adds nothing. The value is added as a text is, and nothing
    /// the element holds is removed.
    /// </remarks>
    /// <param name="source">The source of the value to add.</param>
    /// <typeparam name="TValue">The type of the value the source reads.</typeparam>
    public void Append<TValue>(ITypedSource<TValue> source) => Added(TextForms.Part(source));

    private AsExpression Shown<TValue>(TValue value)
    {
        var content = Owner.Bound.Writer.Content;
        var literal = Literals.IntoLive(value, content.Json, Owner.DeclaredName(_declaredBy));
        return TextForms.Shown(new Computed<TValue>(literal, HeldForm.Live(content.Json, content.RendersUnder)));
    }

    private void Written(Expression value)
    {
        var bound = Owner.Bound;
        if (bound.Target is ObjExpression element) bound.Writer.Content.SetsContentOf(element.Id, _contentName);
        bound.Writer.Write(new SetStatement([Member], SetMembers.Required, true, bound.Target, value));
    }

    private void Added(Expression part)
    {
        var writer = Owner.Bound.Writer;
        var target = Owner.Bound.Target;
        var element = writer.NextBinding("element");
        var text = writer.NextBinding("text");
        writer.Write(new LetStatement(element.Name, false, LetBinds.Line, target));
        writer.Write(new LetStatement(text.Name, false, LetBinds.Line, TextForms.Joined([part])));
        var adds = new DoStatement(false, new CallExpression([new MemberStep(_addsBy)], Owner.Bound.Reads(false), element, [Literals.Text(AtTheEnd), text]));
        var textHoldsSomething = new OpExpression(Operator.Neq, [text, Literals.Text(string.Empty)]);
        writer.Write(GuardedWrites.Writing(new IfStatement(textHoldsSomething, [adds], []), target));
    }
}

/// <summary>
/// An element's classes: a step adds, removes or toggles one by its name, and the list operations read its
/// <see cref="Items"/>.
/// </summary>
public sealed class ClassList : JsObject
{
    /// <summary>Gets a source of the element's class names, read when a list operation runs.</summary>
    public TypedSource<IReadOnlyList<string>> Items => DataIterator<string>();

    /// <summary>Adds a class to the element.</summary>
    /// <param name="name">The class's name; null changes no class.</param>
    public void Add(string? name) => Changed("add", Literals.Text(name));

    /// <summary>Adds the class a source names when the step runs.</summary>
    /// <param name="name">The source of the class's name; one that holds null or cannot be read changes no class.</param>
    public void Add(ITypedSource<string?> name) => Changed("add", Sources.Of(name));

    /// <summary>Removes a class from the element.</summary>
    /// <param name="name">The class's name; null changes no class.</param>
    public void Remove(string? name) => Changed("remove", Literals.Text(name));

    /// <summary>Removes the class a source names when the step runs.</summary>
    /// <param name="name">The source of the class's name; one that holds null or cannot be read changes no class.</param>
    public void Remove(ITypedSource<string?> name) => Changed("remove", Sources.Of(name));

    /// <summary>Adds a class the element lacks, or removes one it has.</summary>
    /// <param name="name">The class's name; null changes no class.</param>
    public void Toggle(string? name) => Changed("toggle", Literals.Text(name));

    /// <summary>Adds or removes the class a source names when the step runs.</summary>
    /// <param name="name">The source of the class's name; one that holds null or cannot be read changes no class.</param>
    public void Toggle(ITypedSource<string?> name) => Changed("toggle", Sources.Of(name));

    private void Changed(string method, Expression name)
    {
        var writer = Bound.Writer;
        var target = Bound.Target;
        var className = writer.NextBinding("className");
        writer.Write(new LetStatement(className.Name, false, LetBinds.Line, name));
        var change = new DoStatement(false, new CallExpression([new MemberStep(method)], Bound.Reads(false), target, [className]));
        var nameHoldsAValue = new OpExpression(Operator.Neq, [className, Literals.Text(null)]);
        writer.Write(GuardedWrites.Writing(new IfStatement(nameHoldsAValue, [change], []), target));
    }
}

/// <summary>
/// An element's child elements, whose <see cref="Items"/> the list operations read.
/// </summary>
[Mirrors("lib.dom", "HTMLCollection")]
public sealed class ElementChildren : JsObject
{
    /// <summary>Gets the child elements the element holds when a list operation runs.</summary>
    public LiveObjects<ChildElement> Items => LiveIterator<ChildElement>();
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
