using System.Collections.Concurrent;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.World;

internal sealed class WorldData
{
    private const string Anywhere = "";

    private readonly ConcurrentQueue<string> _residents = new();
    private readonly ConcurrentDictionary<(string Gate, string At), WorldGate> _gates = new();
    private volatile string _word = "";
    private volatile string _componentText = "Alpha";
    private volatile string _componentOuterText = "Gamma";
    private volatile string _componentReadOnlyText = "RO-2026-11-10";
    private volatile string _componentCurrentItem = "One";
    private volatile int _heldNamedItems = 4;
    private volatile int _heldTotal = 14;
    private volatile int _componentOuterCount = 8;
    private volatile int _componentActiveItems = 3;
    private volatile int _componentCount = 3;
    private volatile ComponentVersion _componentVersion = ComponentVersion.Current;
    private volatile int _heldMatchingItems = 1;
    private volatile bool _textRefuses;
    private volatile bool _heldCountRefuses;
    private volatile bool _settleRefuses;
    private volatile bool _countRefuses;
    private volatile bool _itemRefuses;

    internal IReadOnlyCollection<string> Residents => _residents;

    internal string Word => _word;

    internal string ComponentText => _componentText;

    internal string ComponentOuterText => _componentOuterText;

    internal string ComponentReadOnlyText => _componentReadOnlyText;

    internal string ComponentCurrentItem => _componentCurrentItem;

    internal int HeldNamedItems => _heldNamedItems;

    internal int HeldTotal => _heldTotal;

    internal int ComponentOuterCount => _componentOuterCount;

    internal int ComponentActiveItems => _componentActiveItems;

    internal int ComponentCount => _componentCount;

    internal ComponentVersion ComponentVersion => _componentVersion;

    internal int HeldMatchingItems => _heldMatchingItems;

    internal bool TextRefuses => _textRefuses;

    internal bool HeldCountRefuses => _heldCountRefuses;

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
        if (seed.HeldNamedItems is { } heldNamedItems) _heldNamedItems = heldNamedItems;
        if (seed.HeldTotal is { } heldTotal) _heldTotal = heldTotal;
        if (seed.ComponentOuterCount is { } outerCount) _componentOuterCount = outerCount;
        if (seed.ComponentActiveItems is { } activeItems) _componentActiveItems = activeItems;
        if (seed.ComponentCount is { } count) _componentCount = count;
        if (seed.ComponentVersion is { } version) _componentVersion = version;
        if (seed.HeldMatchingItems is { } heldMatchingItems) _heldMatchingItems = heldMatchingItems;
        if (seed.TextRefuses is { } textRefuses) _textRefuses = textRefuses;
        if (seed.HeldCountRefuses is { } heldCountRefuses) _heldCountRefuses = heldCountRefuses;
        if (seed.SettleRefuses is { } settleRefuses) _settleRefuses = settleRefuses;
        if (seed.CountRefuses is { } countRefuses) _countRefuses = countRefuses;
        if (seed.ItemRefuses is { } itemRefuses) _itemRefuses = itemRefuses;
    }

    internal void Arm(GateOrder order) => _gates[(order.Gate, order.At)] = new WorldGate();

    internal Task Reach(string gate, string at)
    {
        var armed = NotYetHeld(gate, at) ?? NotYetHeld(gate, Anywhere);
        return armed is null ? Task.CompletedTask : armed.Park();
    }

    internal Task? Held(GateOrder order) => _gates.TryGetValue((order.Gate, order.At), out var armed) ? armed.Held : null;

    internal bool Release(GateOrder order)
    {
        if (!_gates.TryGetValue((order.Gate, order.At), out var armed) || !armed.IsParked) return false;

        armed.Release();
        return true;
    }

    internal IList<string> End()
    {
        var parked = _gates.Where(gate => gate.Value.IsParked).Select(gate => Named(gate.Key.Gate, gate.Key.At)).Order(StringComparer.Ordinal).ToList();
        foreach (var gate in _gates.Values) gate.EndWithItsWorld();
        return parked;
    }

    internal static string Named(string gate, string at) => at == Anywhere ? gate : $"{gate} at {at}";

    private WorldGate? NotYetHeld(string gate, string at) => _gates.TryGetValue((gate, at), out var armed) && !armed.Held.IsCompleted ? armed : null;
}
