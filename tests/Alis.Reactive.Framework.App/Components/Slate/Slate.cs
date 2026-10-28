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

    /// <summary>Takes every row off the slate.</summary>
    public void Clear() => Method();

    /// <summary>Shows a placed line of four values, and adds the number to the slate's total.</summary>
    public void Place(Arg<string> text, Arg<decimal> number, Arg<bool> flag, Arg<DateTime> day) => Method(text, number, flag, day);

    /// <summary>Shows a card: whose it is, and the day it is due.</summary>
    public void Fill(Arg<SlateCard> card) => Method(card);

    /// <summary>Whether a row carries the label.</summary>
    public TypedSource<bool> Has(Arg<string> label) => Method<bool>(label);

    /// <summary>The sum of every number placed so far.</summary>
    public TypedSource<decimal> Total() => Method<decimal>();

    /// <summary>Where a save stands: idle, saving, or saved.</summary>
    public ReadOnly<string> Status => ReadOnly<string>();

    /// <summary>Saves the slate; finishes a moment later.</summary>
    public void Save() => MethodAsync();

    /// <summary>Finds a row by its label, a moment later.</summary>
    public TypedSource<string> Lookup(Arg<string> label) => MethodAsync<string>(label);

    /// <summary>Refuses, by throwing.</summary>
    public void Fail() => Method();

    /// <summary>Refuses a moment later, by rejecting.</summary>
    public void FailLater() => MethodAsync();
}

/// <summary>A card the slate shows: whose it is and the day it is due.</summary>
public sealed class SlateCard
{
    public string Name { get; set; } = "";

    public DateTime Due { get; set; }
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
