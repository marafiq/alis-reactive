using System;
using System.Collections.Generic;
using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.ValuesAndSources;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class ConformanceComponent : Component, IInputComponent
{
    public static ComponentVendor Declared { get; } = ComponentVendor.Define("conformance-component", objectPath: "conformanceComponent", subscribe: "addEventListener", unsubscribe: "removeEventListener");

    public override ComponentVendor Vendor => Declared;

    public Property<string?> Value => Property<string?>();

    public Property<string> Text => Property<string>();

    public Property<int> Count => Property<int>();

    public Property<decimal> Amount => Property<decimal>();

    public Property<double> Ratio => Property<double>();

    public Property<bool> Enabled => Property<bool>();

    public Property<DateTime> Day => Property<DateTime>();

    public Property<DateTime?> NullableDay => Property<DateTime?>();

    public Property<DateTimeOffset> Moment => Property<DateTimeOffset>();

    public Property<Choice> Choice => Property<Choice>();

    public Property<CareDays> Days => Property<CareDays>();

    public Property<int?> NullableCount => Property<int?>();

    public Property<string?> OptionalText => Property<string?>(optional: true);

    public Property<Record> Record => Property<Record>();

    public Property<List<Record>> Records => Property<List<Record>>();

    public Property<List<Record>?> NullableRecords => Property<List<Record>?>();

    public Property<List<Binding>> Bindings => Property<List<Binding>>();

    public Property<Binding> Binding => Property<Binding>();

    public Property<Starts> Starts => Property<Starts>();

    public ReadOnly<string> ReadOnlyText => ReadOnly<string>();

    public ReadOnly<int> ReadOnlyCount => ReadOnly<int>();

    public ReadOnly<int> Calls => ReadOnly<int>();

    public ReadOnly<bool> IsDestroyed => ReadOnly<bool>();

    public WriteOnly<string> WriteOnlyText => WriteOnly<string>();

    public Outer Outer => LiveObject<Outer>();

    public Outer NullableOuter => LiveObject<Outer>(nullable: true);

    public LiveObjects<Item> Items => LiveObjects<Item>();

    public LiveObjects<Item> NullableItems => LiveObjects<Item>(nullable: true);

    public Callback<Item, string> Answer => Callback<Item, string>();

    public Callback<PairArgs, string> Pair => Callback<PairArgs, string>();

    public void Ping() => Method();

    public MethodResult<int> Total() => Method<int>();

    public MethodResult<bool> Check() => Method<bool>();

    public MethodResult<string> Echo(Arg<string> text) => Method<string>([text]);

    public MethodResult<Record> Snapshot() => Method<Record>();

    public Item Current() => ReturnedObject<Item>();

    public Item MaybeItem() => ReturnedObject<Item>(nullable: true);

    public LiveObjects<Item> Matching() => ReturnedObjects<Item>();

    public ConformanceComponent Self() => ReturnedObject<ConformanceComponent>();

    public void Settle() => MethodAsync();

    public MethodResult<int> SettleCount() => MethodAsync<int>();

    public MethodResult<bool> SettleCheck() => MethodAsync<bool>();

    public MethodResult<Record> SettleRecord() => MethodAsync<Record>();

    public MethodResult<Record?> SettleMaybe() => MethodAsync<Record?>();

    public void Log(Arg<string> text) => Method([text]);

    public void Add(Arg<decimal> amount) => Method([amount]);

    public void TakeNullable(Arg<int?> count) => Method([count]);

    public void TakeText(Arg<string?> text) => Method([text]);

    public void TakeDay(Arg<DateTime> day) => Method([day]);

    public void TakeMoment(Arg<DateTimeOffset> moment) => Method([moment]);

    public void Take(Arg<Record> record) => Method([record]);

    public void TakeAll(Arg<IReadOnlyList<Record>> records) => Method([records]);

    public void TakeDays(Arg<IReadOnlyList<DateTime>> days) => Method([days]);

    public void TakeLinked(Arg<Linked> linked) => Method([linked]);

    public void SortBy(Arg<Field> field) => Method([field]);

    public void SortItems(Func<Item, TypedSource<int>> key) => Method([Answer(key)]);

    public void Destroy() => Method();

    public Event<Change> Change => Event<Change>();

    public Event<Change> Commit => Event<Change>();

    public Event<Raised> Raised => Event<Raised>();

    public Event<ConformancePeer> Handed => Event<ConformancePeer>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);
}
