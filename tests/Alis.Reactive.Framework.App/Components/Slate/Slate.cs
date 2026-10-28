namespace Alis.Reactive.Framework.App.Components;

/// <summary>
/// A board that holds a title, a note, a header and rows, and shows them in its own markup (slate.ts). It
/// keeps itself on its element as <c>slate</c>, takes listeners through <c>on</c> and <c>off</c>, and hands a
/// listener the event's payload.
/// </summary>
public sealed class Slate : Component
{
    public static ComponentVendor SlateVendor { get; } = ComponentVendor.Define(
        "slate",
        objectPath: "slate",
        subscribe: "on",
        unsubscribe: "off",
        payload: VendorEventPayload.Argument);

    public override ComponentVendor Vendor => SlateVendor;

    /// <summary>How many rows the slate holds; the slate counts them, nothing writes it.</summary>
    public ReadOnly<int> Count => ReadOnly<int>();

    /// <summary>A note the slate shows under its title; it can be written, never read back.</summary>
    public WriteOnly<string> Note => WriteOnly<string>();

    /// <summary>The slate's title, shown as its heading.</summary>
    public Property<string> Title => Property<string>();

    /// <summary>The header above the title: an object the slate holds, with members of its own.</summary>
    public SlateHeader Header => Object<SlateHeader>();

    /// <summary>The slate's rows, in order.</summary>
    public Objects<SlateRow> Rows => Objects<SlateRow>();

    /// <summary>The row at a position, as the object the slate returns for it.</summary>
    public SlateRow RowAt(Arg<int> index) => Object<SlateRow>(index);

    /// <summary>The rows marked so far, as a list the slate returns.</summary>
    public Objects<SlateRow> MarkedRows() => Objects<SlateRow>();

    /// <summary>Shows the labels it is given as the slate's summary line.</summary>
    public void Summarize(Arg<IReadOnlyList<string>> labels) => Method(labels);
}

public sealed class SlateHeader : JsObject
{
    public Property<string> Caption => Property<string>();

    public void Rename(Arg<string> caption) => Method(caption);
}

public sealed class SlateRow : JsObject
{
    public ReadOnly<string> Label => ReadOnly<string>();

    public ReadOnly<bool> Marked => ReadOnly<bool>();

    public void Mark() => Method();
}
