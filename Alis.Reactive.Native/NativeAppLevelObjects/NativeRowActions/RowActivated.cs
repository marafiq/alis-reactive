using Alis.Reactive.Declarations;
using Alis.Reactive.Elements;

namespace Alis.Reactive.Native.NativeAppLevelObjects;

/// <summary>
/// What a row action's steps receive each time a person activates one of its controls.
/// </summary>
/// <typeparam name="TRow">The type of a row.</typeparam>
public sealed class RowActivated<TRow> : JsObject
{
    /// <summary>Gets the row the activated control was rendered for, holding the members the action's steps read.</summary>
    public ReadOnly<TRow> Row => ReadOnly<TRow>();

    /// <summary>Gets the element of the activated control.</summary>
    public Element Element => LiveObject<Element>();
}
