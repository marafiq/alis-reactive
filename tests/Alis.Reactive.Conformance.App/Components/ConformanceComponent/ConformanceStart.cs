using System.Collections.Generic;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class ConformanceStart
{
    public ComponentVersion Version { get; init; }

    public string Text { get; init; } = "";

    public string? Value { get; init; }

    public int Count { get; init; }

    public decimal Amount { get; init; }

    public OuterStart Outer { get; init; } = new();

    public HeldStart Held { get; init; } = new();

    public string ReadOnlyText { get; init; } = "";

    public string? CurrentItem { get; init; }

    public IReadOnlyList<ItemStart> Items { get; init; } = [];

    public string SettleEndpoint { get; init; } = WorldEndpoints.Settle;

    public string CountEndpoint { get; init; } = WorldEndpoints.Count;

    public string ItemEndpoint { get; init; } = WorldEndpoints.Item;

    public string TextEndpoint { get; init; } = WorldEndpoints.Text;

    public string HeldCountEndpoint { get; init; } = WorldEndpoints.HeldCount;
}
