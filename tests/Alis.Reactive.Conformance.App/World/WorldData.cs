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

    internal IReadOnlyCollection<string> Residents => _residents;

    internal string Word => _word;

    internal string ComponentText => _componentText;

    internal string ComponentOuterText => _componentOuterText;

    internal string ComponentReadOnlyText => _componentReadOnlyText;

    internal string ComponentCurrentItem => _componentCurrentItem;

    internal void Take(Seed seed)
    {
        foreach (var resident in seed.Residents) _residents.Enqueue(resident);
        if (seed.Word is { } word) _word = word;
        if (seed.ComponentText is { } text) _componentText = text;
        if (seed.ComponentOuterText is { } outerText) _componentOuterText = outerText;
        if (seed.ComponentReadOnlyText is { } readOnlyText) _componentReadOnlyText = readOnlyText;
        if (seed.ComponentCurrentItem is { } currentItem) _componentCurrentItem = currentItem;
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
