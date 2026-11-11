using System.Collections.Concurrent;

namespace Alis.Reactive.Conformance.App.World;

internal sealed class Worlds
{
    private readonly ConcurrentDictionary<string, WorldData> _byKey = new(StringComparer.Ordinal);

    internal WorldData Of(string key) => _byKey.GetOrAdd(key, _ => new WorldData());

    internal IList<string> End(string key) => _byKey.TryRemove(key, out var world) ? world.End() : [];
}
