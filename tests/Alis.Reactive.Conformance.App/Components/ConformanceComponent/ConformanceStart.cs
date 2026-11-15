using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class ConformanceStart
{
    [JsonPropertyName("version")]
    public ComponentVersion Version { get; init; }

    [JsonPropertyName("text")]
    public string? Text { get; init; } = "";

    [JsonPropertyName("value")]
    public string? Value { get; init; }

    [JsonPropertyName("options")]
    public IReadOnlyList<string> Options { get; init; } = [];

    [JsonPropertyName("count")]
    public int Count { get; init; }

    [JsonPropertyName("record")]
    public RecordStart Record { get; init; } = new();

    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    [JsonPropertyName("outer")]
    public OuterStart Outer { get; init; } = new();

    [JsonPropertyName("held")]
    public HeldStart Held { get; init; } = new();

    [JsonPropertyName("readOnlyText")]
    public string ReadOnlyText { get; init; } = "";

    [JsonPropertyName("currentItem")]
    public string? CurrentItem { get; init; }

    [JsonPropertyName("currentItemLacksAct")]
    public bool CurrentItemLacksAct { get; init; }

    [JsonPropertyName("currentItemLacksValue")]
    public bool CurrentItemLacksValue { get; init; }

    [JsonPropertyName("bindingCountPending")]
    public bool BindingCountPending { get; init; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; init; } = true;

    [JsonPropertyName("starts")]
    public StartsStart Starts { get; init; } = new();

    [JsonPropertyName("bindings")]
    public IReadOnlyList<BindingStart> Bindings { get; init; } = [];

    [JsonPropertyName("items")]
    public IReadOnlyList<ItemStart> Items { get; init; } = [];

    [JsonPropertyName("nullableItems")]
    public IReadOnlyList<ItemStart>? NullableItems { get; init; }

    [JsonPropertyName("raises")]
    public IReadOnlyList<RaiseStart> Raises { get; init; } = [];

    [JsonPropertyName("records")]
    public IReadOnlyList<RecordStart> Records { get; init; } = [];

    [JsonPropertyName("builtBy")]
    public string? BuiltBy { get; init; }

    [JsonPropertyName("settleEndpoint")]
    public string SettleEndpoint { get; init; } = WorldEndpoints.Settle;

    [JsonPropertyName("countEndpoint")]
    public string CountEndpoint { get; init; } = WorldEndpoints.Count;

    [JsonPropertyName("itemEndpoint")]
    public string ItemEndpoint { get; init; } = WorldEndpoints.Item;

    [JsonPropertyName("textEndpoint")]
    public string TextEndpoint { get; init; } = WorldEndpoints.Text;

    [JsonPropertyName("heldCountEndpoint")]
    public string HeldCountEndpoint { get; init; } = WorldEndpoints.HeldCount;

    [JsonPropertyName("recordEndpoint")]
    public string RecordEndpoint { get; init; } = WorldEndpoints.Record;

    [JsonPropertyName("checkEndpoint")]
    public string CheckEndpoint { get; init; } = WorldEndpoints.Check;
}
