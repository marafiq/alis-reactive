using System.Collections.Concurrent;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.World;

internal sealed class WorldData
{
    private readonly ConcurrentQueue<string> _residents = new();
    private readonly ConcurrentDictionary<string, WorldGate> _gates = new(StringComparer.Ordinal);
    private volatile string _word = "";

    internal IReadOnlyCollection<string> Residents => _residents;

    internal string Word => _word;

    internal void Take(Seed seed)
    {
        foreach (var resident in seed.Residents) _residents.Enqueue(resident);
        if (seed.Word is { } word) _word = word;
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
