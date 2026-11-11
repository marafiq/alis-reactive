namespace Alis.Reactive.Conformance.App.World;

internal sealed class CurrentWorld
{
    private string? _key;
    private WorldData? _data;

    internal string Key => _key ?? throw new InvalidOperationException("No world is open for this request: the world middleware runs before every World endpoint.");

    internal WorldData Data => _data ?? throw new InvalidOperationException("No world is open for this request: the world middleware runs before every World endpoint.");

    internal void Open(string key, WorldData data)
    {
        _key = key;
        _data = data;
    }
}
