using System.Collections.Generic;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class ConformanceStart
{
    public ComponentVersion Version { get; init; }

    public string? Text { get; init; } = "";

    public string? Value { get; init; }

    public IReadOnlyList<string> Options { get; init; } = [];

    public int Count { get; init; }

    public RecordStart Record { get; init; } = new();

    public decimal Amount { get; init; }

    public OuterStart Outer { get; init; } = new();

    public HeldStart Held { get; init; } = new();

    public string ReadOnlyText { get; init; } = "";

    public string? CurrentItem { get; init; }

    public bool CurrentItemLacksAct { get; init; }

    public bool CurrentItemLacksValue { get; init; }

    public bool Enabled { get; init; } = true;

    public StartsStart Starts { get; init; } = new();

    public IReadOnlyList<BindingStart> Bindings { get; init; } = [];

    public IReadOnlyList<ItemStart> Items { get; init; } = [];

    public IReadOnlyList<ItemStart>? NullableItems { get; init; }

    public IReadOnlyList<RaiseStart> Raises { get; init; } = [];

    public IReadOnlyList<RecordStart> Records { get; init; } = [];

    public string? BuiltBy { get; init; }

    public string SettleEndpoint { get; init; } = WorldEndpoints.Settle;

    public string CountEndpoint { get; init; } = WorldEndpoints.Count;

    public string ItemEndpoint { get; init; } = WorldEndpoints.Item;

    public string TextEndpoint { get; init; } = WorldEndpoints.Text;

    public string HeldCountEndpoint { get; init; } = WorldEndpoints.HeldCount;

    public string RecordEndpoint { get; init; } = WorldEndpoints.Record;
}
