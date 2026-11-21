using System;
using System.Collections.Generic;
using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class Raised : JsObject
{
    public ReadOnly<bool> Flag => ReadOnly<bool>();

    public ReadOnly<int> Count => ReadOnly<int>();

    public ReadOnly<string> Text => ReadOnly<string>();

    public ReadOnly<string?> NullableText => ReadOnly<string?>();

    public ReadOnly<string?> OptionalText => ReadOnly<string?>(optional: true);

    public ReadOnly<DateTime> Day => ReadOnly<DateTime>();

    public ReadOnly<TimeOnly> Time => ReadOnly<TimeOnly>();

    public ReadOnly<Record> Record => ReadOnly<Record>();

    public WriteOnly<string> WriteOnlyText => WriteOnly<string>();

    public Outer Outer => LiveObject<Outer>();

    public LiveObjects<Item> Items => LiveObjects<Item>();

    public void Act() => Method();

    public MethodResult<int> Total() => Method<int>();

    public MethodResult<string> Settle() => MethodAsync<string>();

    public Item Current() => ReturnedObject<Item>();

    public LiveObjects<Item> Matching() => ReturnedObjects<Item>();

    public Callback<Item, string> Answer => Callback<Item, string>();

    public Held Held => LiveObject<Held>();

    public ConformancePeer Peer => LiveObject<ConformancePeer>();

    public void Fill(Arg<IReadOnlyList<string>> names) => Method([names]);
}
