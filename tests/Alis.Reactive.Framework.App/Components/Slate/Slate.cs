namespace Alis.Reactive.Framework.App.Components;

public sealed class Slate : Component
{
    public static ComponentVendor SlateVendor { get; } = ComponentVendor.Define(
        "slate",
        objectPath: "slate",
        subscribe: "on",
        unsubscribe: "off",
        payload: VendorEventPayload.Argument);

    public override ComponentVendor Vendor => SlateVendor;

    public ReadOnly<int> Count => ReadOnly<int>();

    public WriteOnly<string> Note => WriteOnly<string>();

    public Property<string> Title => Property<string>();

    public SlateHeader Header => Object<SlateHeader>();

    public Objects<SlateRow> Rows => Objects<SlateRow>();

    public SlateRow RowAt(Arg<int> index) => Object<SlateRow>(index);

    public SlateRow RowFrom(Arg<int> index, Arg<bool> fromLast) => Object<SlateRow>(index, fromLast);

    public Objects<SlateRow> MarkedRows() => Objects<SlateRow>();

    public TypedSource<string> LabelsFrom(Arg<int> start, Arg<int> count) => Method<string>(start, count);

    public TypedSource<string> LabelsJoined(Arg<int> start, Arg<int> count, Arg<string> separator) => Method<string>(start, count, separator);

    public void Summarize(Arg<IReadOnlyList<string>> labels) => Method(labels);

    public void Clear() => Method();

    public void Place(Arg<string> text, Arg<decimal> number, Arg<bool> flag, Arg<DateTime> day) => Method(text, number, flag, day);

    public void Fill(Arg<SlateCard> card) => Method(card);

    public TypedSource<bool> Has(Arg<string> label) => Method<bool>(label);

    public TypedSource<decimal> Total() => Method<decimal>();

    public ReadOnly<string> Status => ReadOnly<string>();

    public void Save() => MethodAsync();

    public TypedSource<int> SaveRows() => MethodAsync<int>();

    public void StartSaving() => Method(name: "save");

    public TypedSource<string> Lookup(Arg<string> label) => MethodAsync<string>(label);

    public void Fail() => Method();

    public void FailLater() => MethodAsync();

    public ReadOnly<SlateCard> LastCard => ReadOnly<SlateCard>();

    public ReadOnly<IReadOnlyList<SlateCard>> Cards => ReadOnly<IReadOnlyList<SlateCard>>();

    public Property<int?> Limit => Property<int?>();

    public ReadOnly<DateTime> StartedAt => ReadOnly<DateTime>();

    public ReadOnly<bool> Limited => ReadOnly<bool>(name: "limit");

    public ReadOnly<bool> HoldsCards => ReadOnly<bool>(name: "cards");

    public ReadOnly<bool> HasPlaced => ReadOnly<bool>(name: "placedOn");

    public ReadOnly<int> PlacedFlags => ReadOnly<int>(name: "placedFlag");

    public void SetCard<TValue>(Arg<int> index, Field<TValue> field, Arg<TValue> value) => Method(index, field, value);

    public void SortCards(Field field) => Method(field);

    public void AddRowBy(Arg<string> label) => Method(label, Fixed("page"));

    public void SummarizeAny<TItems>(TypedSource<TItems> labels) where TItems : IEnumerable<object> =>
        Method(Pass<IEnumerable<object>, TItems>(labels), name: "summarize");

    public ReadOnly<string[]> LabelArray => ReadOnly<string[]>(name: "labels");

    public ReadOnly<List<string>> LabelList => ReadOnly<List<string>>(name: "labels");

    public ReadOnly<IEnumerable<string>> LabelSequence => ReadOnly<IEnumerable<string>>(name: "labels");

    public Event Cleared => Event();

    public Event<SlateRowAdded> RowAdded => Event<SlateRowAdded>();

    public Event<SlateChanging> Changing => Event<SlateChanging>();
}

public sealed class SlateRowAdded : JsObject
{
    public ReadOnly<string> Label => ReadOnly<string>();

    public ReadOnly<int> Count => ReadOnly<int>();
}

public sealed class SlateChanging : JsObject
{
    public ReadOnly<string> Requested => ReadOnly<string>();

    public ReadOnly<string> Current => ReadOnly<string>();

    public Property<bool> Cancel => Property<bool>();

    public Property<string> Reason => Property<string>();

    public void Clamp() => Method();
}

public sealed class SlateCard
{
    public string Name { get; set; } = "";

    public DateTime Due { get; set; }
}

public sealed class SlateHeader : JsObject
{
    public Property<string> Caption => Property<string>();

    public void Rename(Arg<string> caption) => Method(caption);

    public Event<SlateHeaderRenamed> Renamed => Event<SlateHeaderRenamed>();
}

public sealed class SlateHeaderRenamed : JsObject
{
    public ReadOnly<string> Caption => ReadOnly<string>();
}

public sealed class SlateRow : JsObject
{
    public ReadOnly<string> Label => ReadOnly<string>();

    public void Mark() => Method();
}
