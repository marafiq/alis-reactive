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

    /// <summary>The row at a position counted from the first row, or from the last, as the object the slate returns for it.</summary>
    public SlateRow RowFrom(Arg<int> index, Arg<bool> fromLast) => Object<SlateRow>(index, fromLast);

    /// <summary>The rows marked so far, as a list the slate returns.</summary>
    public Objects<SlateRow> MarkedRows() => Objects<SlateRow>();

    /// <summary>The labels of a run of rows, from a position, joined by commas.</summary>
    public TypedSource<string> LabelsFrom(Arg<int> start, Arg<int> count) => Method<string>(start, count);

    /// <summary>The labels of a run of rows, from a position, joined by the separator given.</summary>
    public TypedSource<string> LabelsJoined(Arg<int> start, Arg<int> count, Arg<string> separator) => Method<string>(start, count, separator);

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

    /// <summary>Saves the slate and says how many rows it saved; finishes a moment later.</summary>
    public TypedSource<int> SaveRows() => MethodAsync<int>();

    /// <summary>Starts the slate's save and goes on without waiting for it: the same save, declared as a plain method.</summary>
    public void StartSaving() => Method(name: "save");

    /// <summary>Finds a row by its label, a moment later.</summary>
    public TypedSource<string> Lookup(Arg<string> label) => MethodAsync<string>(label);

    /// <summary>Refuses, by throwing.</summary>
    public void Fail() => Method();

    /// <summary>Refuses a moment later, by rejecting.</summary>
    public void FailLater() => MethodAsync();

    /// <summary>The card filled last, as data the slate keeps: its members are read, never called.</summary>
    public ReadOnly<SlateCard> LastCard => ReadOnly<SlateCard>();

    /// <summary>Every card filled, in the order the slate keeps them.</summary>
    public ReadOnly<IReadOnlyList<SlateCard>> Cards => ReadOnly<IReadOnlyList<SlateCard>>();

    /// <summary>How many rows the slate allows; nothing until a page sets it.</summary>
    public Property<int?> Limit => Property<int?>();

    /// <summary>When the slate was started; the slate keeps it as a timestamp, a number of milliseconds.</summary>
    public ReadOnly<DateTime> StartedAt => ReadOnly<DateTime>();

    /// <summary>Whether the slate has a limit: the limit the slate keeps, a number or nothing, read as a flag.</summary>
    public ReadOnly<bool> Limited => ReadOnly<bool>(name: "limit");

    /// <summary>Whether the slate holds a card: the list of cards the slate keeps, read as a flag.</summary>
    public ReadOnly<bool> HoldsCards => ReadOnly<bool>(name: "cards");

    /// <summary>Whether a line has been placed: the day the slate keeps of the line placed last, or nothing, read as a flag.</summary>
    public ReadOnly<bool> HasPlaced => ReadOnly<bool>(name: "placedOn");

    /// <summary>The flag of the line placed last, counted as one or none; the slate keeps it as true or false.</summary>
    public ReadOnly<int> PlacedFlags => ReadOnly<int>(name: "placedFlag");

    /// <summary>Writes one field of a card, named as the slate names it, with a value of the field's type.</summary>
    public void SetCard<TValue>(Arg<int> index, Field<TValue> field, Arg<TValue> value) => Method(index, field, value);

    /// <summary>Orders the cards by a field.</summary>
    public void SortCards(Field field) => Method(field);

    /// <summary>Adds a row the page asks for; the slate is always told it was the page.</summary>
    public void AddRowBy(Arg<string> label) => Method(label, Fixed("page"));

    /// <summary>Shows any list of labels as the summary line: the slate's method takes any list.</summary>
    public void SummarizeAny<TItems>(TypedSource<TItems> labels) where TItems : IEnumerable<object> =>
        Method(Pass<IEnumerable<object>, TItems>(labels), name: "summarize");

    /// <summary>The rows' labels, as the array the slate keeps them in.</summary>
    public ReadOnly<string[]> LabelArray => ReadOnly<string[]>(name: "labels");

    /// <summary>The rows' labels, declared as a list.</summary>
    public ReadOnly<List<string>> LabelList => ReadOnly<List<string>>(name: "labels");

    /// <summary>The rows' labels, declared as a sequence.</summary>
    public ReadOnly<IEnumerable<string>> LabelSequence => ReadOnly<IEnumerable<string>>(name: "labels");

    /// <summary>Raised when the slate has been cleared.</summary>
    public Event Cleared => Event();

    /// <summary>Raised when the slate's own Add row button has added a row.</summary>
    public Event<SlateRowAdded> RowAdded => Event<SlateRowAdded>();

    /// <summary>Raised before the title changes, when the slate's own retitle buttons ask for it; the slate reads the payload back.</summary>
    public Event<SlateChanging> Changing => Event<SlateChanging>();
}

public sealed class SlateRowAdded : JsObject
{
    public ReadOnly<string> Label => ReadOnly<string>();

    public ReadOnly<int> Count => ReadOnly<int>();
}

/// <summary>A title change the slate has been asked for, before it takes it.</summary>
public sealed class SlateChanging : JsObject
{
    public ReadOnly<string> Requested => ReadOnly<string>();

    public ReadOnly<string> Current => ReadOnly<string>();

    /// <summary>Written by a listener: the slate keeps its title.</summary>
    public Property<bool> Cancel => Property<bool>();

    /// <summary>Written by a listener: why, shown by the slate when it keeps its title.</summary>
    public Property<string> Reason => Property<string>();

    /// <summary>Shortens the requested title to its first word.</summary>
    public void Clamp() => Method();
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

    /// <summary>Raised by the header when it is renamed, with its new caption.</summary>
    public Event<SlateHeaderRenamed> Renamed => Event<SlateHeaderRenamed>();
}

public sealed class SlateHeaderRenamed : JsObject
{
    public ReadOnly<string> Caption => ReadOnly<string>();
}

public sealed class SlateRow : JsObject
{
    public ReadOnly<string> Label => ReadOnly<string>();

    public ReadOnly<bool> Marked => ReadOnly<bool>();

    public void Mark() => Method();
}
