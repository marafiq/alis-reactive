using System.Collections.Concurrent;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.World;

internal sealed class WorldData
{
    private readonly ConcurrentQueue<string> _residents = new();
    private readonly ConcurrentDictionary<string, WorldGate> _gates = new(StringComparer.Ordinal);
    private volatile string _word = "";
    private volatile string _componentText = "Alpha";
    private volatile string _componentOuterText = "Gamma";
    private volatile string _componentReadOnlyText = "RO-2026-11-10";
    private volatile string _componentCurrentItem = "One";
    private volatile int _componentOuterCount = 8;
    private volatile int _componentActiveItems = 3;
    private volatile int _componentCount = 3;
    private volatile ComponentVersion _componentVersion = ComponentVersion.Current;
    private volatile bool _settleRefuses;
    private volatile bool _countRefuses;
    private volatile bool _itemRefuses;

    internal IReadOnlyCollection<string> Residents => _residents;

    internal string Word => _word;

    internal string ComponentText => _componentText;

    internal string ComponentOuterText => _componentOuterText;

    internal string ComponentReadOnlyText => _componentReadOnlyText;

    internal string ComponentCurrentItem => _componentCurrentItem;

    internal int ComponentOuterCount => _componentOuterCount;

    internal int ComponentActiveItems => _componentActiveItems;

    internal int ComponentCount => _componentCount;

    internal ComponentVersion ComponentVersion => _componentVersion;

    internal bool SettleRefuses => _settleRefuses;

    internal bool CountRefuses => _countRefuses;

    internal bool ItemRefuses => _itemRefuses;

    internal void Take(Seed seed)
    {
        foreach (var resident in seed.Residents) _residents.Enqueue(resident);
        if (seed.Word is { } word) _word = word;
        if (seed.ComponentText is { } text) _componentText = text;
        if (seed.ComponentOuterText is { } outerText) _componentOuterText = outerText;
        if (seed.ComponentReadOnlyText is { } readOnlyText) _componentReadOnlyText = readOnlyText;
        if (seed.ComponentCurrentItem is { } currentItem) _componentCurrentItem = currentItem;
        if (seed.ComponentOuterCount is { } outerCount) _componentOuterCount = outerCount;
        if (seed.ComponentActiveItems is { } activeItems) _componentActiveItems = activeItems;
        if (seed.ComponentCount is { } count) _componentCount = count;
        if (seed.ComponentVersion is { } version) _componentVersion = version;
        if (seed.SettleRefuses is { } settleRefuses) _settleRefuses = settleRefuses;
        if (seed.CountRefuses is { } countRefuses) _countRefuses = countRefuses;
        if (seed.ItemRefuses is { } itemRefuses) _itemRefuses = itemRefuses;
    }

    internal void Arm(string gate) => _gates[gate] = new WorldGate();

    internal Task Reach(string gate) => _gates.TryGetValue(gate, out var armed) && !armed.Held.IsCompleted ? armed.Park() : Task.CompletedTask;

    internal Task? Held(string gate) => _gates.TryGetValue(gate, out var armed) ? armed.Held : null;

    internal bool Release(string gate)
    {
        if (!_gates.TryGetValue(gate, out var armed) || !armed.IsParked) return false;

        armed.Release();
        return true;
    }

    internal IList<string> End()
    {
        var parked = _gates.Where(gate => gate.Value.IsParked).Select(gate => gate.Key).Order(StringComparer.Ordinal).ToList();
        foreach (var gate in _gates.Values) gate.EndWithItsWorld();
        return parked;
    }
}
