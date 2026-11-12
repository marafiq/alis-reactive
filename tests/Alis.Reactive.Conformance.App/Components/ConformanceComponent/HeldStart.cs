using System.Collections.Generic;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class HeldStart
{
    public IReadOnlyList<ItemStart> Items { get; init; } = [];

    public int Total { get; init; }

    public int MatchingItems { get; init; }
}
